using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RealtorApi.Infrastructure.Results;

public static class ResultProblemDetailsMapper
{
    public static ProblemDetails ToProblemDetails<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new ProblemDetails
            {
                Status = StatusCodes.Status200OK,
                Title = "Success",
                Detail = "The operation completed successfully."
            };
        }

        var error = result.Error ?? new ErrorDetails(
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "An unexpected error occurred.",
            "error.internal");

        return new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Detail,
            Type = $"https://httpstatuses.com/{error.StatusCode}",
            Extensions =
            {
                ["code"] = error.Code
            }
        };
    }
}
