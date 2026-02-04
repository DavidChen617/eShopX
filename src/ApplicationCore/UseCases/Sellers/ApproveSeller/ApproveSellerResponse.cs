namespace ApplicationCore.UseCases.Sellers.ApproveSeller;

public record ApproveSellerResponse(
    Guid UserId,
    string UserName,
    DateTime ApprovedAt);
