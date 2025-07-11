using ClassLibrary1.Drone;
using ClassLibrary1.Tasks;
using ClassLibrary1.Data;

namespace WebApplication1.Services.Interfaces
{
    public interface IDataService
    {
        // 无人机相关操作
        Task<List<Drone>> GetDronesAsync();
        Task<Drone?> GetDroneAsync(Guid id);
        Task<Drone?> GetDroneByNameAsync(string droneName);
        Task<bool> AddDroneAsync(Drone drone);
        Task<bool> UpdateDroneAsync(Drone drone);
        Task<bool> DeleteDroneAsync(Guid id);
        Task<int> GetDroneCountAsync();

        // 任务相关操作
        Task<List<MainTask>> GetTasksAsync();
        Task<MainTask?> GetTaskAsync(Guid id);
        Task<bool> AddTaskAsync(MainTask task, string createdBy);
        Task<bool> UpdateTaskAsync(MainTask task);
        Task<bool> DeleteTaskAsync(Guid id);
        Task<int> GetTaskCountAsync();

        // 子任务相关操作
        Task<List<SubTask>> GetSubTasksAsync(Guid mainTaskId);
        Task<SubTask?> GetSubTaskAsync(Guid mainTaskId, Guid subTaskId);
        Task<bool> AddSubTaskAsync(SubTask subTask);
        Task<bool> UpdateSubTaskAsync(SubTask subTask);
        Task<bool> DeleteSubTaskAsync(Guid mainTaskId, Guid subTaskId);

        //无人机-子任务相关操作
        Task<bool> AssignSubTaskToDroneAsync(Guid subTaskId, Guid droneId);
        Task<bool> UnassignSubTaskFromDroneAsync(Guid subTaskId, Guid droneId);
        Task<List<SubTask>> GetAssignedSubTasksAsync(Guid mainTaskId,Guid droneId);
        Task<List<SubTask>> GetUnassignedSubTasksAsync(Guid mainTaskId);

        // 图片相关操作
        Task<Guid> SaveImageAsync(Guid subTaskId, byte[] imageData, string fileName, int imageIndex = 1, string? description = null);
        Task<List<SubTaskImage>> GetImagesAsync(Guid subTaskId);
        Task<SubTaskImage?> GetImageAsync(Guid imageId);
        Task<bool> DeleteImageAsync(Guid imageId);

        // 批量操作
        Task<bool> BulkUpdateDronesAsync(IEnumerable<Drone> drones);
        Task<bool> BulkUpdateTasksAsync(IEnumerable<MainTask> tasks);

    }
}
