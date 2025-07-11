using Microsoft.AspNetCore.Mvc;
using WebApplication1.Middleware.Interfaces;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// 数据源测试控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataSourceTestController : ControllerBase
    {
        private readonly WebApplication1.Middleware.Interfaces.IDataSourceMiddleware _dataSourceMiddleware;
        private readonly IDataService _dataService;
        private readonly ILogger<DataSourceTestController> _logger;

        public DataSourceTestController(
            WebApplication1.Middleware.Interfaces.IDataSourceMiddleware dataSourceMiddleware,
            IDataService dataService,
            ILogger<DataSourceTestController> logger)
        {
            _dataSourceMiddleware = dataSourceMiddleware;
            _dataService = dataService;
            _logger = logger;
        }

        /// <summary>
        /// 测试不同数据源模式下的数据访问性能
        /// </summary>
        [HttpGet("performance")]
        public async Task<ActionResult<object>> TestPerformance()
        {
            var results = new List<object>();
            
            // 测试数据库优先模式
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            var dbFirstStart = DateTime.UtcNow;
            var drones1 = await _dataService.GetDronesAsync();
            var dbFirstTime = DateTime.UtcNow - dbFirstStart;
            
            results.Add(new
            {
                Mode = "DatabaseFirst",
                Count = drones1.Count,
                TimeMs = dbFirstTime.TotalMilliseconds,
                DataSourceType = _dataSourceMiddleware.CurrentDataSource
            });
            
            // 测试缓存优先模式
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            var cacheFirstStart = DateTime.UtcNow;
            var drones2 = await _dataService.GetDronesAsync();
            var cacheFirstTime = DateTime.UtcNow - cacheFirstStart;
            
            results.Add(new
            {
                Mode = "CacheFirst",
                Count = drones2.Count,
                TimeMs = cacheFirstTime.TotalMilliseconds,
                DataSourceType = _dataSourceMiddleware.CurrentDataSource
            });
            
            // 测试仅数据库模式
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseOnly);
            var dbOnlyStart = DateTime.UtcNow;
            var drones3 = await _dataService.GetDronesAsync();
            var dbOnlyTime = DateTime.UtcNow - dbOnlyStart;
            
            results.Add(new
            {
                Mode = "DatabaseOnly",
                Count = drones3.Count,
                TimeMs = dbOnlyTime.TotalMilliseconds,
                DataSourceType = _dataSourceMiddleware.CurrentDataSource
            });
            
            // 测试仅缓存模式
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheOnly);
            var cacheOnlyStart = DateTime.UtcNow;
            var drones4 = await _dataService.GetDronesAsync();
            var cacheOnlyTime = DateTime.UtcNow - cacheOnlyStart;
            
            results.Add(new
            {
                Mode = "CacheOnly",
                Count = drones4.Count,
                TimeMs = cacheOnlyTime.TotalMilliseconds,
                DataSourceType = _dataSourceMiddleware.CurrentDataSource
            });
            
            return Ok(new
            {
                TestTime = DateTime.UtcNow,
                Results = results
            });
        }

        /// <summary>
        /// 测试缓存预热功能
        /// </summary>
        [HttpGet("warmup-test")]
        public async Task<ActionResult<object>> TestWarmup()
        {
            try
            {
                // 清空缓存
                await _dataSourceMiddleware.ClearCacheAsync();
                
                // 预热缓存
                var warmupStart = DateTime.UtcNow;
                await _dataSourceMiddleware.WarmupCacheAsync();
                var warmupTime = DateTime.UtcNow - warmupStart;
                
                // 获取缓存统计
                var cacheStats = await _dataSourceMiddleware.GetCacheStatisticsAsync();
                
                return Ok(new
                {
                    WarmupTime = warmupTime.TotalMilliseconds,
                    CacheStats = cacheStats,
                    Message = "缓存预热完成"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "缓存预热测试失败");
                return StatusCode(500, new { error = "缓存预热测试失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 测试数据源健康状态
        /// </summary>
        [HttpGet("health-test")]
        public async Task<ActionResult<object>> TestHealth()
        {
            try
            {
                var health = await _dataSourceMiddleware.CheckHealthAsync();
                var status = _dataSourceMiddleware.GetDataSourceStatus();
                var config = _dataSourceMiddleware.GetDataSourceConfig();
                
                return Ok(new
                {
                    Health = health,
                    Status = status,
                    Config = config,
                    TestTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康状态测试失败");
                return StatusCode(500, new { error = "健康状态测试失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 测试数据源切换
        /// </summary>
        [HttpGet("switch-test")]
        public async Task<ActionResult<object>> TestSwitch()
        {
            var results = new List<object>();
            
            foreach (DataSourceType type in Enum.GetValues(typeof(DataSourceType)))
            {
                try
                {
                    await _dataSourceMiddleware.SwitchDataSourceAsync(type);
                    var status = _dataSourceMiddleware.GetDataSourceStatus();
                    
                    results.Add(new
                    {
                        DataSourceType = type,
                        Success = true,
                        Status = status
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        DataSourceType = type,
                        Success = false,
                        Error = ex.Message
                    });
                }
            }
            
            return Ok(new
            {
                TestTime = DateTime.UtcNow,
                Results = results
            });
        }
    }
} 