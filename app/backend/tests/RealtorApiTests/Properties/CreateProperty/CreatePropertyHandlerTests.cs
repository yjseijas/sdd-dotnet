using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RealtorApi.Features.Properties.CreateProperty;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;

namespace RealtorApiTests.Properties.CreateProperty;

public sealed class CreatePropertyHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenPersistenceFails_DeletesTheStoredImage()
    {
        var imageStorage = new StubImageStorage();
        var logger = new RecordingLogger<CreatePropertyHandler>();
        await using var dbContext = CreateFailingDbContext();
        var handler = new CreatePropertyHandler(dbContext, imageStorage, logger);

        var result = await handler.HandleAsync(ValidRequest(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.Error!.StatusCode);
        Assert.Equal(1, imageStorage.DeleteCalls);
        Assert.Same(imageStorage.StoredImage, imageStorage.DeletedImage);
    }

    [Fact]
    public async Task HandleAsync_WhenPersistenceAndImageCleanupFail_LogsCleanupFailureStructurally()
    {
        var imageStorage = new StubImageStorage { DeleteException = new IOException("cleanup failed") };
        var logger = new RecordingLogger<CreatePropertyHandler>();
        await using var dbContext = CreateFailingDbContext();
        var handler = new CreatePropertyHandler(dbContext, imageStorage, logger);

        var result = await handler.HandleAsync(ValidRequest(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.Error!.StatusCode);
        Assert.Equal(1, imageStorage.DeleteCalls);
        var cleanupLog = Assert.Single(
            logger.Entries,
            entry => entry.Message.Contains("Failed to remove property image", StringComparison.Ordinal));
        Assert.IsType<IOException>(cleanupLog.Exception);
        Assert.Contains(
            cleanupLog.State,
            item => item.Key == "ImageFileName" && Equals(item.Value, imageStorage.StoredImage.FileName));
    }

    private static AppDbContext CreateFailingDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(new SqliteConnection("Data Source=:memory:"))
            .AddInterceptors(new FailingSaveChangesInterceptor())
            .Options;
        return new AppDbContext(options);
    }

    private static CreatePropertyRequest ValidRequest()
    {
        return new CreatePropertyRequest
        {
            Title = "Casa central",
            Description = "Casa luminosa",
            Address = "Calle 123",
            Price = 125_000m,
            Status = "Available",
            BedroomCount = 3,
            BathroomCount = 2,
            AreaSquareMeters = 120m,
            Image = new FormFile(new MemoryStream([1]), 0, 1, "image", "property.png")
        };
    }

    private sealed class StubImageStorage : IPropertyImageStorage
    {
        public StoredPropertyImage StoredImage { get; } = new(
            "generated-image.png",
            "/assets/properties/generated-image.png",
            Path.Combine(Path.GetTempPath(), "generated-image.png"));

        public int DeleteCalls { get; private set; }
        public StoredPropertyImage? DeletedImage { get; private set; }
        public Exception? DeleteException { get; init; }

        public Task<Result<StoredPropertyImage>> StoreAsync(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<StoredPropertyImage>.Success(StoredImage));
        }

        public Task DeleteAsync(StoredPropertyImage image, CancellationToken cancellationToken)
        {
            DeleteCalls++;
            DeletedImage = image;
            if (DeleteException is not null)
            {
                throw DeleteException;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FailingSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Persistence failed.");
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var properties = state is IEnumerable<KeyValuePair<string, object?>> values
                ? values.ToArray()
                : [];
            Entries.Add(new LogEntry(formatter(state, exception), exception, properties));
        }
    }

    private sealed record LogEntry(
        string Message,
        Exception? Exception,
        IReadOnlyList<KeyValuePair<string, object?>> State);
}
