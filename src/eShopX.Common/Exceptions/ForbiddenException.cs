using System.Net;

namespace eShopX.Common.Exceptions;

public class ForbiddenException(string message) :
    AppException(message, HttpStatusCode.Forbidden);
