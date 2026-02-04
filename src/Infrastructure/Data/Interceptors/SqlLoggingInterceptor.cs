using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Data.Interceptors;

// TODO 1. 敏感參數不顯示策略，查詢過慢警示
public class SqlLoggingInterceptor : DbCommandInterceptor
{
}
