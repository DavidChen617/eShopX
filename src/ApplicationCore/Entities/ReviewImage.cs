namespace ApplicationCore.Entities;

public class ReviewImage : BaseEntity
{
    public Guid ReviewId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public Review Review { get; set; } = null!;
}
