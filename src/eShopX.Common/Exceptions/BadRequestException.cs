using System.Net;

namespace eShopX.Common.Exceptions;

public class BadRequestException(string message) :
    AppException(message, HttpStatusCode.BadRequest);
