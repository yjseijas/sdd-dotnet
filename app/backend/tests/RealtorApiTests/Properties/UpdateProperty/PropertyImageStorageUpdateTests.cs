using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using RealtorApi.Features.Properties.CreateProperty;

namespace RealtorApiTests.Properties.UpdateProperty;

public sealed class PropertyImageStorageUpdateTests
{
    [Fact]
    public async Task DeleteIfManagedAsync_WhenImageIsInsideManagedDirectory_DeletesIt()
    {
        using var directory = new TemporaryDirectory();
        var storage = CreateStorage(directory.Path);
        var fileName = "safe-file.png";
        var physicalPath = Path.Combine(directory.Path, "wwwroot", "assets", "properties", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
        await File.WriteAllBytesAsync(physicalPath, [1, 2, 3]);

        await storage.DeleteIfManagedAsync($"/assets/properties/{fileName}", CancellationToken.None);

        Assert.False(File.Exists(physicalPath));
    }

    [Fact]
    public async Task DeleteIfManagedAsync_WhenImageIsOutsideManagedDirectory_DoesNotDelete()
    {
        using var directory = new TemporaryDirectory();
        var storage = CreateStorage(directory.Path);
        var externalFile = Path.Combine(directory.Path, "external.png");
        await File.WriteAllBytesAsync(externalFile, [1, 2, 3]);

        await storage.DeleteIfManagedAsync($"/assets/{Path.GetFileName(externalFile)}", CancellationToken.None);

        Assert.True(File.Exists(externalFile));
    }

    private static PropertyImageStorage CreateStorage(string contentRoot)
    {
        return new PropertyImageStorage(new TestWebHostEnvironment(contentRoot), NullLogger<PropertyImageStorage>.Instance);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"realtor-update-storage-{Guid.NewGuid():N}");
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
