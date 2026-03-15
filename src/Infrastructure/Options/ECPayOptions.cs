namespace Infrastructure.Options;

public class ECPayOptions
{
    public static readonly string OptionKey = "ECPay";
    public string MerchantID { get; set; } = string.Empty;
    public string HashKey { get; set; } = string.Empty;
    public string HashIV { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://logistics-stage.ecpay.com.tw";
    public string PublicBaseUrl { get; set; } = string.Empty;
    public string GoodsName { get; set; } = "商品";
    public string SenderName { get; set; } = string.Empty;
    public string SenderPhone { get; set; } = string.Empty;
}
