namespace ApplicationCore.UseCases.Carts.CheckoutPreview;

public record CheckoutPreviewQuery(Guid UserId) : IRequest<CheckoutPreviewResponse>;
