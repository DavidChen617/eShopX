using System.Text.Json.Serialization;

namespace eShopX.Common.Responses;

public class ApiResponse
{
    [JsonPropertyName("isError")]
    public bool IsError { get; init; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ApiError>? Errors { get; init; }

    public static ApiResponse Fail(int statusCode, string message, List<ApiError> errors) => new()
    {
        IsError = true,
        StatusCode = statusCode,
        Message = message,
        Errors = errors
    };

    public static ApiResponse Fail(int statusCode, List<ApiError> errors) =>
        Fail(statusCode, "Validation failed", errors);

    public static ApiResponse Fail(int statusCode, string message, IDictionary<string, string[]> errors) =>
        Fail(statusCode, message, errors
            .SelectMany(kvp => kvp.Value.Select(v => new ApiError(kvp.Key, v)))
            .ToList());

    public static ApiResponse Error(int statusCode, string message) => new()
    {
        IsError = true,
        StatusCode = statusCode,
        Message = message,
        Errors = null
    };

    public static ApiResponse NoContent(string message = "No content", int statusCode = 204) => new()
    {
        IsError = false,
        StatusCode = statusCode,
        Message = message,
        Errors = null
    };
}

public class ApiResponse<T> : ApiResponse
{
    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Result { get; init; }

    public static ApiResponse<T> Success(T data, string message = "Request successful", int statusCode = 200) => new()
    {
        IsError = false,
        StatusCode = statusCode,
        Message = message,
        Result = data,
        Errors = null
    };
}