namespace eShopX.Common.Exceptions;

public class ConcurrencyException(string message)
    : ConflictException(message);
