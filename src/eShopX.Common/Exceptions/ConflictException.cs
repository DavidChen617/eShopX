using System.Net;

namespace eShopX.Common.Exceptions;

public class ConflictException(string message) :
    AppException(message, HttpStatusCode.Conflict);
