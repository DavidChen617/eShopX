namespace Infrastructure.Options;

public class HuggingFaceOptions
{
    public static readonly string OptionKey = "HuggingFace";
    public string Token { get; set; } = string.Empty;
    public string EmbeddingUrl { get; set; } = string.Empty;
}
