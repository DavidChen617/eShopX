using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.DeleteBanner;

public class DeleteBannerHandler(
    IRepository<Banner> bannerRepository,
    ICacheService cacheService)
    : IRequestHandler<DeleteBannerCommand, DeleteBannerResponse>
{
    public async Task<DeleteBannerResponse> Handle(DeleteBannerCommand command, CancellationToken cancellationToken = default)
    {
        var banner = await bannerRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Banner", command.Id);

        bannerRepository.Remove(banner);
        await bannerRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:banners", cancellationToken);

        return new DeleteBannerResponse(true);
    }
}
