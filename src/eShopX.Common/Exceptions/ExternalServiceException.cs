using System.Net;

namespace eShopX.Common.Exceptions;

public class ExternalServiceException(string message) :
    AppException(message, HttpStatusCode.BadGateway);
