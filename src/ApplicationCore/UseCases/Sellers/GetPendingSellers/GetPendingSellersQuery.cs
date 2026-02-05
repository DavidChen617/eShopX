namespace ApplicationCore.UseCases.Sellers.GetPendingSellers;

public record GetPendingSellersQuery(Guid AdminId) : IRequest<GetPendingSellersResponse>;