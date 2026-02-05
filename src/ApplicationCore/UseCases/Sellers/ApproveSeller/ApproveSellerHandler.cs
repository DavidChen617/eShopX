using ApplicationCore.Enums;

namespace ApplicationCore.UseCases.Sellers.ApproveSeller;

public class ApproveSellerHandler(IRepository<User> userRepository)
    : IRequestHandler<ApproveSellerCommand, ApproveSellerResponse>
{
    public async Task<ApproveSellerResponse> Handle(
        ApproveSellerCommand command,
        CancellationToken cancellationToken = default)
    {
        // 驗證管理員身分
        var admin = await userRepository.GetByIdAsync(command.AdminId, cancellationToken)
            ?? throw new NotFoundException("Admin", command.AdminId);

        if (!admin.IsAdmin)
        {
            throw new ForbiddenException("只有管理員可以審核賣家申請");
        }

        // 取得待審核用戶
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("User", command.UserId);

        // 檢查申請狀態
        if (user.SellerStatus != SellerStatus.Pending)
        {
            throw new ConflictException("此用戶沒有待審核的賣家申請");
        }

        // 通過申請
        user.IsSeller = true;
        user.SellerStatus = SellerStatus.Approved;
        user.SellerApprovedAt = DateTime.UtcNow;
        user.SellerApprovedBy = command.AdminId;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        return new ApproveSellerResponse(
            user.Id,
            user.Name,
            user.SellerApprovedAt.Value);
    }
}