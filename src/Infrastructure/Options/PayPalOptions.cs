namespace Infrastructure.Options;

public class PayPalOptions
{
    public static readonly string OptionKey = "PayPal";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";
    public string PublicBaseUrl { get; set; } = string.Empty;   // ngrok
    public string FrontendBaseUrl { get; set; } = "http://localhost:4200";
}
