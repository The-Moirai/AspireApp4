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
            var baseConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__app-db") ?? throw new InvalidOperationException("Connection string not found");
            
            // 添加连接池和超时配置
            var builder = new SqlConnectionStringBuilder(baseConnectionString)
            {
                MaxPoolSize = 100,           // 最大连接池大小
                MinPoolSize = 5,             // 最小连接池大小
                Pooling = true,              // 启用连接池
                ConnectTimeout = 30,          // 连接超时30秒
                ApplicationName = "WebApplication1" // 应用程序名称
            };
            
            _connectionString = builder.ConnectionString;
            _logger = logger;
            _logger.LogInformation("SQL Service initialized with connection string: {ConnectionString}", _connectionString);
        }

        public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string query)
        {
            _logger.LogInformation("数据库执行查询：{Query}", query);
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(query, connection)
            {
                CommandTimeout = 60  // 命令超时60秒
            };
            using var reader = await command.ExecuteReaderAsync();
            
            var results = new List<dynamic>();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(row);
            }
            
            return results.AsEnumerable();
        }

        public async Task<int> ExecuteCommandAsync(string command)
        {
            _logger.LogInformation("数据库执行命令：{Command}", command);
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var sqlCommand = new SqlCommand(command, connection)
            {
                CommandTimeout = 60  // 命令超时60秒
            };
            return await sqlCommand.ExecuteNonQueryAsync();
        }

        public async Task<int> ExecuteCommandWithImageAsync(string command, byte[] imageData)
        {
            _logger.LogInformation("数据库执行带图片数据的命令：{Command}", command);
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var sqlCommand = new SqlCommand(command, connection)
            {
                CommandTimeout = 60  // 命令超时60秒
            };
            sqlCommand.Parameters.AddWithValue("@ImageData", imageData);
            return await sqlCommand.ExecuteNonQueryAsync();
        }

        public async Task<int> ExecuteCommandWithParametersAsync(string command, Dictionary<string, object> parameters)
        {
            _logger.LogInformation("数据库执行带参数的命令：{Command}", command);
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var sqlCommand = new SqlCommand(command, connection)
                {
                    CommandTimeout = 60  // 命令超时60秒
                };

                // 添加参数
                foreach (var param in parameters)
                {
                    sqlCommand.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }

                int affectedRows = await sqlCommand.ExecuteNonQueryAsync();
                _logger.LogInformation("插入结果：影响行数={AffectedRows}, 参数={Parameters}", affectedRows, System.Text.Json.JsonSerializer.Serialize(parameters));
                return affectedRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据库插入异常，SQL：{Command}，参数：{Parameters}", command, System.Text.Json.JsonSerializer.Serialize(parameters));
                throw;
            }
        }
    }
}
