namespace ApplicationCore.UseCases.Sellers.ApproveSeller;

public record ApproveSellerCommand(
    Guid UserId,
    Guid AdminId) : IRequest<ApproveSellerResponse>;