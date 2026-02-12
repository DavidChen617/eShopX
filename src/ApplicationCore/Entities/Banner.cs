namespace ApplicationCore.Entities;

public class Banner : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string? ImagePublicId { get; set; }
    public string? ImageFormat { get; set; }
    public int? ImageWidth { get; set; }
    public int? ImageHeight { get; set; }
    public long? ImageBytes { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
