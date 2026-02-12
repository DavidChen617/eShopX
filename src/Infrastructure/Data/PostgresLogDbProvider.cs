using eShopX.Common.Logging;
using eShopX.Common.Logging.Sinks;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Data;

public sealed class PostgresLogDbProvider(IOptions<ConnectionStrings> connectionString): ILogDbProvider
{
    public void Add(ApplicationLog log)
    {
        using var conn = new NpgsqlConnection(connectionString.Value.PostgreSQL);                                                                              
        conn.Open();                                                                                                                                           
        using var cmd = new NpgsqlCommand(                                                                                                                     
            @"INSERT INTO ""ApplicationLogs"" (""ScopeId"", ""Message"", ""CreatedAt"")
                VALUES (@scopeId, @msg, @time)", conn);                                                                                                          
        cmd.Parameters.AddWithValue("scopeId", (object?)log.ScopeId ?? DBNull.Value);                                                                          
        cmd.Parameters.AddWithValue("msg", log.Message);                                                                                                       
        cmd.Parameters.AddWithValue("time", log.CreatedAt);                                                                                                    
        cmd.ExecuteNonQuery();   
    }
}
