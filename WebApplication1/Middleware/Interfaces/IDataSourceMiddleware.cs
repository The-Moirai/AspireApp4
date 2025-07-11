using ClassLibrary1.Data;
using ClassLibrary1.Drone;
using ClassLibrary1.Tasks;

namespace WebApplication1.Middleware.Interfaces
{
    /// <summary>
    /// 数据源中间件接口
    /// </summary>
    public interface IDataSourceMiddleware
    {
        /// <summary>
        /// 数据源类型
        /// </summary>
        DataSourceType CurrentDataSource { get; }
        
        /// <summary>
        /// 切换数据源
        /// </summary>
        /// <param name="dataSourceType">数据源类型</param>
        Task SwitchDataSourceAsync(DataSourceType dataSourceType);
        
        /// <summary>
        /// 获取数据源配置
        /// </summary>
        DataSourceConfig GetDataSourceConfig();
        
        /// <summary>
        /// 更新数据源配置
        /// </summary>
        /// <param name="config">配置</param>
        Task UpdateDataSourceConfigAsync(DataSourceConfig config);
        
        /// <summary>
        /// 获取数据源状态
        /// </summary>
        DataSourceStatus GetDataSourceStatus();
        
        /// <summary>
        /// 刷新缓存
        /// </summary>
        Task RefreshCacheAsync();
        
        /// <summary>
        /// 清空缓存
        /// </summary>
        Task ClearCacheAsync();
        
        /// <summary>
        /// 预热缓存
        /// </summary>
        Task WarmupCacheAsync();
        
        /// <summary>
        /// 检查数据源健康状态
        /// </summary>
        Task<DataSourceHealthStatus> CheckHealthAsync();
        
        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        Task<WebApplication1.Services.Interfaces.CacheStatistics> GetCacheStatisticsAsync();
    }
    
    /// <summary>
    /// 数据源类型
    /// </summary>
    public enum DataSourceType
    {
        /// <summary>
        /// 数据库优先
        /// </summary>
        DatabaseFirst,
        
        /// <summary>
        /// 缓存优先
        /// </summary>
        CacheFirst,
        
        /// <summary>
        /// 仅数据库
        /// </summary>
        DatabaseOnly,
        
        /// <summary>
        /// 仅缓存
        /// </summary>
        CacheOnly,
        
        /// <summary>
        /// 混合模式
        /// </summary>
        Hybrid
    }
    
    /// <summary>
    /// 数据源配置
    /// </summary>
    public class DataSourceConfig
    {
        /// <summary>
        /// 数据源类型
        /// </summary>
        public DataSourceType DataSourceType { get; set; } = DataSourceType.DatabaseFirst;
        
        /// <summary>
        /// 缓存过期时间（分钟）
        /// </summary>
        public int CacheExpiryMinutes { get; set; } = 5;
        
        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool EnableCache { get; set; } = true;
        
        /// <summary>
        /// 是否启用数据库
        /// </summary>
        public bool EnableDatabase { get; set; } = true;
        
        /// <summary>
        /// 缓存预热策略
        /// </summary>
        public CacheWarmupStrategy WarmupStrategy { get; set; } = CacheWarmupStrategy.OnDemand;
        
        /// <summary>
        /// 数据库连接超时（秒）
        /// </summary>
        public int DatabaseTimeoutSeconds { get; set; } = 60;
        
        /// <summary>
        /// 是否启用详细日志
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;
    }
    
    /// <summary>
    /// 缓存预热策略
    /// </summary>
    public enum CacheWarmupStrategy
    {
        /// <summary>
        /// 按需预热
        /// </summary>
        OnDemand,
        
        /// <summary>
        /// 启动时预热
        /// </summary>
        OnStartup,
        
        /// <summary>
        /// 定时预热
        /// </summary>
        Scheduled,
        
        /// <summary>
        /// 不预热
        /// </summary>
        None
    }
    
    /// <summary>
    /// 数据源状态
    /// </summary>
    public class DataSourceStatus
    {
        /// <summary>
        /// 当前数据源类型
        /// </summary>
        public DataSourceType CurrentDataSource { get; set; }
        
        /// <summary>
        /// 数据库连接状态
        /// </summary>
        public bool DatabaseConnected { get; set; }
        
        /// <summary>
        /// 缓存状态
        /// </summary>
        public bool CacheAvailable { get; set; }
        
        /// <summary>
        /// 缓存项数量
        /// </summary>
        public long CacheItemCount { get; set; }
        
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// 数据源健康状态
    /// </summary>
    public class DataSourceHealthStatus
    {
        /// <summary>
        /// 检查时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 数据源类型
        /// </summary>
        public DataSourceType DataSourceType { get; set; }
        
        /// <summary>
        /// 缓存是否健康
        /// </summary>
        public bool CacheHealthy { get; set; }
        
        /// <summary>
        /// 数据库是否健康
        /// </summary>
        public bool DatabaseHealthy { get; set; }
        
        /// <summary>
        /// 缓存项数量
        /// </summary>
        public long CacheItemCount { get; set; }
        
        /// <summary>
        /// 缓存错误信息
        /// </summary>
        public string? CacheError { get; set; }
        
        /// <summary>
        /// 数据库错误信息
        /// </summary>
        public string? DatabaseError { get; set; }
        
        /// <summary>
        /// 整体健康状态
        /// </summary>
        public bool IsHealthy => CacheHealthy && DatabaseHealthy;
        
        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool EnableCache { get; set; }
        
        /// <summary>
        /// 是否启用数据库
        /// </summary>
        public bool EnableDatabase { get; set; }
    }
    

} 