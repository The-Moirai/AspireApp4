using ClassLibrary1.Data;
using ClassLibrary1.Drone;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// 无人机管理API控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DronesController : ControllerBase
    {
        private readonly IDroneService _droneService;
        private readonly ILogger<DronesController> _logger;

        public DronesController(IDroneService droneService, ILogger<DronesController> logger)
        {
            _droneService = droneService;
            _logger = logger;
        }

        #region 基础CRUD操作
        /// <summary>
        /// 获取所有无人机
        /// </summary>
        /// <returns>无人机列表</returns>
        [HttpGet]
        public async Task<ActionResult<List<Drone>>> GetDrones()
        {
            try
            {
                var drones = await _droneService.GetDronesAsync();
                return Ok(drones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取无人机列表失败");
                return StatusCode(500, new { error = "获取无人机列表失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 根据ID获取无人机
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <returns>无人机信息</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Drone>> GetDrone(Guid id)
        {
            try
            {
                var drone = await _droneService.GetDroneAsync(id);
                if (drone == null)
                {
                    return NotFound(new { error = "无人机不存在", id });
                }
                return Ok(drone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取无人机失败: {DroneId}", id);
                return StatusCode(500, new { error = "获取无人机失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 根据名称获取无人机
        /// </summary>
        /// <param name="name">无人机名称</param>
        /// <returns>无人机信息</returns>
        [HttpGet("byname/{name}")]
        public async Task<ActionResult<Drone>> GetDroneByName(string name)
        {
            try
            {
                var drone = await _droneService.GetDroneByNameAsync(name);
                if (drone == null)
                {
                    return NotFound(new { error = "无人机不存在", name });
                }
                return Ok(drone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据名称获取无人机失败: {DroneName}", name);
                return StatusCode(500, new { error = "获取无人机失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 添加无人机
        /// </summary>
        /// <param name="drone">无人机信息</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<ActionResult<bool>> AddDrone([FromBody] Drone drone)
        {
            try
            {
                if (drone == null)
                {
                    return BadRequest(new { error = "无人机信息不能为空" });
                }

                var result = await _droneService.AddDroneAsync(drone);
                if (result)
                {
                    return CreatedAtAction(nameof(GetDrone), new { id = drone.Id }, new { success = true, id = drone.Id });
                }
                return BadRequest(new { error = "添加无人机失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加无人机失败: {DroneName}", drone?.Name);
                return StatusCode(500, new { error = "添加无人机失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 更新无人机
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <param name="drone">无人机信息</param>
        /// <returns>操作结果</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<bool>> UpdateDrone(Guid id, [FromBody] Drone drone)
        {
            try
            {
                if (drone == null)
                {
                    return BadRequest(new { error = "无人机信息不能为空" });
                }

                if (id != drone.Id)
                {
                    return BadRequest(new { error = "ID不匹配" });
                }

                var result = await _droneService.UpdateDroneAsync(drone);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "更新无人机失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新无人机失败: {DroneId}", id);
                return StatusCode(500, new { error = "更新无人机失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 删除无人机
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<bool>> DeleteDrone(Guid id)
        {
            try
            {
                var result = await _droneService.DeleteDroneAsync(id);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return NotFound(new { error = "无人机不存在或删除失败", id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除无人机失败: {DroneId}", id);
                return StatusCode(500, new { error = "删除无人机失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取无人机数量
        /// </summary>
        /// <returns>无人机数量</returns>
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetDroneCount()
        {
            try
            {
                var count = await _droneService.GetDroneCountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取无人机数量失败");
                return StatusCode(500, new { error = "获取无人机数量失败", message = ex.Message });
            }
        }
        #endregion

        #region 状态管理
        /// <summary>
        /// 更新无人机状态
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <param name="status">新状态</param>
        /// <returns>操作结果</returns>
        [HttpPut("{id:guid}/status")]
        public async Task<ActionResult<bool>> UpdateDroneStatus(Guid id, [FromBody] DroneStatus status)
        {
            try
            {
                var result = await _droneService.UpdateDroneStatusAsync(id, status);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "更新无人机状态失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新无人机状态失败: {DroneId} -> {Status}", id, status);
                return StatusCode(500, new { error = "更新无人机状态失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 更新无人机位置
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <param name="request">位置信息</param>
        /// <returns>操作结果</returns>
        [HttpPut("{id:guid}/position")]
        public async Task<ActionResult<bool>> UpdateDronePosition(Guid id, [FromBody] UpdatePositionRequest request)
        {
            try
            {
                var result = await _droneService.UpdateDronePositionAsync(id, request.Latitude, request.Longitude);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "更新无人机位置失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新无人机位置失败: {DroneId} -> ({Latitude}, {Longitude})", id, request.Latitude, request.Longitude);
                return StatusCode(500, new { error = "更新无人机位置失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 更新无人机指标
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <param name="request">指标信息</param>
        /// <returns>操作结果</returns>
        [HttpPut("{id:guid}/metrics")]
        public async Task<ActionResult<bool>> UpdateDroneMetrics(Guid id, [FromBody] UpdateMetricsRequest request)
        {
            try
            {
                var result = await _droneService.UpdateDroneMetricsAsync(id, request.CpuUsage, request.MemoryUsage, request.Bandwidth);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "更新无人机指标失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新无人机指标失败: {DroneId}", id);
                return StatusCode(500, new { error = "更新无人机指标失败", message = ex.Message });
            }
        }
        #endregion

        #region 数据点管理
        /// <summary>
        /// 获取无人机数据点
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>数据点列表</returns>
        [HttpGet("{id:guid}/datapoints")]
        public async Task<ActionResult<List<DroneDataPoint>>> GetDroneDataPoints(
            Guid id,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            try
            {
                var dataPoints = await _droneService.GetDroneDataPointsAsync(id, startTime, endTime);
                return Ok(dataPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取无人机数据点失败: {DroneId}", id);
                return StatusCode(500, new { error = "获取无人机数据点失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取无人机数据点（分页）
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>分页数据点</returns>
        [HttpGet("{id:guid}/datapoints/paged")]
        public async Task<ActionResult<PagedResult<DroneDataPoint>>> GetDroneDataPointsPaged(
            Guid id,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var dataPoints = await _droneService.GetDroneDataPointsAsync(id, startTime, endTime, pageIndex, pageSize);
                var totalCount = await _droneService.GetDroneDataPointsCountAsync(id, startTime, endTime);
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var result = new PagedResult<DroneDataPoint>
                {
                    Data = dataPoints,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取无人机数据点失败: {DroneId}", id);
                return StatusCode(500, new { error = "获取无人机数据点失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取最新无人机数据点
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <returns>最新数据点</returns>
        [HttpGet("{id:guid}/latest-datapoint")]
        public async Task<ActionResult<DroneDataPoint>> GetLatestDroneDataPoint(Guid id)
        {
            try
            {
                var dataPoint = await _droneService.GetLatestDroneDataPointAsync(id);
                if (dataPoint == null)
                {
                    return NotFound(new { error = "未找到无人机数据点", id });
                }
                return Ok(dataPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取最新无人机数据点失败: {DroneId}", id);
                return StatusCode(500, new { error = "获取最新无人机数据点失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取所有无人机的最新全数据
        /// </summary>
        /// <returns>所有无人机的最新数据点列表</returns>
        [HttpGet("latest-datapoints")]
        public async Task<ActionResult<List<DroneDataPoint>>> GetAllDronesLatestDataPoints()
        {
            try
            {
                var dataPoints = await _droneService.GetAllDronesLatestDataPointsAsync();
                return Ok(dataPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有无人机最新数据点失败");
                return StatusCode(500, new { error = "获取所有无人机最新数据点失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 添加无人机数据点
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <param name="dataPoint">数据点信息</param>
        /// <returns>操作结果</returns>
        [HttpPost("{id:guid}/datapoints")]
        public async Task<ActionResult<bool>> AddDroneDataPoint(Guid id, [FromBody] DroneDataPoint dataPoint)
        {
            try
            {
                if (dataPoint == null)
                {
                    return BadRequest(new { error = "数据点信息不能为空" });
                }

                dataPoint.Id = id; // 确保ID匹配
                var result = await _droneService.AddDroneDataPointAsync(dataPoint);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "添加数据点失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加无人机数据点失败: {DroneId}", id);
                return StatusCode(500, new { error = "添加数据点失败", message = ex.Message });
            }
        }
        #endregion

        #region 任务分配
        /// <summary>
        /// 分配子任务到无人机
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <param name="subTaskId">子任务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("{id:guid}/assign/{subTaskId:guid}")]
        public async Task<ActionResult<bool>> AssignSubTask(Guid id, Guid subTaskId)
        {
            try
            {
                var result = await _droneService.AssignSubTaskToDroneAsync(id, subTaskId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "分配子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分配子任务失败: {SubTaskId} -> {DroneId}", subTaskId, id);
                return StatusCode(500, new { error = "分配子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 取消分配子任务
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <param name="subTaskId">子任务ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("{id:guid}/assign/{subTaskId:guid}")]
        public async Task<ActionResult<bool>> UnassignSubTask(Guid id, Guid subTaskId)
        {
            try
            {
                var result = await _droneService.UnassignSubTaskFromDroneAsync(id, subTaskId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "取消分配子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消分配子任务失败: {SubTaskId} <- {DroneId}", subTaskId, id);
                return StatusCode(500, new { error = "取消分配子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取无人机分配的子任务ID列表
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <returns>子任务ID列表</returns>
        [HttpGet("{id:guid}/assigned-tasks")]
        public async Task<ActionResult<List<Guid>>> GetAssignedSubTaskIds(Guid id)
        {
            try
            {
                var subTaskIds = await _droneService.GetAssignedSubTaskIdsAsync(id);
                return Ok(subTaskIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取无人机分配的子任务失败: {DroneId}", id);
                return StatusCode(500, new { error = "获取分配的子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取可用无人机列表
        /// </summary>
        /// <returns>可用无人机ID列表</returns>
        [HttpGet("available")]
        public async Task<ActionResult<List<Guid>>> GetAvailableDrones()
        {
            try
            {
                var availableDrones = await _droneService.GetAvailableDronesAsync();
                return Ok(availableDrones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取可用无人机列表失败");
                return StatusCode(500, new { error = "获取可用无人机列表失败", message = ex.Message });
            }
        }
        #endregion

        #region 健康检查
        /// <summary>
        /// 获取在线无人机列表
        /// </summary>
        /// <returns>在线无人机列表</returns>
        [HttpGet("online")]
        public async Task<ActionResult<List<Drone>>> GetOnlineDrones()
        {
            try
            {
                var onlineDrones = await _droneService.GetOnlineDronesAsync();
                return Ok(onlineDrones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取在线无人机列表失败");
                return StatusCode(500, new { error = "获取在线无人机列表失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取离线无人机列表
        /// </summary>
        /// <returns>离线无人机列表</returns>
        [HttpGet("offline")]
        public async Task<ActionResult<List<Drone>>> GetOfflineDrones()
        {
            try
            {
                var offlineDrones = await _droneService.GetOfflineDronesAsync();
                return Ok(offlineDrones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取离线无人机列表失败");
                return StatusCode(500, new { error = "获取离线无人机列表失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 检查无人机是否在线
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <returns>在线状态</returns>
        [HttpGet("{id:guid}/online")]
        public async Task<ActionResult<bool>> IsDroneOnline(Guid id)
        {
            try
            {
                var isOnline = await _droneService.IsDroneOnlineAsync(id);
                return Ok(new { isOnline });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查无人机在线状态失败: {DroneId}", id);
                return StatusCode(500, new { error = "检查在线状态失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取无人机离线持续时间
        /// </summary>
        /// <param name="id">无人机ID</param>
        /// <returns>离线持续时间</returns>
        [HttpGet("{id:guid}/offline-duration")]
        public async Task<ActionResult<TimeSpan>> GetDroneOfflineDuration(Guid id)
        {
            try
            {
                var duration = await _droneService.GetDroneOfflineDurationAsync(id);
                return Ok(new { duration = duration.TotalSeconds });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取无人机离线持续时间失败: {DroneId}", id);
                return StatusCode(500, new { error = "获取离线持续时间失败", message = ex.Message });
            }
        }
        #endregion

        #region 批量操作
        /// <summary>
        /// 批量更新无人机
        /// </summary>
        /// <param name="drones">无人机列表</param>
        /// <returns>操作结果</returns>
        [HttpPut("bulk")]
        public async Task<ActionResult<bool>> BulkUpdateDrones([FromBody] List<Drone> drones)
        {
            try
            {
                if (drones == null || !drones.Any())
                {
                    return BadRequest(new { error = "无人机列表不能为空" });
                }

                var result = await _droneService.BulkUpdateDronesAsync(drones);
                if (result)
                {
                    return Ok(new { success = true, count = drones.Count });
                }
                return BadRequest(new { error = "批量更新无人机失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新无人机失败: {Count}个", drones?.Count);
                return StatusCode(500, new { error = "批量更新无人机失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 批量更新无人机状态
        /// </summary>
        /// <param name="statusUpdates">状态更新字典</param>
        /// <returns>操作结果</returns>
        [HttpPut("bulk/status")]
        public async Task<ActionResult<bool>> BulkUpdateDroneStatus([FromBody] Dictionary<Guid, DroneStatus> statusUpdates)
        {
            try
            {
                if (statusUpdates == null || !statusUpdates.Any())
                {
                    return BadRequest(new { error = "状态更新信息不能为空" });
                }

                var result = await _droneService.BulkUpdateDroneStatusAsync(statusUpdates);
                if (result)
                {
                    return Ok(new { success = true, count = statusUpdates.Count });
                }
                return BadRequest(new { error = "批量更新无人机状态失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新无人机状态失败: {Count}个", statusUpdates?.Count);
                return StatusCode(500, new { error = "批量更新无人机状态失败", message = ex.Message });
            }
        }
        #endregion
    }

    #region 请求模型
    /// <summary>
    /// 更新位置请求
    /// </summary>
    public class UpdatePositionRequest
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    /// <summary>
    /// 更新指标请求
    /// </summary>
    public class UpdateMetricsRequest
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double Bandwidth { get; set; }
    }

    /// <summary>
    /// 分页结果
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
    #endregion
}
