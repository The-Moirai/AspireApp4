namespace WebApplication1.Services.Interfaces
{
    /// <summary>
    /// Redis缓存服务接口
    /// </summary>
    public interface ICacheService
    {
        #region 基础缓存操作
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>操作结果</returns>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>操作结果</returns>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 设置缓存过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>操作结果</returns>
        Task<bool> SetExpiryAsync(string key, TimeSpan expiry);
        #endregion

        #region 批量操作
        /// <summary>
        /// 批量设置缓存
        /// </summary>
        /// <param name="keyValues">键值对</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>操作结果</returns>
        Task<bool> SetMultipleAsync<T>(Dictionary<string, T> keyValues, TimeSpan? expiry = null);

        /// <summary>
        /// 批量获取缓存
        /// </summary>
        /// <param name="keys">缓存键列表</param>
        /// <returns>键值对</returns>
        Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys);

        /// <summary>
        /// 批量删除缓存
        /// </summary>
        /// <param name="keys">缓存键列表</param>
        /// <returns>删除数量</returns>
        Task<long> RemoveMultipleAsync(IEnumerable<string> keys);
        #endregion

        #region 高级操作
        /// <summary>
        /// 获取或设置缓存（如果不存在则设置）
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="factory">值工厂</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>缓存值</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);

        /// <summary>
        /// 原子递增
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">增量</param>
        /// <returns>递增后的值</returns>
        Task<long> IncrementAsync(string key, long value = 1);

        /// <summary>
        /// 原子递减
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">减量</param>
        /// <returns>递减后的值</returns>
        Task<long> DecrementAsync(string key, long value = 1);

        /// <summary>
        /// 设置哈希表字段
        /// </summary>
        /// <param name="key">哈希表键</param>
        /// <param name="field">字段名</param>
        /// <param name="value">字段值</param>
        /// <returns>操作结果</returns>
        Task<bool> SetHashAsync<T>(string key, string field, T value);

        /// <summary>
        /// 获取哈希表字段
        /// </summary>
        /// <param name="key">哈希表键</param>
        /// <param name="field">字段名</param>
        /// <returns>字段值</returns>
        Task<T?> GetHashAsync<T>(string key, string field);

        /// <summary>
        /// 获取哈希表所有字段
        /// </summary>
        /// <param name="key">哈希表键</param>
        /// <returns>所有字段</returns>
        Task<Dictionary<string, T?>> GetHashAllAsync<T>(string key);
        #endregion

        #region 模式匹配
        /// <summary>
        /// 根据模式获取键
        /// </summary>
        /// <param name="pattern">模式</param>
        /// <returns>匹配的键</returns>
        Task<IEnumerable<string>> GetKeysAsync(string pattern);

        /// <summary>
        /// 根据模式删除键
        /// </summary>
        /// <param name="pattern">模式</param>
        /// <returns>删除数量</returns>
        Task<long> RemoveByPatternAsync(string pattern);
        #endregion

        #region 缓存统计
        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        Task<CacheStatistics> GetStatisticsAsync();

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        /// <returns>操作结果</returns>
        Task<bool> ClearAllAsync();
        #endregion
    }

    /// <summary>
    /// 缓存统计信息
    /// </summary>
    public class CacheStatistics
    {
        public long TotalKeys { get; set; }
        public long HitCount { get; set; }
        public long MissCount { get; set; }
        public double HitRate => TotalKeys > 0 ? (double)HitCount / (HitCount + MissCount) * 100 : 0;
        public long MemoryUsage { get; set; }
        public DateTime LastResetTime { get; set; }
    }
}
