using System.Net;

namespace eShopX.Common.Exceptions;

public sealed class BadRequestException(string message) :
    AppException(message, HttpStatusCode.BadRequest);
