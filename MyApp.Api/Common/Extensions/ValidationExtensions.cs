using FluentValidation;
using MyApp.Api.Common.Models;

namespace MyApp.Api.Common.Extensions;

public static class ValidationExtensions
{
    public static async Task<(bool IsValid, IResult? ErrorResult)> ValidateRequestAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken ct = default)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (result.IsValid) return (true, null);

        var errors = result.Errors.Select(e => e.ErrorMessage);
        return (false, Results.BadRequest(ApiResponse.Fail(errors)));
    }
}
