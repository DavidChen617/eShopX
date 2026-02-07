namespace ApplicationCore.UseCases.Sellers.ApplyForSeller;

public record ApplyForSellerCommand(Guid UserId) : IRequest<ApplyForSellerResponse>;
