namespace eShopX.Application.UseCases.Logistics;

public static class LogisticsCacheKeys
{
    public static string LogisticsSession(string token) => $"logistics_session:{token}";
    public static string UserLogistics(Guid userId) => $"userId_logistics:{userId}";
}
