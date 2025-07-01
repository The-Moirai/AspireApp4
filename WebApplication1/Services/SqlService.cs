using WebApplication1.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace WebApplication1.Services
{
    public class SqlService : ISqlService
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlService> _logger;

        public SqlService(ILogger<SqlService> logger)
        {
            _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__app-db") ?? throw new InvalidOperationException("Connection string not found");
            _logger = logger;
        }
        public Task<IEnumerable<dynamic>> ExecuteQueryAsync(string query)
        {
            _logger.LogInformation("数据库执行查询：{Query}", query);
            return Task.Run(() =>
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();
                var results = new List<dynamic>();
                while (reader.Read())
                {
                    var row = new Object() as IDictionary<string, object>;
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    results.Add(row);
                }
                return results.AsEnumerable();
            });
        }
        public Task<int> ExecuteCommandAsync(string command)
        {
            _logger.LogInformation("数据库执行命令：{Command}", command);
            return Task.Run(() =>
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                using var sqlCommand = new SqlCommand(command, connection);
                return sqlCommand.ExecuteNonQuery();
            });
        }
    }
}
