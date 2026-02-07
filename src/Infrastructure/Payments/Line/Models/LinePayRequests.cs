namespace Infrastructure.Payments.Line.Models;

public record LinePayRequest(
    decimal Amount,
    string Currency,
    string OrderId,
    List<LinePayPackage> Packages,
    LinePayRedirectUrls RedirectUrls,
    LinePayOptionsRequest? Options = null
);

public record LinePayPackage(
    string Id,
    decimal Amount,
    List<LinePayProduct> Products,
    decimal? UserFee = null,
    string? Name = null
);

public record LinePayProduct(
    string Id,
    string Name,
    int Quantity,
    decimal Price,
    string? ImageUrl = null
);

public record LinePayRedirectUrls(
    string ConfirmUrl,
    string CancelUrl
);

public record LinePayOptionsRequest(
    LinePayPaymentOptions? Payment = null,
    LinePayDisplayOptions? Display = null
);

public record LinePayPaymentOptions(
    bool? Capture = null,
    string? PayType = null
);

public record LinePayDisplayOptions(
    string? Locale = null,
    bool? CheckConfirmUrlBrowser = null
);

public record LinePayConfirmRequest(
    decimal Amount,
    string Currency
);

public record LinePayConfirmInput(
    long TransactionId,
    LinePayConfirmRequest Request
);
