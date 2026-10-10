using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RealtorApi.Infrastructure.Logging;
using RealtorApi.Infrastructure.Results;

namespace RealtorApiTests.Infrastructure;

public class ResultAndLoggingTests
{
    [Fact]
    public void Result_ToProblemDetails_PreservesExpectedStatusAndDetails()
    {
        var result = Result<string>.Failure(
            StatusCodes.Status404NotFound,
            "Not found",
            "The requested record was not found.",
            "record.not_found");

        var problem = result.ToProblemDetails();

        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Not found", problem.Title);
        Assert.Equal("The requested record was not found.", problem.Detail);
        Assert.Equal("record.not_found", problem.Extensions["code"]);
    }

    [Fact]
    public void StructuredLoggerExtensions_LogValidationFailure_UsesStructuredProperties()
    {
        var logger = new TestLogger();

        logger.LogValidationFailure("GetPropertyById", "propertyId", "Property identifier is required.");

        Assert.Contains(logger.Entries, entry =>
            entry.Contains("GetPropertyById", StringComparison.Ordinal)
            && entry.Contains("propertyId", StringComparison.Ordinal));
    }

    private sealed class TestLogger : ILogger
    {
        public List<string> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(state?.ToString() ?? string.Empty);
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
