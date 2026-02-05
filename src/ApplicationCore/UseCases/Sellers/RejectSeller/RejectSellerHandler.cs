using ApplicationCore.Enums;

namespace ApplicationCore.UseCases.Sellers.RejectSeller;

public class RejectSellerHandler(IRepository<User> userRepository)
    : IRequestHandler<RejectSellerCommand, RejectSellerResponse>
{
    public async Task<RejectSellerResponse> Handle(
        RejectSellerCommand command,
        CancellationToken cancellationToken = default)
    {
        var admin = await userRepository.GetByIdAsync(command.AdminId, cancellationToken)
            ?? throw new NotFoundException("Admin", command.AdminId);

        if (!admin.IsAdmin)
        {
            throw new ForbiddenException("只有管理員可以審核賣家申請");
        }

        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("User", command.UserId);

        if (user.SellerStatus != SellerStatus.Pending)
        {
            throw new ConflictException("此用戶沒有待審核的賣家申請");
        }

        user.IsSeller = false;
        user.SellerStatus = SellerStatus.Rejected;
        user.SellerApprovedAt = null;
        user.SellerApprovedBy = command.AdminId;
        user.SellerRejectionReason = command.Reason;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        return new RejectSellerResponse(
            user.Id,
            user.Name,
            DateTime.UtcNow,
            command.Reason);
    }
}