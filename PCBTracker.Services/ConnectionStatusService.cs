using Microsoft.Data.SqlClient;

public class ConnectionStatusService
{
    private readonly string _connectionString;
    public ConnectionStatusService(string connectionString) => _connectionString = connectionString;

    public async Task<bool> CheckConnectionAsync()
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
