namespace WebApplication1.Services.Interfaces
{
    public interface ISqlService
    {

        /// <summary>
        /// 执行SQL查询
        /// </summary>
        /// <param name="query">SQL查询语句</param>
        /// <returns>查询结果</returns>
        Task<IEnumerable<dynamic>> ExecuteQueryAsync(string query);
        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="command">SQL命令语句</param>
        /// <returns>受影响的行数</returns>
        Task<int> ExecuteCommandAsync(string command);
        /// <summary>
        /// 执行带图片数据的SQL命令
        /// </summary>
        /// <param name="command">SQL命令语句</param>
        /// <param name="imageData">图片二进制数据</param>
        /// <returns>受影响的行数</returns>
        Task<int> ExecuteCommandWithImageAsync(string command, byte[] imageData);
        /// <summary>
        /// 执行带参数的SQL命令
        /// </summary>
        /// <param name="command">SQL命令语句</param>
        /// <param name="parameters">参数字典</param>
        /// <returns>受影响的行数</returns>
        Task<int> ExecuteCommandWithParametersAsync(string command, Dictionary<string, object> parameters);
    }
}
