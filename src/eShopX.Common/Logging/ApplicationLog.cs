namespace eShopX.Common.Logging;

public class ApplicationLog
{
    public long Id { get; set; }
    public string? ScopeId { get; set; }
    public string Message { get; set; } =  string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
