using ApplicationCore.Enums;

namespace ApplicationCore.UseCases.Sellers.GetPendingSellers;

public class GetPendingSellersHandler(
    IRepository<User> userRepository,
    IReadRepository<User> readRepository)
    : IRequestHandler<GetPendingSellersQuery, GetPendingSellersResponse>
{
    public async Task<GetPendingSellersResponse> Handle(
        GetPendingSellersQuery query,
        CancellationToken cancellationToken = default)
    {
        var admin = await userRepository.GetByIdAsync(query.AdminId, cancellationToken)
            ?? throw new NotFoundException("Admin", query.AdminId);

        if (!admin.IsAdmin)
        {
            throw new ForbiddenException("只有管理員可以查詢賣家申請");
        }

        var items = await readRepository.QueryAsync(q =>
                q.Where(x => x.SellerStatus == SellerStatus.Pending)
                    .OrderByDescending(x => x.SellerAppliedAt)
                    .Select(x => new PendingSellerItem(
                        x.Id,
                        x.Name,
                        x.Email,
                        x.SellerAppliedAt ?? DateTime.MinValue)),
            cancellationToken);

        return new GetPendingSellersResponse(items);
    }
}
