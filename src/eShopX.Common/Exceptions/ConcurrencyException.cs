using System.Net;

namespace eShopX.Common.Exceptions;

public class ConcurrencyException(string message) 
    : AppException(message, HttpStatusCode.Conflict);
