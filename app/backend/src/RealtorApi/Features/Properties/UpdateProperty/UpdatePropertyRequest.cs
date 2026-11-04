using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace RealtorApi.Features.Properties.UpdateProperty;

public sealed record UpdatePropertyRequest
{
    public Guid? Id { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Address { get; init; }

    public decimal? Price { get; init; }

    public string? Status { get; init; }

    public int? BedroomCount { get; init; }

    public int? BathroomCount { get; init; }

    public decimal? AreaSquareMeters { get; init; }

    public IFormFile? Image { get; init; }

    public static async ValueTask<UpdatePropertyRequest> BindAsync(
        HttpContext httpContext,
        ParameterInfo parameter)
    {
        if (!httpContext.Request.HasFormContentType)
        {
            return new UpdatePropertyRequest
            {
                Id = GetRouteId(httpContext)
            };
        }

        var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);

        return new UpdatePropertyRequest
        {
            Id = GetRouteId(httpContext),
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

    private static Guid? GetRouteId(HttpContext httpContext)
    {
        if (httpContext.Request.RouteValues.TryGetValue("id", out var value)
            && value is not null
            && Guid.TryParse(value.ToString(), out var routeId))
        {
            return routeId;
        }

        return null;
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
