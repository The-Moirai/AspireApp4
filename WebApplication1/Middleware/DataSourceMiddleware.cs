using WebApplication1.Middleware.Interfaces;
using WebApplication1.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace WebApplication1.Middleware
{
    /// <summary>
    /// 数据源中间件实现
    /// </summary>
    public class DataSourceMiddleware : IDataSourceMiddleware
    {
        private readonly ILogger<DataSourceMiddleware> _logger;
        private readonly IDataService _dataService;
        private readonly ICacheService _cacheService;
        private readonly ISqlService _sqlService;
        private readonly IOptionsMonitor<DataSourceConfig> _configMonitor;
        
        private DataSourceConfig _currentConfig;
        private DataSourceType _currentDataSource;
        private readonly object _configLock = new object();

        public DataSourceMiddleware(
            ILogger<DataSourceMiddleware> logger,
            IDataService dataService,
            ICacheService cacheService,
            ISqlService sqlService,
            IOptionsMonitor<DataSourceConfig> configMonitor)
        {
            _logger = logger;
            _dataService = dataService;
            _cacheService = cacheService;
            _sqlService = sqlService;
            _configMonitor = configMonitor;
            
            // 初始化配置
            _currentConfig = _configMonitor.CurrentValue;
            _currentDataSource = _currentConfig.DataSourceType;
            
            _logger.LogInformation("数据源中间件初始化完成，当前数据源类型：{DataSourceType}", _currentDataSource);
        }

        public DataSourceType CurrentDataSource => _currentDataSource;

        public async Task SwitchDataSourceAsync(DataSourceType dataSourceType)
        {
            lock (_configLock)
            {
                _currentDataSource = dataSourceType;
                _currentConfig.DataSourceType = dataSourceType;
            }
            
            _logger.LogInformation("切换数据源类型：{DataSourceType}", dataSourceType);
            
            // 根据新的数据源类型调整配置
            switch (dataSourceType)
            {
                case DataSourceType.DatabaseFirst:
                    _currentConfig.EnableDatabase = true;
                    _currentConfig.EnableCache = true;
                    break;
                case DataSourceType.CacheFirst:
                    _currentConfig.EnableDatabase = true;
                    _currentConfig.EnableCache = true;
                    break;
                case DataSourceType.DatabaseOnly:
                    _currentConfig.EnableDatabase = true;
                    _currentConfig.EnableCache = false;
                    break;
                case DataSourceType.CacheOnly:
                    _currentConfig.EnableDatabase = false;
                    _currentConfig.EnableCache = true;
                    break;
                case DataSourceType.Hybrid:
                    _currentConfig.EnableDatabase = true;
                    _currentConfig.EnableCache = true;
                    break;
            }
            
            await Task.CompletedTask;
        }

        public DataSourceConfig GetDataSourceConfig()
        {
            lock (_configLock)
            {
                return new DataSourceConfig
                {
                    DataSourceType = _currentConfig.DataSourceType,
                    CacheExpiryMinutes = _currentConfig.CacheExpiryMinutes,
                    EnableCache = _currentConfig.EnableCache,
                    EnableDatabase = _currentConfig.EnableDatabase,
                    WarmupStrategy = _currentConfig.WarmupStrategy,
                    DatabaseTimeoutSeconds = _currentConfig.DatabaseTimeoutSeconds,
                    EnableDetailedLogging = _currentConfig.EnableDetailedLogging
                };
            }
        }

        public async Task UpdateDataSourceConfigAsync(DataSourceConfig config)
        {
            lock (_configLock)
            {
                _currentConfig = config;
                _currentDataSource = config.DataSourceType;
            }
            
            _logger.LogInformation("更新数据源配置：{Config}", System.Text.Json.JsonSerializer.Serialize(config));
            
            // 如果禁用了缓存，清空缓存
            if (!config.EnableCache)
            {
                await ClearCacheAsync();
            }
            
            // 如果启用了缓存且配置了预热策略，执行预热
            if (config.EnableCache && config.WarmupStrategy == CacheWarmupStrategy.OnDemand)
            {
                await WarmupCacheAsync();
            }
        }

        public DataSourceStatus GetDataSourceStatus()
        {
            var status = new DataSourceStatus
            {
                CurrentDataSource = _currentDataSource,
                LastUpdated = DateTime.UtcNow,
                CacheAvailable = _currentConfig.EnableCache,
                CacheItemCount = 0 // 需要从缓存服务获取实际数量
            };
            
            // 检查数据库连接状态
            try
            {
                // 这里可以添加数据库连接测试
                status.DatabaseConnected = _currentConfig.EnableDatabase;
            }
            catch (Exception ex)
            {
                status.DatabaseConnected = false;
                status.ErrorMessage = ex.Message;
                _logger.LogError(ex, "检查数据库连接状态失败");
            }
            
            return status;
        }

        public async Task RefreshCacheAsync()
        {
            if (!_currentConfig.EnableCache)
            {
                _logger.LogWarning("缓存已禁用，无法刷新缓存");
                return;
            }
            
            _logger.LogInformation("开始刷新缓存");
            
            try
            {
                // 清空现有缓存
                await _cacheService.ClearAllAsync();
                
                // 重新加载数据到缓存
                await WarmupCacheAsync();
                
                _logger.LogInformation("缓存刷新完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新缓存失败");
                throw;
            }
        }

        public async Task ClearCacheAsync()
        {
            _logger.LogInformation("清空缓存");
            
            try
            {
                await _cacheService.ClearAllAsync();
                _logger.LogInformation("缓存清空完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空缓存失败");
                throw;
            }
        }

        public async Task WarmupCacheAsync()
        {
            if (!_currentConfig.EnableCache)
            {
                _logger.LogWarning("缓存已禁用，无法预热缓存");
                return;
            }
            
            _logger.LogInformation("开始预热缓存");
            
            try
            {
                // 预热无人机数据
                await WarmupDronesCacheAsync();
                
                // 预热任务数据
                await WarmupTasksCacheAsync();
                
                _logger.LogInformation("缓存预热完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "预热缓存失败");
                throw;
            }
        }

        public async Task<DataSourceHealthStatus> CheckHealthAsync()
        {
            var health = new DataSourceHealthStatus
            {
                Timestamp = DateTime.UtcNow,
                DataSourceType = _currentDataSource,
                EnableCache = _currentConfig.EnableCache,
                EnableDatabase = _currentConfig.EnableDatabase
            };
            
            // 检查缓存健康状态
            if (_currentConfig.EnableCache)
            {
                try
                {
                    var cacheStats = await GetCacheStatisticsAsync();
                    health.CacheHealthy = true;
                    health.CacheItemCount = cacheStats.TotalKeys;
                }
                catch (Exception ex)
                {
                    health.CacheHealthy = false;
                    health.CacheError = ex.Message;
                }
            }
            else
            {
                health.CacheHealthy = true; // 如果禁用缓存，认为缓存是健康的
            }
            
            // 检查数据库健康状态
            if (_currentConfig.EnableDatabase)
            {
                try
                {
                    // 执行简单查询测试数据库连接
                    var result = await _sqlService.ExecuteQueryAsync("SELECT 1 as Test");
                    health.DatabaseHealthy = result.Any();
                }
                catch (Exception ex)
                {
                    health.DatabaseHealthy = false;
                    health.DatabaseError = ex.Message;
                }
            }
            else
            {
                health.DatabaseHealthy = true; // 如果禁用数据库，认为数据库是健康的
            }
            
            return health;
        }

        public async Task<WebApplication1.Services.Interfaces.CacheStatistics> GetCacheStatisticsAsync()
        {
            try
            {
                return await _cacheService.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓存统计信息失败");
                return new WebApplication1.Services.Interfaces.CacheStatistics();
            }
        }

        private async Task WarmupDronesCacheAsync()
        {
            try
            {
                var drones = await _dataService.GetDronesAsync();
                _logger.LogDebug("预热无人机缓存，数量：{Count}", drones.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "预热无人机缓存失败");
            }
        }

        private async Task WarmupTasksCacheAsync()
        {
            try
            {
                var tasks = await _dataService.GetTasksAsync();
                _logger.LogDebug("预热任务缓存，数量：{Count}", tasks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "预热任务缓存失败");
            }
        }
    }
} 