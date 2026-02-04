using System.Net;
using eShopX.Common.Validation;

namespace eShopX.Common.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(IDictionary<string, string[]> errors) : base("One or more validation errors occurred.",
        HttpStatusCode.BadRequest)
    {
        Errors = errors;
    }

    public ValidationException(string field, string error) : base("One or more validation errors occurred.",
        HttpStatusCode.BadRequest)
    {
        Errors = new Dictionary<string, string[]> { { field, [error] } };
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : base("One or more validation errors occurred.",
        HttpStatusCode.BadRequest)
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray());
    }

    public IDictionary<string, string[]> Errors { get; set; }
}
