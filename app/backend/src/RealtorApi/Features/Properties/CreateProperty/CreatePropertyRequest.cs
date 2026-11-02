using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace RealtorApi.Features.Properties.CreateProperty;

/// <summary>Datos multipart para dar de alta una propiedad con imagen opcional.</summary>
public sealed record CreatePropertyRequest
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Address { get; init; }

    public decimal? Price { get; init; }

    public string? Status { get; init; }

    public int? BedroomCount { get; init; }

    public int? BathroomCount { get; init; }

    public decimal? AreaSquareMeters { get; init; }

    public IFormFile? Image { get; init; }

    public static async ValueTask<CreatePropertyRequest> BindAsync(
        HttpContext httpContext,
        ParameterInfo parameter)
    {
        if (!httpContext.Request.HasFormContentType)
        {
            return new CreatePropertyRequest();
        }

        var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);

        return new CreatePropertyRequest
        {
            Title = GetValue(form, "title"),
            Description = GetValue(form, "description"),
            Address = GetValue(form, "address"),
            Price = ParseDecimal(GetValue(form, "price")),
            Status = GetValue(form, "status"),
            BedroomCount = ParseInt(GetValue(form, "bedroomCount")),
            BathroomCount = ParseInt(GetValue(form, "bathroomCount")),
            AreaSquareMeters = ParseDecimal(GetValue(form, "areaSquareMeters")),
            Image = form.Files.GetFile("image")
        };
    }

    private static string? GetValue(IFormCollection form, string name)
    {
        return form.TryGetValue(name, out var value) ? value.ToString() : null;
    }

    private static decimal? ParseDecimal(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static int? ParseInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}
