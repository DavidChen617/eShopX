namespace eShopX.Common.Logging;

public interface IScopeIdAccessor
{
    string? ScopeId { get; set; }
}

public sealed class ScopeIdAccessor : IScopeIdAccessor
{
    private static readonly AsyncLocal<string?> Current = new();

    public string? ScopeId
    {
        get => Current.Value;
        set => Current.Value = value;
    }
}
