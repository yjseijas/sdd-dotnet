using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed class CreatePropertyHandler(
    AppDbContext dbContext,
    IPropertyImageStorage imageStorage,
    ILogger<CreatePropertyHandler> logger)
    : IHandler<CreatePropertyRequest, CreatePropertyResponse>
{
    public async Task<Result<CreatePropertyResponse>> HandleAsync(
        CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Image is { Length: > 5_242_880 })
        {
            return Result<CreatePropertyResponse>.Failure(
                StatusCodes.Status413PayloadTooLarge,
                "Payload Too Large",
                "El archivo de imagen supera el límite de 5.242.880 bytes.",
                "property.image_too_large");
        }

        StoredPropertyImage? storedImage = null;
        if (request.Image is not null)
        {
            try
            {
                var storeResult = await imageStorage.StoreAsync(request.Image, cancellationToken);
                if (!storeResult.IsSuccess)
                {
                    var error = storeResult.Error!;
                    return Result<CreatePropertyResponse>.Failure(
                        error.StatusCode,
                        error.Title,
                        error.Detail,
                        error.Code);
                }

                storedImage = storeResult.Value;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to store the image for a new property.");
                return Result<CreatePropertyResponse>.Failure(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "No se pudo almacenar la imagen.",
                    "property.image_storage_failed");
            }
        }

        var property = request.ToEntity();
        property.Id = Guid.NewGuid();
        property.ImageUrl = storedImage?.PublicPath;
        property.CreatedAt = DateTime.UtcNow;
        property.UpdatedAt = null;

        try
        {
            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync(cancellationToken);

            return property.ToResponse();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to persist property {PropertyId}.", property.Id);
            if (storedImage is not null)
            {
                try
                {
                    await imageStorage.DeleteAsync(storedImage, CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    logger.LogError(
                        cleanupException,
                        "Failed to remove property image {ImageFileName} after persistence failure.",
                        storedImage.FileName);
                }
            }

            return Result<CreatePropertyResponse>.Failure(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "No se pudo crear la propiedad.",
                "property.persistence_failed");
        }
    }
}
