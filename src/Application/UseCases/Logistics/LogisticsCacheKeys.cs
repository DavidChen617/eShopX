namespace eShopX.Application.UseCases.Logistics;

internal static class LogisticsCacheKeys
{
    public static string LogisticsSession(string token) => $"logistics_session:{token}";
    public static string UserLogistics(Guid userId) => $"userId_logistics:{userId}";
}
