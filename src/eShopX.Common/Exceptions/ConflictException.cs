using System.Net;

namespace eShopX.Common.Exceptions;

public sealed class ConflictException(string message) :
    AppException(message, HttpStatusCode.Conflict);
