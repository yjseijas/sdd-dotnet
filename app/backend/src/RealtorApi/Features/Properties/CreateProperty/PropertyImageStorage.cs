using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed record StoredPropertyImage(
    string FileName,
    string PublicPath,
    string PhysicalPath);

public interface IPropertyImageStorage
{
    Task<Result<StoredPropertyImage>> StoreAsync(
        IFormFile file,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        StoredPropertyImage image,
        CancellationToken cancellationToken);
}

public sealed class PropertyImageStorage(
    IWebHostEnvironment environment,
    ILogger<PropertyImageStorage> logger)
    : IPropertyImageStorage
{
    public const long MaxFileBytes = 5_242_880;
    public const long MaxDecodedMemoryBytes = 268_435_456;
    private const int CopyBufferSize = 81_920;

    public async Task<Result<StoredPropertyImage>> StoreAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length > MaxFileBytes)
        {
            return InvalidImage(
                StatusCodes.Status413PayloadTooLarge,
                "El archivo de imagen supera el límite de 5.242.880 bytes.",
                "property.image_too_large");
        }

        if (file.Length <= 0)
        {
            return InvalidImage(
                StatusCodes.Status400BadRequest,
                "La imagen está vacía o no es válida.",
                "property.image_invalid");
        }

        await using var source = file.OpenReadStream();
        await using var buffer = new MemoryStream(capacity: (int)Math.Min(file.Length, MaxFileBytes));
        var bufferBytes = new byte[CopyBufferSize];
        long totalBytes = 0;

        while (true)
        {
            var bytesRead = await source.ReadAsync(bufferBytes.AsMemory(), cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            totalBytes += bytesRead;
            if (totalBytes > MaxFileBytes)
            {
                return InvalidImage(
                    StatusCodes.Status413PayloadTooLarge,
                    "El archivo de imagen supera el límite de 5.242.880 bytes.",
                    "property.image_too_large");
            }

            await buffer.WriteAsync(bufferBytes.AsMemory(0, bytesRead), cancellationToken);
        }

        if (totalBytes == 0)
        {
            return InvalidImage(
                StatusCodes.Status400BadRequest,
                "La imagen está vacía o no es válida.",
                "property.image_invalid");
        }

        buffer.Position = 0;

        ImageInfo imageInfo;
        try
        {
            imageInfo = await Image.IdentifyAsync(buffer, cancellationToken);
        }
        catch (ImageFormatException)
        {
            return InvalidImage(
                StatusCodes.Status400BadRequest,
                "La imagen está vacía o no es válida.",
                "property.image_invalid");
        }

        var format = imageInfo.Metadata.DecodedImageFormat;
        var extension = GetValidatedExtension(format, file.FileName, file.ContentType);
        if (extension is null)
        {
            return InvalidImage(
                StatusCodes.Status400BadRequest,
                "El contenido, la extensión y el tipo MIME de la imagen deben corresponder a PNG o JPEG.",
                "property.image_invalid");
        }

        if (!IsWithinDecodedMemoryLimit(imageInfo.GetPixelMemorySize()))
        {
            return InvalidImage(
                StatusCodes.Status400BadRequest,
                "La imagen supera el presupuesto de memoria decodificada de 268.435.456 bytes.",
                "property.image_memory_limit_exceeded");
        }

        buffer.Position = 0;
        try
        {
            using var decodedImage = await Image.LoadAsync(buffer, cancellationToken);
        }
        catch (ImageFormatException)
        {
            return InvalidImage(
                StatusCodes.Status400BadRequest,
                "La imagen está vacía o no es válida.",
                "property.image_invalid");
        }

        var directory = Path.Combine(
            environment.ContentRootPath,
            "wwwroot",
            "assets",
            "properties");
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.GetFullPath(Path.Combine(directory, fileName));
        var fileCreated = false;

        try
        {
            await using (var destination = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                CopyBufferSize,
                FileOptions.Asynchronous))
            {
                fileCreated = true;
                buffer.Position = 0;
                await buffer.CopyToAsync(destination, CopyBufferSize, cancellationToken);
            }

            var relativePath = $"/assets/properties/{fileName}";
            return new StoredPropertyImage(fileName, relativePath, filePath);
        }
        catch
        {
            if (fileCreated)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception cleanupException)
                {
                    logger.LogError(
                        cleanupException,
                        "Failed to remove incomplete property image {ImageFileName}.",
                        fileName);
                }
            }

            throw;
        }
    }

    public Task DeleteAsync(
        StoredPropertyImage image,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var expectedDirectory = Path.GetFullPath(Path.Combine(
            environment.ContentRootPath,
            "wwwroot",
            "assets",
            "properties"));
        var expectedPath = Path.GetFullPath(Path.Combine(expectedDirectory, image.FileName));
        var actualPath = Path.GetFullPath(image.PhysicalPath);
        if (!string.Equals(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The image path is outside the property image directory.");
        }

        File.Delete(actualPath);
        return Task.CompletedTask;
    }

    public static bool IsWithinDecodedMemoryLimit(long estimatedBytes)
    {
        return estimatedBytes <= MaxDecodedMemoryBytes;
    }

    private static string? GetValidatedExtension(
        IImageFormat? format,
        string fileName,
        string contentType)
    {
        var declaredExtension = Path.GetExtension(fileName);
        var declaredContentType = contentType.Split(';', 2)[0].Trim();

        if (string.Equals(format?.Name, "PNG", StringComparison.OrdinalIgnoreCase)
            && string.Equals(declaredExtension, ".png", StringComparison.OrdinalIgnoreCase)
            && string.Equals(declaredContentType, "image/png", StringComparison.OrdinalIgnoreCase))
        {
            return ".png";
        }

        if (string.Equals(format?.Name, "JPEG", StringComparison.OrdinalIgnoreCase)
            && (string.Equals(declaredExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(declaredExtension, ".jpeg", StringComparison.OrdinalIgnoreCase))
            && string.Equals(declaredContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return ".jpg";
        }

        return null;
    }

    private static Result<StoredPropertyImage> InvalidImage(
        int statusCode,
        string detail,
        string code)
    {
        return Result<StoredPropertyImage>.Failure(
            statusCode,
            statusCode == StatusCodes.Status413PayloadTooLarge
                ? "Payload Too Large"
                : "Invalid image",
            detail,
            code);
    }
}
