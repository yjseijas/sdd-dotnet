using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RealtorApi.Infrastructure.Validation;

public static class ValidationFilterFactory
{
    public static Func<EndpointFilterFactoryContext, EndpointFilterDelegate, EndpointFilterDelegate> CreateFactory()
    {
        return (context, next) =>
        {
            var requestParameter = context.MethodInfo
                .GetParameters()
                .FirstOrDefault(parameter =>
                    parameter.ParameterType != typeof(CancellationToken)
                    && parameter.ParameterType != typeof(HttpContext)
                    && parameter.ParameterType != typeof(IServiceProvider));

            if (requestParameter is null)
            {
                return next;
            }

            return async invocationContext =>
            {
                var request = invocationContext.Arguments[requestParameter.Position];
                var validationProblem = await ValidateAsync(
                    invocationContext.HttpContext.RequestServices,
                    request,
                    invocationContext.HttpContext.RequestAborted);

                if (validationProblem is not null)
                {
                    return validationProblem;
                }

                return await next(invocationContext);
            };
        };
    }

    public static async Task<IResult?> ValidateAsync(
        IServiceProvider serviceProvider,
        object? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return null;
        }

        var requestType = request.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        var serviceAvailability = serviceProvider.GetService<IServiceProviderIsService>();

        if (serviceAvailability?.IsService(validatorType) != true)
        {
            return null;
        }

        var validator = serviceProvider.GetRequiredService(validatorType);
        var validationResult = await ((IValidator)validator)
            .ValidateAsync(new ValidationContext<object>(request), cancellationToken);

        if (validationResult.IsValid)
        {
            return null;
        }

        return Microsoft.AspNetCore.Http.Results.ValidationProblem(
            validationResult.Errors
                .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(item => item.ErrorMessage).ToArray()));
    }
}
