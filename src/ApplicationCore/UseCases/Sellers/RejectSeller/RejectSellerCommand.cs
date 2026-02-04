namespace ApplicationCore.UseCases.Sellers.RejectSeller;

public record RejectSellerCommand(
    Guid UserId,
    Guid AdminId,
    string Reason) : IRequest<RejectSellerResponse>;
