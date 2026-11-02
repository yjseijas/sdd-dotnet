using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using RealtorApi.Features.Properties.CreateProperty;

namespace RealtorApiTests.Properties.CreateProperty;

public sealed class PropertyImageStorageTests
{
    [Fact]
    public async Task StoreAsync_WhenPngIsValid_StoresOriginalWithGeneratedName()
    {
        using var directory = new TemporaryDirectory();
        var storage = CreateStorage(directory.Path);
        var formFile = CreateFile(CreateEncodedImage(png: true), "client-name.png", "image/png");

        var result = await storage.StoreAsync(formFile, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var storedImage = Assert.IsType<StoredPropertyImage>(result.Value);
        Assert.EndsWith(".png", storedImage.FileName, StringComparison.Ordinal);
        Assert.NotEqual("client-name.png", storedImage.FileName);
        Assert.Equal($"/assets/properties/{storedImage.FileName}", storedImage.PublicPath);
        Assert.True(File.Exists(storedImage.PhysicalPath));
        Assert.Equal(
            CreateEncodedImage(png: true),
            await File.ReadAllBytesAsync(storedImage.PhysicalPath));
    }

    [Fact]
    public async Task StoreAsync_WhenJpegIsValid_AcceptsJpegExtensionAndMime()
    {
        using var directory = new TemporaryDirectory();
        var storage = CreateStorage(directory.Path);
        var formFile = CreateFile(CreateEncodedImage(png: false), "photo.JPEG", "image/jpeg");

        var result = await storage.StoreAsync(formFile, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.EndsWith(".jpg", result.Value!.FileName, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("photo.jpg", "image/png")]
    [InlineData("photo.gif", "image/gif")]
    [InlineData("photo.png", "image/jpeg")]
    public async Task StoreAsync_WhenExtensionOrMimeDoesNotMatch_RejectsImage(
        string fileName,
        string contentType)
    {
        using var directory = new TemporaryDirectory();
        var storage = CreateStorage(directory.Path);
        var formFile = CreateFile(CreateEncodedImage(png: true), fileName, contentType);

        var result = await storage.StoreAsync(formFile, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Error!.StatusCode);
        Assert.False(Directory.EnumerateFiles(directory.Path, "*", SearchOption.AllDirectories).Any());
    }

    [Fact]
    public async Task StoreAsync_WhenFileIsCorrupt_RejectsImage()
    {
        using var directory = new TemporaryDirectory();
        var storage = CreateStorage(directory.Path);
        var formFile = CreateFile([1, 2, 3, 4], "photo.png", "image/png");

        var result = await storage.StoreAsync(formFile, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Error!.StatusCode);
    }

    [Fact]
    public async Task StoreAsync_WhenFileIsEmpty_RejectsImage()
    {
        using var directory = new TemporaryDirectory();
        var storage = CreateStorage(directory.Path);
        var formFile = CreateFile([], "empty.png", "image/png");

        var result = await storage.StoreAsync(formFile, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Error!.StatusCode);
    }

    [Fact]
    public async Task StoreAsync_WhenFileIsExactlyFiveMib_AcceptsIt()
    {
        using var directory = new TemporaryDirectory();
        var storage = CreateStorage(directory.Path);
        var image = CreateEncodedImage(png: true);
        var paddedImage = new byte[PropertyImageStorage.MaxFileBytes];
        image.CopyTo(paddedImage, 0);
        var formFile = CreateFile(paddedImage, "max.png", "image/png");

        var result = await storage.StoreAsync(formFile, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PropertyImageStorage.MaxFileBytes, new FileInfo(result.Value!.PhysicalPath).Length);
    }

    [Fact]
    public async Task StoreAsync_WhenFileExceedsFiveMib_RejectsBeforeWriting()
    {
        using var directory = new TemporaryDirectory();
        var storage = CreateStorage(directory.Path);
        var overLimitImage = new byte[PropertyImageStorage.MaxFileBytes + 1];
        var formFile = CreateFile(overLimitImage, "large.png", "image/png");

        var result = await storage.StoreAsync(formFile, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, result.Error!.StatusCode);
        Assert.False(Directory.Exists(Path.Combine(directory.Path, "wwwroot", "assets", "properties")));
    }

    [Fact]
    public void IsWithinDecodedMemoryLimit_IncludesExactLimitAndRejectsLargerValues()
    {
        Assert.True(PropertyImageStorage.IsWithinDecodedMemoryLimit(268_435_456));
        Assert.False(PropertyImageStorage.IsWithinDecodedMemoryLimit(268_435_457));
    }

    private static PropertyImageStorage CreateStorage(string contentRoot)
    {
        var environment = new TestWebHostEnvironment(contentRoot);
        return new PropertyImageStorage(environment, NullLogger<PropertyImageStorage>.Instance);
    }

    private static FormFile CreateFile(byte[] bytes, string fileName, string contentType)
    {
        var headers = new HeaderDictionary
        {
            ["Content-Type"] = contentType
        };
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "image", fileName)
        {
            Headers = headers
        };
    }

    private static byte[] CreateEncodedImage(bool png)
    {
        using var image = new Image<Rgba32>(2, 2);
        using var stream = new MemoryStream();
        if (png)
        {
            image.Save(stream, new PngEncoder());
        }
        else
        {
            image.Save(stream, new JpegEncoder());
        }

        return stream.ToArray();
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"realtor-image-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }

    private sealed class TestWebHostEnvironment(string contentRoot) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "RealtorApiTests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRoot;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
