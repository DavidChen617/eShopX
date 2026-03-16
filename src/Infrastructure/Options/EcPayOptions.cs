namespace Infrastructure.Options;

public class EcPayOptions
{
    public static readonly string OptionKey = "ECPay";
    public string MerchantID { get; set; } = string.Empty;
    public string HashKey { get; set; } = string.Empty;
    public string HashIV { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string GoodsName { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderPhone { get; set; } = string.Empty;
    public string SenderZipCode { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
}
