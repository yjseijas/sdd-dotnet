using Microsoft.Extensions.Logging;

namespace RealtorApi.Infrastructure.Logging;

public static class StructuredLoggerExtensions
{
    public static void LogSliceDiscovery(this ILogger logger, Type sliceType, string assemblyName)
    {
        logger.LogInformation(
            "Discovered slice {SliceType} from assembly {AssemblyName}",
            sliceType.FullName,
            assemblyName);
    }

    public static void LogValidationFailure(this ILogger logger, string endpointName, string propertyName, string message)
    {
        logger.LogWarning(
            "Validation failed for endpoint {EndpointName} on property {PropertyName}: {Message}",
            endpointName,
            propertyName,
            message);
    }
}
