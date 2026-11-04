using Microsoft.EntityFrameworkCore;
using RealtorApi.Features.Properties.CreateProperty;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.UpdateProperty;

public sealed class UpdatePropertyHandler(
    AppDbContext dbContext,
    IPropertyImageStorage imageStorage,
    ILogger<UpdatePropertyHandler> logger)
    : IHandler<UpdatePropertyRequest, UpdatePropertyResponse>
{
    public async Task<Result<UpdatePropertyResponse>> HandleAsync(
        UpdatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var validation = new UpdatePropertyValidator().Validate(request);
        if (!validation.IsValid)
        {
            var firstError = validation.Errors.First();
            return Result<UpdatePropertyResponse>.Failure(
                StatusCodes.Status400BadRequest,
                "Validation Error",
                firstError.ErrorMessage,
                $"property.{firstError.PropertyName}");
        }

        if (request.Id is null || request.Id == Guid.Empty)
        {
            return Result<UpdatePropertyResponse>.Failure(
                StatusCodes.Status400BadRequest,
                "Bad Request",
                "El identificador de la propiedad no es válido.",
                "property.id_invalid");
        }

        var property = await dbContext.Properties
            .SingleOrDefaultAsync(item => item.Id == request.Id.Value, cancellationToken);

        if (property is null)
        {
            return Result<UpdatePropertyResponse>.Failure(
                StatusCodes.Status404NotFound,
                "Not Found",
                "La propiedad no existe.",
                "property.not_found");
        }

        StoredPropertyImage? storedImage = null;
        var previousImageUrl = property.ImageUrl;
        if (request.Image is not null)
        {
            if (request.Image.Length > PropertyImageStorage.MaxFileBytes)
            {
                return Result<UpdatePropertyResponse>.Failure(
                    StatusCodes.Status413PayloadTooLarge,
                    "Payload Too Large",
                    "El archivo de imagen supera el límite de 5.242.880 bytes.",
                    "property.image_too_large");
            }

            try
            {
                var storeResult = await imageStorage.StoreAsync(request.Image, cancellationToken);
                if (!storeResult.IsSuccess)
                {
                    var error = storeResult.Error!;
                    var statusCode = error.StatusCode == StatusCodes.Status400BadRequest
                        && error.Code == "property.image_invalid"
                        ? StatusCodes.Status415UnsupportedMediaType
                        : error.StatusCode;

                    return Result<UpdatePropertyResponse>.Failure(
                        statusCode,
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
                logger.LogError(exception, "Failed to store the replacement image for property {PropertyId}.", property.Id);
                return Result<UpdatePropertyResponse>.Failure(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "No se pudo almacenar la imagen nueva.",
                    "property.image_storage_failed");
            }
        }

        request.ApplyUpdates(property);
        if (storedImage is not null)
        {
            property.ImageUrl = storedImage.PublicPath;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to update property {PropertyId}.", property.Id);
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
                        "Failed to remove uploaded property image {ImageFileName} after a persistence failure.",
                        storedImage.FileName);
                }
            }

            return Result<UpdatePropertyResponse>.Failure(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "No se pudo actualizar la propiedad.",
                "property.persistence_failed");
        }

        if (storedImage is not null
            && !string.IsNullOrWhiteSpace(previousImageUrl)
            && !string.Equals(previousImageUrl, storedImage.PublicPath, StringComparison.OrdinalIgnoreCase))
        {
            var hasOtherReference = await dbContext.Properties
                .AnyAsync(item => item.Id != property.Id && item.ImageUrl == previousImageUrl, cancellationToken);

            if (!hasOtherReference)
            {
                try
                {
                    await imageStorage.DeleteIfManagedAsync(previousImageUrl, cancellationToken);
                }
                catch (Exception cleanupException)
                {
                    logger.LogError(
                        cleanupException,
                        "Failed to delete previous property image {ImageUrl} for property {PropertyId}.",
                        previousImageUrl,
                        property.Id);
                }
            }
        }

        return property.ToResponse();
    }
}
