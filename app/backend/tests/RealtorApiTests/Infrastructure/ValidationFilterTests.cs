using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Validation;

namespace RealtorApiTests.Infrastructure;

public class ValidationFilterTests
{
    [Fact]
    public async Task ValidateAsync_UsesValidatorAndReturnsProblemDetailsWhenInvalid()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddValidatorsFromAssemblyContaining<ExampleRequestValidator>();

        using var provider = services.BuildServiceProvider();
        var response = await ValidationFilterFactory.ValidateAsync(
            provider,
            new ExampleRequest { Name = string.Empty },
            CancellationToken.None);

        Assert.NotNull(response);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = provider
        };
        httpContext.Response.Body = new MemoryStream();

        await response!.ExecuteAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync();

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        Assert.Contains("Name", payload, StringComparison.Ordinal);
    }

    public sealed record ExampleRequest
    {
        public string Name { get; init; } = string.Empty;
    }

    public sealed class ExampleRequestValidator : AbstractValidator<ExampleRequest>
    {
        public ExampleRequestValidator()
        {
            RuleFor(request => request.Name).NotEmpty();
        }
    }
}
