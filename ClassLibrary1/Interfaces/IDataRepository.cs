using ClassLibrary1.Data;
using ClassLibrary1.Entity_Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Interfaces
{

    /// <summary>
    /// 数据仓库接口 - 定义基本的CRUD操作
    /// </summary>
    public interface IDataRepository<T> where T : class
    {
        /// <summary>
        /// 获取所有实体
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// 添加实体
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// 更新实体
        /// </summary>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// 删除实体
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// 检查实体是否存在
        /// </summary>
        Task<bool> ExistsAsync(Guid id);
    }

    /// <summary>
    /// 无人机数据仓库接口
    /// </summary>
    public interface IDroneRepository : IDataRepository<DroneEntity>
    {
        /// <summary>
        /// 根据名称获取无人机
        /// </summary>
        Task<DroneEntity?> GetByNameAsync(string name);

        /// <summary>
        /// 获取所有活跃的无人机
        /// </summary>
        Task<IEnumerable<DroneEntity>> GetActiveDronesAsync();

        /// <summary>
        /// 更新无人机心跳时间
        /// </summary>
        Task UpdateHeartbeatAsync(Guid droneId, DateTime heartbeatTime);

        /// <summary>
        /// 获取无人机状态历史
        /// </summary>
        Task<IEnumerable<DroneDataPoint>> GetStatusHistoryAsync(Guid droneId, DateTime? startTime = null, DateTime? endTime = null);

        /// <summary>
        /// 添加无人机状态历史记录
        /// </summary>
        Task<DroneDataPoint> AddStatusHistoryAsync(DroneDataPoint statusPoint);
    }

    /// <summary>
    /// 主任务数据仓库接口
    /// </summary>
    public interface IMainTaskRepository : IDataRepository<MainTaskEntity>
    {
        /// <summary>
        /// 根据状态获取任务
        /// </summary>
        Task<IEnumerable<MainTaskEntity>> GetByStatusAsync(TaskStatus status);

        /// <summary>
        /// 获取任务及其子任务
        /// </summary>
        Task<MainTaskEntity?> GetWithSubTasksAsync(Guid taskId);

        /// <summary>
        /// 更新任务状态
        /// </summary>
        Task<bool> UpdateStatusAsync(Guid taskId, TaskStatus status, DateTime? startTime = null, DateTime? completedTime = null);
    }

    /// <summary>
    /// 子任务数据仓库接口
    /// </summary>
    public interface ISubTaskRepository : IDataRepository<SubTaskEntity>
    {
        /// <summary>
        /// 根据父任务ID获取子任务
        /// </summary>
        Task<IEnumerable<SubTaskEntity>> GetByParentTaskAsync(Guid parentTaskId);

        /// <summary>
        /// 根据状态获取子任务
        /// </summary>
        Task<IEnumerable<SubTaskEntity>> GetByStatusAsync(TaskStatus status);

        /// <summary>
        /// 分配子任务给无人机
        /// </summary>
        Task<bool> AssignToDroneAsync(Guid subTaskId, string droneName);

        /// <summary>
        /// 更新子任务状态
        /// </summary>
        Task<bool> UpdateStatusAsync(Guid subTaskId, TaskStatus status, DateTime? assignedTime = null, DateTime? completedTime = null);

        /// <summary>
        /// 获取子任务历史记录
        /// </summary>
        Task<IEnumerable<SubTaskDataPoint>> GetHistoryAsync(Guid subTaskId);

        /// <summary>
        /// 添加子任务历史记录
        /// </summary>
        Task<SubTaskDataPoint> AddHistoryAsync(SubTaskDataPoint historyPoint);
    }

    /// <summary>
    /// 子任务图片数据仓库接口
    /// </summary>
    public interface ISubTaskImageRepository : IDataRepository<SubTaskImageEntity>
    {
        /// <summary>
        /// 根据子任务ID获取图片
        /// </summary>
        Task<IEnumerable<SubTaskImageEntity>> GetBySubTaskIdAsync(Guid subTaskId);

        /// <summary>
        /// 获取图片数据
        /// </summary>
        Task<byte[]?> GetImageDataAsync(Guid imageId);

        /// <summary>
        /// 上传图片
        /// </summary>
        Task<SubTaskImageEntity> UploadImageAsync(Guid subTaskId, byte[] imageData, string fileName, string contentType);
    }

    /// <summary>
    /// 无人机-子任务关联数据仓库接口
    /// </summary>
    public interface IDroneSubTaskRepository : IDataRepository<DroneSubTaskEntity>
    {
        /// <summary>
        /// 获取无人机的活跃任务
        /// </summary>
        Task<IEnumerable<SubTaskEntity>> GetActiveTasksForDroneAsync(Guid droneId);

        /// <summary>
        /// 分配任务给无人机
        /// </summary>
        Task<bool> AssignTaskToDroneAsync(Guid droneId, Guid subTaskId);

        /// <summary>
        /// 取消无人机任务分配
        /// </summary>
        Task<bool> UnassignTaskFromDroneAsync(Guid droneId, Guid subTaskId);

        /// <summary>
        /// 获取任务的所有分配记录
        /// </summary>
        Task<IEnumerable<DroneSubTaskEntity>> GetAssignmentsForTaskAsync(Guid subTaskId);
    }
}
