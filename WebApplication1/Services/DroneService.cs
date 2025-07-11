using ClassLibrary1.Data;
using ClassLibrary1.Drone;
using ClassLibrary1.Tasks;
using System.Collections.Concurrent;
using WebApplication1.Middleware.Interfaces;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services
{
    /// <summary>
    /// 无人机服务实现
    /// </summary>
    public class DroneService : IDroneService
    {
        private readonly IDataService _dataService;
        private readonly IHistoryService _historyService;
        private readonly WebApplication1.Middleware.Interfaces.IDataSourceMiddleware _dataSourceMiddleware;
        private readonly ILogger<DroneService> _logger;
        private readonly Dictionary<Guid, DateTime> _lastHeartbeatTimes = new();
        private readonly object _heartbeatLock = new();
        // 内存缓存
        private readonly ConcurrentDictionary<Guid, Drone> _drones = new();
        private readonly ConcurrentDictionary<string, Guid> _droneNameMapping = new();
        // 新增：只缓存每台无人机的最新数据点
        private readonly ConcurrentDictionary<Guid, DroneDataPoint> _latestDroneDataCache = new();

        // 事件
        public event EventHandler<DroneChangedEventArgs>? DroneChanged;

        public DroneService(IDataService dataService, IHistoryService historyService, WebApplication1.Middleware.Interfaces.IDataSourceMiddleware dataSourceMiddleware, ILogger<DroneService> logger)
        {
            _dataService = dataService;
            _historyService = historyService;
            _dataSourceMiddleware = dataSourceMiddleware;
            _logger = logger;
        }

        #region 基础CRUD操作
        public async Task<List<Drone>> GetDronesAsync()
        {
            _logger.LogInformation("获取所有无人机");
            // 对于列表查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            return await _dataService.GetDronesAsync();
        }

        public async Task<Drone?> GetDroneAsync(Guid id)
        {
            _logger.LogInformation("获取无人机: {DroneId}", id);
            // 对于单个查询，使用混合策略
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.Hybrid);
            return await _dataService.GetDroneAsync(id);
        }

        public async Task<Drone?> GetDroneByNameAsync(string name)
        {
            _logger.LogInformation("根据名称获取无人机: {DroneName}", name);
            // 对于名称查询，使用缓存优先策略
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            return await _dataService.GetDroneByNameAsync(name);
        }

        public async Task<bool> AddDroneAsync(Drone drone)
        {
            _logger.LogInformation("添加无人机: {DroneName}", drone.Name);
            // 对于写操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            var result = await _dataService.AddDroneAsync(drone);
            if (result)
            {
                // 记录初始数据点
                var dataPoint = new DroneDataPoint
                {
                    Id = drone.Id,
                    Status = drone.Status,
                    Timestamp = DateTime.Now,
                    Latitude = (decimal)(drone.CurrentPosition?.Latitude_x ?? 0),
                    Longitude = (decimal)(drone.CurrentPosition?.Longitude_y ?? 0),
                    cpu_used_rate = drone.cpu_used_rate,
                    left_bandwidth = drone.left_bandwidth,
                    memory = drone.memory
                };
                await _historyService.AddDroneDataAsync(dataPoint);
            }
            return result;
        }

        public async Task<bool> UpdateDroneAsync(Drone drone)
        {
            _logger.LogInformation("更新无人机: {DroneName}", drone.Name);
            // 对于更新操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);

            // 1. 先写入数据点（只要有更新请求就写入）
            await AddDroneDataPointAsync(new DroneDataPoint
            {
                Id = drone.Id,
                Status = drone.Status,
                Timestamp = DateTime.Now,
                Latitude = (decimal)(drone.CurrentPosition?.Latitude_x ?? 0),
                Longitude = (decimal)(drone.CurrentPosition?.Longitude_y ?? 0),
                cpu_used_rate = drone.cpu_used_rate,
                left_bandwidth = drone.left_bandwidth,
                memory = drone.memory
            });

            // 2. 只更新属性，保持原有ID不变
            var result = await _dataService.UpdateDroneAsync(drone);

            // 3. 更新内存缓存
            _drones[drone.Id] = drone;
            _logger.LogDebug("更新内存中的无人机: {DroneName} (ID: {DroneId})", drone.Name, drone.Id);

            return result;
        }

        public async Task<bool> DeleteDroneAsync(Guid id)
        {
            _logger.LogInformation("删除无人机: {DroneId}", id);
            // 对于删除操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            _drones.TryRemove(id, out _);
            _droneNameMapping.TryRemove(_drones[id].Name, out _);
            OnDroneChanged("Delete", _drones[id]);
            return await _dataService.DeleteDroneAsync(id);
        }

        public async Task<int> GetDroneCountAsync()
        {
            _logger.LogInformation("获取无人机数量");
            // 对于统计查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            return await _dataService.GetDroneCountAsync();
        }
        #endregion

        #region 无人机状态管理
        public async Task<bool> UpdateDroneStatusAsync(Guid droneId, DroneStatus status)
        {
            _logger.LogInformation("更新无人机状态: {DroneId} -> {Status}", droneId, status);
            // 对于状态更新，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);

            var drone = await _dataService.GetDroneAsync(droneId);
            if (drone == null)
            {
                _logger.LogWarning("无人机不存在: {DroneId}", droneId);
                return false;
            }

            drone.Status = status;
            var result = await _dataService.UpdateDroneAsync(drone);

            if (result)
            {
                // 记录状态变更数据点
                var dataPoint = new DroneDataPoint
                {
                    Id = droneId,
                    Status = status,
                    Timestamp = DateTime.Now,
                    Latitude = (decimal)(drone.CurrentPosition?.Latitude_x ?? 0),
                    Longitude = (decimal)(drone.CurrentPosition?.Longitude_y ?? 0),
                    cpu_used_rate = drone.cpu_used_rate,
                    left_bandwidth = drone.left_bandwidth,
                    memory = drone.memory
                };
                await _historyService.AddDroneDataAsync(dataPoint);
            }

            return result;
        }

        public async Task<bool> UpdateDronePositionAsync(Guid droneId, decimal latitude, decimal longitude)
        {
            _logger.LogInformation("更新无人机位置: {DroneId} -> ({Latitude}, {Longitude})", droneId, latitude, longitude);
            // 对于位置更新，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);

            var drone = await _dataService.GetDroneAsync(droneId);
            if (drone == null)
            {
                _logger.LogWarning("无人机不存在: {DroneId}", droneId);
                return false;
            }

            drone.CurrentPosition = new GPSPosition((double)latitude, (double)longitude);
            var result = await _dataService.UpdateDroneAsync(drone);

            if (result)
            {
                // 记录位置数据点
                var dataPoint = new DroneDataPoint
                {
                    Id = droneId,
                    Status = drone.Status,
                    Timestamp = DateTime.Now,
                    Latitude = latitude,
                    Longitude = longitude,
                    cpu_used_rate = drone.cpu_used_rate,
                    left_bandwidth = drone.left_bandwidth,
                    memory = drone.memory
                };
                await _historyService.AddDroneDataAsync(dataPoint);
            }

            return result;
        }

        public async Task<bool> UpdateDroneMetricsAsync(Guid droneId, double cpuUsage, double memoryUsage, double bandwidth)
        {
            _logger.LogDebug("更新无人机指标: {DroneId} -> CPU:{CpuUsage}%, Memory:{MemoryUsage}%, Bandwidth:{Bandwidth}Mbps",
                droneId, cpuUsage, memoryUsage, bandwidth);
            // 对于指标更新，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);

            var drone = await _dataService.GetDroneAsync(droneId);
            if (drone == null)
            {
                _logger.LogWarning("无人机不存在: {DroneId}", droneId);
                return false;
            }

            drone.cpu_used_rate = cpuUsage;
            drone.memory = memoryUsage;
            drone.left_bandwidth = bandwidth;
            var result = await _dataService.UpdateDroneAsync(drone);

            if (result)
            {
                // 记录指标数据点
                var dataPoint = new DroneDataPoint
                {
                    Id = droneId,
                    Status = drone.Status,
                    Timestamp = DateTime.Now,
                    Latitude = (decimal)(drone.CurrentPosition?.Latitude_x ?? 0),
                    Longitude = (decimal)(drone.CurrentPosition?.Longitude_y ?? 0),
                    cpu_used_rate = cpuUsage,
                    left_bandwidth = bandwidth,
                    memory = memoryUsage
                };
                await _historyService.AddDroneDataAsync(dataPoint);
            }

            return result;
        }
        #endregion

        #region 无人机数据点管理
        // 写入数据点时，更新数据库和缓存
        public async Task<bool> AddDroneDataPointAsync(DroneDataPoint dataPoint)
        {
            // 对于数据点写入，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            
            // 1. 写入数据库
            var dbResult = await _historyService.AddDroneDataAsync(dataPoint);

            // 2. 更新内存缓存
            _latestDroneDataCache.AddOrUpdate(dataPoint.Id, dataPoint, (id, old) =>
                dataPoint.Timestamp > old.Timestamp ? dataPoint : old);

            return dbResult;
        }

        // 查询最新数据点（优先查缓存）
        public async Task<DroneDataPoint?> GetLatestDroneDataPointAsync(Guid droneId)
        {
            // 对于最新数据点查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            
            if (_latestDroneDataCache.TryGetValue(droneId, out var latest))
            {
                return latest;
            }
            // 缓存未命中，查数据库
            return await _historyService.GetLatestDroneDataPointAsync(droneId);
        }

        // 其他历史数据点查询依然查数据库
        public async Task<List<DroneDataPoint>> GetDroneDataPointsAsync(Guid droneId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取无人机数据点: {DroneId}, {StartTime} - {EndTime}", droneId, startTime, endTime);
            // 对于历史数据查询，使用混合策略
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.Hybrid);
            return await _historyService.GetDroneDataAsync(droneId, startTime, endTime);
        }

        public async Task<List<DroneDataPoint>> GetDroneDataPointsAsync(Guid droneId, DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            _logger.LogInformation("获取无人机数据点(分页): {DroneId}, {StartTime} - {EndTime}, 第{PageIndex}页, 每页{PageSize}条",
                droneId, startTime, endTime, pageIndex, pageSize);
            // 对于分页查询，使用混合策略
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.Hybrid);
            return await _historyService.GetDroneDataAsync(droneId, startTime, endTime);
        }

        public async Task<int> GetDroneDataPointsCountAsync(Guid droneId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取无人机数据点数量: {DroneId}, {StartTime} - {EndTime}", droneId, startTime, endTime);
            // 对于统计查询，使用缓存优先策略
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            return await _historyService.GetDroneDataCountAsync(droneId, startTime, endTime);
        }

        public async Task<List<DroneDataPoint>> GetAllDronesLatestDataPointsAsync()
        {
            _logger.LogInformation("获取所有无人机最新数据点");
            // 对于最新数据点查询，使用缓存优先策略
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            return await _historyService.GetAllDronesLatestDataPointsAsync();
        }
        #endregion

        #region 无人机任务分配
        public async Task<bool> AssignSubTaskToDroneAsync(Guid droneId, Guid subTaskId)
        {
            _logger.LogInformation("分配子任务到无人机: {SubTaskId} -> {DroneId}", subTaskId, droneId);
            return await _dataService.AssignSubTaskToDroneAsync(subTaskId, droneId);
        }

        public async Task<bool> UnassignSubTaskFromDroneAsync(Guid droneId, Guid subTaskId)
        {
            _logger.LogInformation("从无人机取消分配子任务: {SubTaskId} <- {DroneId}", subTaskId, droneId);
            return await _dataService.UnassignSubTaskFromDroneAsync(subTaskId, droneId);
        }

        public async Task<List<Guid>> GetAssignedSubTaskIdsAsync(Guid droneId)
        {
            _logger.LogInformation("获取无人机分配的子任务ID列表: {DroneId}", droneId);
            // 这里需要从数据库查询，暂时返回空列表
            return new List<Guid>();
        }

        public async Task<List<Guid>> GetAvailableDronesAsync()
        {
            _logger.LogInformation("获取可用无人机列表");
            // 对于可用性查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            var drones = await _dataService.GetDronesAsync();
            return drones.Where(d => d.Status == DroneStatus.Idle).Select(d => d.Id).ToList();
        }
        #endregion

        #region 无人机健康检查
        public async Task<List<Drone>> GetOfflineDronesAsync()
        {
            _logger.LogInformation("获取离线无人机列表");
            // 对于状态查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            var drones = await _dataService.GetDronesAsync();
            var offlineDrones = new List<Drone>();

            foreach (var drone in drones)
            {
                if (!await IsDroneOnlineAsync(drone.Id))
                {
                    offlineDrones.Add(drone);
                }
            }

            return offlineDrones;
        }

        public async Task<List<Drone>> GetOnlineDronesAsync()
        {
            _logger.LogInformation("获取在线无人机列表");
            // 对于状态查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            var drones = await _dataService.GetDronesAsync();
            var onlineDrones = new List<Drone>();

            foreach (var drone in drones)
            {
                if (await IsDroneOnlineAsync(drone.Id))
                {
                    onlineDrones.Add(drone);
                }
            }

            return onlineDrones;
        }

        public async Task<bool> IsDroneOnlineAsync(Guid droneId)
        {
            lock (_heartbeatLock)
            {
                if (_lastHeartbeatTimes.TryGetValue(droneId, out var lastHeartbeat))
                {
                    // 如果最后心跳时间超过5分钟，认为离线
                    return DateTime.Now - lastHeartbeat < TimeSpan.FromMinutes(5);
                }
            }

            // 如果没有心跳记录，从数据库获取最后心跳时间
            var drone = await _dataService.GetDroneAsync(droneId);
            if (drone != null)
            {
                // 这里需要从数据库获取LastHeartbeat字段，暂时返回true
                return true;
            }

            return false;
        }

        public async Task<TimeSpan> GetDroneOfflineDurationAsync(Guid droneId)
        {
            lock (_heartbeatLock)
            {
                if (_lastHeartbeatTimes.TryGetValue(droneId, out var lastHeartbeat))
                {
                    return DateTime.Now - lastHeartbeat;
                }
            }

            // 如果没有心跳记录，返回默认值
            return TimeSpan.Zero;
        }
        #endregion

        #region 批量操作
        public async Task<bool> BulkUpdateDronesAsync(IEnumerable<Drone> drones)
        {
            _logger.LogInformation("批量更新无人机: {Count}个", drones.Count());
            return await _dataService.BulkUpdateDronesAsync(drones);
        }

        public async Task<bool> BulkUpdateDroneStatusAsync(Dictionary<Guid, DroneStatus> statusUpdates)
        {
            _logger.LogInformation("批量更新无人机状态: {Count}个", statusUpdates.Count);

            var drones = await _dataService.GetDronesAsync();
            var dronesToUpdate = new List<Drone>();

            foreach (var update in statusUpdates)
            {
                var drone = drones.FirstOrDefault(d => d.Id == update.Key);
                if (drone != null)
                {
                    drone.Status = update.Value;
                    dronesToUpdate.Add(drone);
                }
            }

            return await _dataService.BulkUpdateDronesAsync(dronesToUpdate);
        }

        public Task<bool> UpdateDroneLastHeartbeatAsync(Guid droneId)
        {
            throw new NotImplementedException();
        }
        #endregion

        public void SetDrones(List<Drone> drones)
        {
            foreach (var drone in drones)
            {
                // 使用drone.name作为区分依据，查找是否已存在同名无人机
                if (_droneNameMapping.TryGetValue(drone.Name, out var existingId))
                {
                    // 如果已存在同名无人机，更新现有记录
                    if (_drones.TryGetValue(existingId, out var existingDrone))
                    {
                        // 更新现有无人机数据，保持原有ID
                        existingDrone.memory = drone.memory;
                        existingDrone.cpu_used_rate = drone.cpu_used_rate;
                        existingDrone.left_bandwidth = drone.left_bandwidth;
                        existingDrone.Status = drone.Status;
                        existingDrone.CurrentPosition = drone.CurrentPosition;
                        existingDrone.ModelStatus = drone.ModelStatus;
                        existingDrone.ModelType = drone.ModelType;
                        existingDrone.radius = drone.radius;
                        var updatedDrone = UpdateDroneAsync(existingDrone);

                        _logger.LogDebug("更新现有无人机: {DroneName} (ID: {DroneId})", drone.Name, existingId);
                    }
                }
                else
                {
                    // 如果不存在同名无人机，添加新记录
                    _drones.TryAdd(drone.Id, drone);
                    _droneNameMapping.TryAdd(drone.Name, drone.Id);
                    AddDroneAsync(drone);
                    _logger.LogDebug("添加新无人机: {DroneName} (ID: {DroneId})", drone.Name, drone.Id);
                }
            }
        }
        private void OnDroneChanged(string action, Drone drone)
        {
            try
            {
                DroneChanged?.Invoke(this, new DroneChangedEventArgs
                {
                    Action = action,
                    Drone = drone,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "触发无人机变更事件失败: {Action}", action);
            }
        }
    }
}
