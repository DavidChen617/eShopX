using System.Net;

namespace eShopX.Common.Exceptions;

public class NotFoundException(string resourceName, object key) :
    AppException($"{resourceName} with identifier '{key}' was not found.", HttpStatusCode.NotFound);
