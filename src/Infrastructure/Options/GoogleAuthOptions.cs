namespace Infrastructure.Options;

public class GoogleAuthOptions
{
    public static readonly string OptionKey = nameof(GoogleAuthOptions).Substring(0, 10);
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
}