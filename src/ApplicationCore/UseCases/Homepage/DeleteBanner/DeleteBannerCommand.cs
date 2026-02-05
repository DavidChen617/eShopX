namespace ApplicationCore.UseCases.Homepage.DeleteBanner;

public record DeleteBannerCommand(Guid Id) : IRequest<DeleteBannerResponse>;