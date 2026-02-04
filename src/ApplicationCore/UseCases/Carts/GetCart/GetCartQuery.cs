namespace ApplicationCore.UseCases.Carts.GetCart;

public record GetCartQuery(Guid UserId) : IRequest<GetCartResponse>;
