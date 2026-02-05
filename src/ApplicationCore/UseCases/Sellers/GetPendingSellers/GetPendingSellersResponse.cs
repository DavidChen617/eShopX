namespace ApplicationCore.UseCases.Sellers.GetPendingSellers;

public record PendingSellerItem(
    Guid UserId,
    string UserName,
    string Email,
    DateTime AppliedAt);

public record GetPendingSellersResponse(IReadOnlyList<PendingSellerItem> Items);