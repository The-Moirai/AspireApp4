using ClassLibrary1.Data;
using ClassLibrary1.Tasks;

namespace WebApplication1.Services.Interfaces
{
    /// <summary>
    /// 任务服务接口
    /// </summary>
    public interface ITaskService
    {
        // 主任务管理
        Task<List<MainTask>> GetMainTasksAsync();
        Task<MainTask?> GetMainTaskAsync(Guid id);
        Task<bool> AddMainTaskAsync(MainTask task, string createdBy);
        Task<bool> UpdateMainTaskAsync(MainTask task);
        Task<bool> DeleteMainTaskAsync(Guid id);
        Task<int> GetMainTaskCountAsync();
        Task<bool> LoadSubTasksToMainTaskAsync(Guid mainTaskId,SubTask subTasks);

        // 子任务管理
        Task<List<SubTask>> GetSubTasksAsync(Guid mainTaskId);
        Task<SubTask?> GetSubTaskAsync(Guid mainTaskId, Guid subTaskId);
        Task<bool> AddSubTaskAsync(SubTask subTask);
        Task<bool> UpdateSubTaskAsync(SubTask subTask);
        Task<bool> DeleteSubTaskAsync(Guid mainTaskId, Guid subTaskId);
        Task<int> GetSubTaskCountAsync(Guid mainTaskId);

        // 任务状态管理
        Task<bool> StartMainTaskAsync(Guid mainTaskId);
        Task<bool> CompleteMainTaskAsync(Guid mainTaskId);
        Task<bool> CancelMainTaskAsync(Guid mainTaskId);
        Task<bool> StartSubTaskAsync(Guid subTaskId);
        Task<bool> CompleteSubTaskAsync(Guid subTaskId);
        Task<bool> CancelSubTaskAsync(Guid subTaskId);
        Task<bool> ReassignSubTaskAsync(Guid subTaskId, Guid newDroneId);

        // 任务分配管理
        Task<bool> AssignSubTaskToDroneAsync(Guid subTaskId, Guid droneId);
        Task<bool> UnassignSubTaskFromDroneAsync(Guid subTaskId, Guid droneId);
        Task<List<SubTask>> GetAssignedSubTasksAsync(Guid mainTaskId, Guid droneId);
        Task<List<SubTask>> GetUnassignedSubTasksAsync(Guid mainTaskId);
        Task<List<SubTask>> GetSubTasksByStatusAsync(Guid mainTaskId, System.Threading.Tasks.TaskStatus status);

        // 任务图片管理
        Task<Guid> SaveSubTaskImageAsync(Guid subTaskId, byte[] imageData, string fileName, int imageIndex = 1, string? description = null);
        Task<List<SubTaskImage>> GetSubTaskImagesAsync(Guid subTaskId);
        Task<SubTaskImage?> GetSubTaskImageAsync(Guid imageId);
        Task<bool> DeleteSubTaskImageAsync(Guid imageId);

        // 任务历史记录
        Task<bool> AddSubTaskHistoryAsync(SubTaskDataPoint dataPoint);
        Task<List<SubTaskDataPoint>> GetSubTaskHistoryAsync(Guid subTaskId);
        Task<List<SubTaskDataPoint>> GetSubTaskHistoryAsync(Guid subTaskId, DateTime startTime, DateTime endTime);

        // 任务统计
        Task<Dictionary<System.Threading.Tasks.TaskStatus, int>> GetMainTaskStatusStatisticsAsync();
        Task<Dictionary<System.Threading.Tasks.TaskStatus, int>> GetSubTaskStatusStatisticsAsync(Guid mainTaskId);
        Task<double> GetMainTaskCompletionRateAsync(Guid mainTaskId);
        Task<TimeSpan> GetMainTaskDurationAsync(Guid mainTaskId);

        // 批量操作
        Task<bool> BulkUpdateMainTasksAsync(IEnumerable<MainTask> tasks);
        Task<bool> BulkUpdateSubTasksAsync(IEnumerable<SubTask> subTasks);
        Task<bool> BulkAssignSubTasksAsync(Guid mainTaskId, Dictionary<Guid, Guid> subTaskDroneAssignments);
        // 事件处理
        event EventHandler<TaskChangedEventArgs>? TaskChanged;
    }
}
