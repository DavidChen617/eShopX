namespace ApplicationCore.UseCases.Sellers.RejectSeller;

public record RejectSellerResponse(
    Guid UserId,
    string UserName,
    DateTime RejectedAt,
    string Reason);
