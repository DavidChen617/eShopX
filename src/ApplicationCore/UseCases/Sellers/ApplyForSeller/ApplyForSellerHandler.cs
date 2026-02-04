using ApplicationCore.Enums;

namespace ApplicationCore.UseCases.Sellers.ApplyForSeller;

public class ApplyForSellerHandler(IRepository<User> userRepository)
    : IRequestHandler<ApplyForSellerCommand, ApplyForSellerResponse>
{
    public async Task<ApplyForSellerResponse> Handle(
        ApplyForSellerCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("User", command.UserId);

        // 檢查是否已經是賣家
        if (user.IsSeller)
        {
            throw new ConflictException("您已經是賣家");
        }

        // 檢查是否有待審核的申請
        if (user.SellerStatus == SellerStatus.Pending)
        {
            throw new ConflictException("您已有待審核的賣家申請");
        }

        // 提交申請
        user.SellerStatus = SellerStatus.Pending;
        user.SellerAppliedAt = DateTime.UtcNow;
        user.SellerApprovedAt = null;
        user.SellerApprovedBy = null;
        user.SellerRejectionReason = null;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        return new ApplyForSellerResponse(
            user.Id,
            user.SellerStatus.Value,
            user.SellerAppliedAt.Value);
    }
}
