using Microsoft.AspNetCore.Mvc;
using WebApplication1.Middleware.Interfaces;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// 数据源管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataSourceController : ControllerBase
    {
        private readonly WebApplication1.Middleware.Interfaces.IDataSourceMiddleware _dataSourceMiddleware;
        private readonly ILogger<DataSourceController> _logger;

        public DataSourceController(
            WebApplication1.Middleware.Interfaces.IDataSourceMiddleware dataSourceMiddleware,
            ILogger<DataSourceController> logger)
        {
            _dataSourceMiddleware = dataSourceMiddleware;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前数据源状态
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<DataSourceStatus>> GetStatus()
        {
            try
            {
                var status = _dataSourceMiddleware.GetDataSourceStatus();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取数据源状态失败");
                return StatusCode(500, new { error = "获取数据源状态失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取数据源配置
        /// </summary>
        [HttpGet("config")]
        public ActionResult<DataSourceConfig> GetConfig()
        {
            try
            {
                var config = _dataSourceMiddleware.GetDataSourceConfig();
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取数据源配置失败");
                return StatusCode(500, new { error = "获取数据源配置失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 更新数据源配置
        /// </summary>
        [HttpPut("config")]
        public async Task<ActionResult> UpdateConfig([FromBody] DataSourceConfig config)
        {
            try
            {
                await _dataSourceMiddleware.UpdateDataSourceConfigAsync(config);
                return Ok(new { message = "数据源配置更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新数据源配置失败");
                return StatusCode(500, new { error = "更新数据源配置失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 切换数据源类型
        /// </summary>
        [HttpPost("switch")]
        public async Task<ActionResult> SwitchDataSource([FromBody] DataSourceTypeRequest request)
        {
            try
            {
                await _dataSourceMiddleware.SwitchDataSourceAsync(request.DataSourceType);
                return Ok(new { message = $"数据源切换成功，当前类型：{request.DataSourceType}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换数据源失败");
                return StatusCode(500, new { error = "切换数据源失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        [HttpPost("cache/refresh")]
        public async Task<ActionResult> RefreshCache()
        {
            try
            {
                await _dataSourceMiddleware.RefreshCacheAsync();
                return Ok(new { message = "缓存刷新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新缓存失败");
                return StatusCode(500, new { error = "刷新缓存失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        [HttpPost("cache/clear")]
        public async Task<ActionResult> ClearCache()
        {
            try
            {
                await _dataSourceMiddleware.ClearCacheAsync();
                return Ok(new { message = "缓存清空成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空缓存失败");
                return StatusCode(500, new { error = "清空缓存失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 预热缓存
        /// </summary>
        [HttpPost("cache/warmup")]
        public async Task<ActionResult> WarmupCache()
        {
            try
            {
                await _dataSourceMiddleware.WarmupCacheAsync();
                return Ok(new { message = "缓存预热成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "预热缓存失败");
                return StatusCode(500, new { error = "预热缓存失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 检查数据源健康状态
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult<DataSourceHealthStatus>> CheckHealth()
        {
            try
            {
                var health = await _dataSourceMiddleware.CheckHealthAsync();
                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查数据源健康状态失败");
                return StatusCode(500, new { error = "检查数据源健康状态失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取当前数据源类型
        /// </summary>
        [HttpGet("type")]
        public ActionResult<DataSourceType> GetCurrentDataSourceType()
        {
            try
            {
                var dataSourceType = _dataSourceMiddleware.CurrentDataSource;
                return Ok(dataSourceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取当前数据源类型失败");
                return StatusCode(500, new { error = "获取当前数据源类型失败", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// 数据源类型切换请求
    /// </summary>
    public class DataSourceTypeRequest
    {
        /// <summary>
        /// 数据源类型
        /// </summary>
        public DataSourceType DataSourceType { get; set; }
    }
} 