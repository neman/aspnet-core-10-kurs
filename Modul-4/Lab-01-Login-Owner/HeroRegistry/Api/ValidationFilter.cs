using System.ComponentModel.DataAnnotations;

namespace HeroRegistry.Api;

// .NET 9 Minimal APIs ignore DataAnnotations; [ApiController] checks them only for controllers.
// .NET 10: builder.Services.AddValidation() does it for every endpoint, without a filter.
public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().First();
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(
            argument,
            new ValidationContext(argument),
            results,
            validateAllProperties: true))
        {
            var errors = results
                .GroupBy(r => r.MemberNames.FirstOrDefault() ?? "")
                .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "").ToArray());
            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}
