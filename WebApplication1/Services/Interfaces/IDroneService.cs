using ClassLibrary1.Data;
using ClassLibrary1.Drone;

namespace WebApplication1.Services.Interfaces
{
    /// <summary>
    /// 无人机服务接口
    /// </summary>
    public interface IDroneService
    {
        // 基础CRUD操作
        Task<List<Drone>> GetDronesAsync();
        Task<Drone?> GetDroneAsync(Guid id);
        Task<Drone?> GetDroneByNameAsync(string name);
        Task<bool> AddDroneAsync(Drone drone);
        Task<bool> UpdateDroneAsync(Drone drone);
        Task<bool> DeleteDroneAsync(Guid id);
        Task<int> GetDroneCountAsync();

        // 无人机状态管理
        Task<bool> UpdateDroneStatusAsync(Guid droneId, DroneStatus status);
        Task<bool> UpdateDroneLastHeartbeatAsync(Guid droneId);
        Task<bool> UpdateDronePositionAsync(Guid droneId, decimal latitude, decimal longitude);
        Task<bool> UpdateDroneMetricsAsync(Guid droneId, double cpuUsage, double memoryUsage, double bandwidth);

        // 无人机数据点管理
        Task<bool> AddDroneDataPointAsync(DroneDataPoint dataPoint);
        Task<List<DroneDataPoint>> GetDroneDataPointsAsync(Guid droneId, DateTime startTime, DateTime endTime);
        Task<List<DroneDataPoint>> GetDroneDataPointsAsync(Guid droneId, DateTime startTime, DateTime endTime, int pageIndex, int pageSize);
        Task<int> GetDroneDataPointsCountAsync(Guid droneId, DateTime startTime, DateTime endTime);
        Task<DroneDataPoint?> GetLatestDroneDataPointAsync(Guid droneId);
        Task<List<DroneDataPoint>> GetAllDronesLatestDataPointsAsync();

        // 无人机任务分配
        Task<bool> AssignSubTaskToDroneAsync(Guid droneId, Guid subTaskId);
        Task<bool> UnassignSubTaskFromDroneAsync(Guid droneId, Guid subTaskId);
        Task<List<Guid>> GetAssignedSubTaskIdsAsync(Guid droneId);
        Task<List<Guid>> GetAvailableDronesAsync();

        // 无人机健康检查
        Task<List<Drone>> GetOfflineDronesAsync();
        Task<List<Drone>> GetOnlineDronesAsync();
        Task<bool> IsDroneOnlineAsync(Guid droneId);
        Task<TimeSpan> GetDroneOfflineDurationAsync(Guid droneId);

        // 批量操作
        Task<bool> BulkUpdateDronesAsync(IEnumerable<Drone> drones);
        Task<bool> BulkUpdateDroneStatusAsync(Dictionary<Guid, DroneStatus> statusUpdates);
        //特殊操作
        void SetDrones(List<Drone> drones);
        // 事件处理
        event EventHandler<DroneChangedEventArgs>? DroneChanged;
    }
}
