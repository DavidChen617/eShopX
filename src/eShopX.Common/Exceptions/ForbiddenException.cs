using System.Net;

namespace eShopX.Common.Exceptions;

public sealed class ForbiddenException(string message) :
    AppException(message, HttpStatusCode.Forbidden);
