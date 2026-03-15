namespace Infrastructure.Options;

public class SiteOptions
{
    public static readonly string OptionKey = "Site";
    public string FrontendDomain { get; set; } = "http://localhost:4200";
    public string DomainUrl { get; set; } = string.Empty;
}
