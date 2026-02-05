using ApplicationCore.Entities;
using ApplicationCore.Enums;

namespace ApplicationCore.UseCases.Users.GetMe;

public record GetMeResponse(
    Guid UserId,
    string Name,
    string Email,
    string Phone,
    string? Address,
    DateTime CreatedAt,
    string? AvatarUrl,
    string? AvatarPublicId,
    string? AvatarFormat,
    int? AvatarWidth,
    int? AvatarHeight,
    long? AvatarBytes,
    bool IsSeller,
    bool IsAdmin,
    SellerStatus? SellerStatus,
    DateTime? SellerAppliedAt,
    DateTime? SellerApprovedAt,
    string? SellerRejectionReason);