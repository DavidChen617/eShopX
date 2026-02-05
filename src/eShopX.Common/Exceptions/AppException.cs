using System.Net;

namespace eShopX.Common.Exceptions;

public abstract class AppException(
    string message,
    HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}