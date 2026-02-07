namespace Infrastructure.Payments.Line.Models;

public record LinePayRequestResponse(
    string ReturnCode,
    string ReturnMessage,
    LinePayRequestInfo? Info
);

public record LinePayRequestInfo(
    LinePayPaymentUrl? PaymentUrl,
    long TransactionId,
    string PaymentAccessToken
);

public record LinePayPaymentUrl(
    string Web,
    string App
);

public record LinePayConfirmResponse(
    string ReturnCode,
    string ReturnMessage,
    LinePayConfirmInfo? Info
);

public record LinePayConfirmInfo(
    long TransactionId,
    string? OrderId,
    List<LinePayPayInfo>? PayInfo
);

public record LinePayPayInfo(
    string? Method,
    decimal Amount
);
