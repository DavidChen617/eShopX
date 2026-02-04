namespace ApplicationCore.Entities;

public class ExternalLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public string LoginProvider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string EmailAtLinkTime { get; set; } = string.Empty;
    public DateTime LastLoginAt { get; set; }
    public User? User { get; set; }
}
