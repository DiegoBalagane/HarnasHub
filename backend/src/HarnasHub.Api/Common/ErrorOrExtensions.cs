using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace HarnasHub.Api.Common;

/// <summary>Maps <see cref="ErrorOr"/> failures to an ASP.NET Core <see cref="ProblemDetails"/> result.</summary>
public static class ErrorOrExtensions
{
    #region Public Methods

    public static IResult ToProblemResult(this List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Results.Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            var validationErrors = errors
                .GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());

            return Results.ValidationProblem(validationErrors);
        }

        var firstError = errors[0];
        var statusCode = firstError.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            title: firstError.Code,
            detail: firstError.Description,
            statusCode: statusCode);
    }

    #endregion
}
