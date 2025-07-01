using ClassLibrary1.Data;

namespace WebApplication1.Services.Interfaces
{
    public interface IHistoryService
    {
        //无人机数据点相关操作

        /// <summary>
        /// 添加无人机数据点
        /// </summary>
        /// <param name="dataPoint"></param>
        /// <returns></returns>
        Task<bool> AddDroneDataAsync(DroneDataPoint dataPoint);

        /// <summary>
        /// 删除无人机数据点
        /// </summary>
        /// <param name="dataPointId"></param>
        /// <returns></returns>
        Task<bool> DeleteDroneDataAsync(Guid dataPointId);

        /// <summary>
        /// 更新无人机数据点
        /// </summary>
        /// <param name="dataPoint"></param>
        /// <returns></returns>
        Task<bool> UpdateDroneDataAsync(DroneDataPoint dataPoint);

        /// <summary>
        /// 获取指定ID的无人机数据点
        /// </summary>
        /// <param name="dataPointId"></param>
        /// <returns></returns>
        Task<DroneDataPoint?> GetDroneDataByIdAsync(Guid dataPointId);

        /// <summary>
        /// 获取指定无人机在指定时间范围内的数据点数量
        /// </summary>
        /// <param name="droneId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Task<int> GetDroneDataCountAsync(Guid droneId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 获取指定任务下指定无人机的数据点数量
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="droneId"></param>
        /// <returns></returns>
        Task<int> GetTaskDataCountAsync(Guid taskId, Guid droneId);
        /// <summary>
        /// 获取指定时间范围内所有无人机的数据点数量
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Task<int> GetAllDronesDataCountAsync(DateTime startTime, DateTime endTime);
        /// <summary>
        /// 获取指定无人机在指定时间范围内的数据点
        /// </summary>
        /// <param name="droneId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Task<List<DroneDataPoint>> GetDroneDataAsync(Guid droneId, DateTime startTime, DateTime endTime);
        /// <summary>
        /// 获取指定任务下指定无人机的数据点
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="droneId"></param>
        /// <returns></returns>
        Task<List<DroneDataPoint>> GetTaskDataAsync(Guid taskId, Guid droneId);
        /// <summary>
        /// 获取指定时间范围内所有无人机的数据点
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Task<List<DroneDataPoint>> GetAllDronesDataAsync(DateTime startTime, DateTime endTime);
        /// <summary>
        /// 获取指定时间范围内所有无人机的数据点，支持分页
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<List<DroneDataPoint>> GetAllDronesDataAsync(DateTime startTime, DateTime endTime, int pageIndex, int pageSize);



        //任务数据点相关操作
        /// <summary>
        /// 添加子任务数据点
        /// </summary>
        /// <param name="dataPoint"></param>
        /// <returns></returns>
        Task<bool> AddSubTaskDataAsync(SubTaskDataPoint dataPoint);
        /// <summary>
        /// 删除子任务数据点
        /// </summary>
        /// <param name="dataPointId"></param>
        /// <returns></returns>
        Task<bool> DeleteSubTaskDataAsync(Guid dataPointId);
        /// <summary>
        /// 更新子任务数据点
        /// </summary>
        /// <param name="dataPoint"></param>
        /// <returns></returns>
        Task<bool> UpdateSubTaskDataAsync(SubTaskDataPoint dataPoint);
        /// <summary>
        /// 获取指定ID的子任务数据点
        /// </summary>
        /// <param name="dataPointId"></param>
        /// <returns></returns>
        Task<SubTaskDataPoint?> GetSubTaskDataAsync(Guid dataPointId);
        /// <summary>
        /// 获取指定子任务在指定时间范围内的数据点数量
        /// </summary>
        /// <param name="subTaskId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Task<int> GetSubTaskDataCountAsync(Guid subTaskId, DateTime startTime, DateTime endTime);
    }
}
