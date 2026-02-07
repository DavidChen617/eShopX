namespace Infrastructure.Options;

public class LinePayOptions
{
    public static readonly string OptionKey = "LinePay";
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox-api-pay.line.me";
    public string PublicBaseUrl { get; set; } = string.Empty;
    public string FrontendBaseUrl { get; set; } = "http://localhost:4200";
}
