namespace eShopX.Domain.Aggregates.Users;

public sealed record Address(
    string City,
    string District,
    string Street,
    string PostalCode);
