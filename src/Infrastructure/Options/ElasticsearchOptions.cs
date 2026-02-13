namespace Infrastructure.Options;

public class ElasticsearchOptions
{
    public static readonly string OptionKey = nameof(ElasticsearchOptions).Substring(0, 13);

    public string Url { get; set; } = string.Empty;
    public string IndexName { get; set; } = "products-v1";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableFallbackToDb { get; set; } = true;
    public int RequestTimeoutSeconds { get; set; } = 5;
    public bool AutoCreateIndex { get; set; } = true;
}
