using ApplicationCore.Enums;

namespace ApplicationCore.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarPublicId { get; set; }
    public string? AvatarFormat { get; set; }
    public int? AvatarWidth { get; set; }
    public int? AvatarHeight { get; set; }
    public long? AvatarBytes { get; set; }

    // 角色相關
    public bool IsAdmin { get; set; } = false;
    public bool IsSeller { get; set; } = false;

    // 賣家申請相關
    public SellerStatus? SellerStatus { get; set; }
    public DateTime? SellerAppliedAt { get; set; }
    public DateTime? SellerApprovedAt { get; set; }
    public Guid? SellerApprovedBy { get; set; }
    public string? SellerRejectionReason { get; set; }

    // Navigation Properties
    public ICollection<ExternalLogin> ExternalLogins { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}