using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Results;

namespace RealtorApiTests.Infrastructure;

public class HandlerAndValidationTests
{
    [Fact]
    public void AddHandlers_RegistersConcreteHandlersAutomatically()
    {
        var services = new ServiceCollection();
        services.AddHandlers(typeof(TestHandler).Assembly);

        using var provider = services.BuildServiceProvider();
        var handler = provider.GetService<IHandler<TestRequest, string>>();

        Assert.NotNull(handler);
    }

    [Fact]
    public async Task AddValidatorsFromAssembly_RegistersValidationForRequestType()
    {
        var services = new ServiceCollection();
        services.AddValidatorsFromAssemblyContaining<TestRequestValidator>();

        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IValidator<TestRequest>>();
        var result = await validator.ValidateAsync(new TestRequest { Name = string.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(TestRequest.Name));
    }

    public sealed record TestRequest
    {
        public string Name { get; init; } = string.Empty;
    }

    public sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(request => request.Name).NotEmpty();
        }
    }

    public sealed class TestHandler : IHandler<TestRequest, string>
    {
        public Task<Result<string>> HandleAsync(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success(request.Name));
        }
    }
}
