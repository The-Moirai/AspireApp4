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
    }
}
