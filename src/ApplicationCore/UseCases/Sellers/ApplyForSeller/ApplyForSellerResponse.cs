using ApplicationCore.Enums;

namespace ApplicationCore.UseCases.Sellers.ApplyForSeller;

public record ApplyForSellerResponse(
    Guid UserId,
    SellerStatus Status,
    DateTime AppliedAt);