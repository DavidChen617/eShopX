namespace Infrastructure.Options;

public class LineAuthOptions
{
    public static readonly string OptionKey = nameof(LineAuthOptions).Substring(0, 8);
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}