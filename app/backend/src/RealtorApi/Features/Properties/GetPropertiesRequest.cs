namespace RealtorApi.Features.Properties;

public sealed class GetPropertiesRequest
{
    public string? Page { get; init; }

    public string? PageSize { get; init; }

    public int GetPageOrDefault() => ParsePositiveIntOrDefault(Page, 1);

    public int GetPageSizeOrDefault() => ParsePositiveIntOrDefault(PageSize, 6);

    private static int ParsePositiveIntOrDefault(string? value, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return int.TryParse(value, out var parsedValue) && parsedValue > 0
            ? parsedValue
            : defaultValue;
    }
}
