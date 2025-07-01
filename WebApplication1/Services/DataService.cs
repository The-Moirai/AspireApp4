using ClassLibrary1.Data;
using ClassLibrary1.Drone;
using ClassLibrary1.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services
{
    public class DataService : WebApplication1.Services.Interfaces.IDataService
    {
        private readonly ILogger<DataService> _logger;
        private readonly ISqlService _sqlService;
        public DataService(ILogger<DataService> logger)
        {
            _logger = logger;
        }
        #region 无人机数据相关
        public async Task<List<ClassLibrary1.Drone.Drone>> GetDronesAsync()
        {
            _logger.LogInformation("获取所有无人机");
            var query = $"SELECT * FROM DroneData ";
            var result= await _sqlService.ExecuteQueryAsync(query);
            return result.FirstOrDefault()?.Tolist() ?? 0;
        }
        public async Task<Drone?> GetDroneAsync(Guid id)
        {
            _logger.LogInformation("获取无人机id为：{id}", id);
            var mergeSql = "SELECT Id, Name, ModelStatus, ModelType, RegistrationDate, LastHeartbeat FROM Drones";

            var results = await _sqlService.ExecuteQueryAsync("SELECT * FROM Drones;");
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        public async Task<Drone?> GetDroneByNameAsync(string droneName)
        {
            _logger.LogInformation("查询无人机名称为：{droneName}", droneName);
            var results = await _sqlService.ExecuteQueryAsync($"SELECT * FROM Drones WHERE Name = '{droneName}';");
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        public Task<bool> AddDroneAsync(Drone drone)
        {
            _logger.LogInformation("添加无人机：{drone}", drone);
            var command = $"INSERT INTO Drones (Id, Name, ModelStatus, ModelType, RegistrationDate, LastHeartbeat) VALUES ('{drone.Id}', '{drone.Name}', '{drone.ModelStatus}', '{drone.ModelType}');";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public Task<bool> UpdateDroneAsync(Drone drone)
        {
            _logger.LogInformation("更新无人机：{drone}", drone);
            var command = $"UPDATE Drones SET Name = '{drone.Name}', ModelStatus = '{drone.ModelStatus}', ModelType = '{drone.ModelType}' WHERE Id = '{drone.Id}';";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public Task<bool> DeleteDroneAsync(Guid id)
        {
            _logger.LogInformation("删除无人机id为：{id}", id);
            var command = $"DELETE FROM Drones WHERE Id = '{id}';";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public Task<int> GetDroneCountAsync()
        {
            _logger.LogInformation("获取无人机数量");
            return _sqlService.ExecuteQueryAsync("SELECT COUNT(*) FROM Drones;").ContinueWith(t => (int)t.Result.FirstOrDefault()?["COUNT(*)"]);
        }
        #endregion
        #region 任务数据相关
        public async Task<List<MainTask>> GetTasksAsync()
        {
            _logger.LogInformation("获取所有任务");
            var results = await _sqlService.ExecuteQueryAsync("SELECT * FROM Tasks;");
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        public async Task<MainTask?> GetTaskAsync(Guid id)
        {
            _logger.LogInformation("获取任务id为：{id}", id);
            var results=await _sqlService.ExecuteQueryAsync($"SELECT * FROM Tasks WHERE Id = '{id}';");
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        public Task<bool> AddTaskAsync(MainTask task, string createdBy)
        {
            _logger.LogInformation("添加任务：{task}", task);
            var command = $"INSERT INTO Tasks (Id, Name, Description, CreatedBy, CreatedDate) VALUES ('{task.Id}', '{task.Name}', '{task.Description}', '{createdBy}');";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public Task<bool> UpdateTaskAsync(MainTask task)
        {
            _logger.LogInformation("更新任务：{task}", task);
            var command = $"UPDATE Tasks SET Name = '{task.Name}', Description = '{task.Description}' WHERE Id = '{task.Id}';";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public Task<bool> DeleteTaskAsync(Guid id)
        {
            _logger.LogInformation("删除任务id为：{id}", id);
            var command = $"DELETE FROM Tasks WHERE Id = '{id}';";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public Task<int> GetTaskCountAsync()
        {
            _logger.LogInformation("获取任务数量");
            return _sqlService.ExecuteQueryAsync("SELECT COUNT(*) FROM Tasks;").ContinueWith(t => (int)t.Result.FirstOrDefault()?["COUNT(*)"]);
        }
        #endregion
        #region 子任务数据相关
        public async Task<List<SubTask>> GetSubTasksAsync(Guid mainTaskId)
        {
            _logger.LogInformation("获取指定主任务的所有子任务");
            var results = await _sqlService.ExecuteQueryAsync($"SELECT * FROM SubTasks WHERE ParentTask = '{mainTaskId}';");
            return results.FirstOrDefault()?.Tolist() ?? 0;

        }
        public async Task<SubTask?> GetSubTaskAsync(Guid mainTaskId, Guid subTaskId)
        {
            _logger.LogInformation("获取指定主任务的指定子任务");
            var results = await _sqlService.ExecuteQueryAsync($"SELECT * FROM SubTasks WHERE ParentTask = '{mainTaskId}';");
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        public Task<bool> AddSubTaskAsync(SubTask subTask)
        {
            _logger.LogInformation("新子任务生成");
            var command = $"INSERT INTO SubTasks (Id, Description, Status, CreationTime，ParentTask) VALUES ('{subTask.Id}','{subTask.Description}', '{subTask.Status}', '{subTask.CreationTime}','{subTask.ParentTask}');";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public Task<bool> UpdateSubTaskAsync(SubTask subTask)
        {
            _logger.LogInformation("更新子任务：{subTask}", subTask);
            var command = $"UPDATE SubTasks SET Description = '{subTask.Description}', Status = '{subTask.Status}' WHERE Id = '{subTask.Id}';";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public Task<bool> DeleteSubTaskAsync(Guid mainTaskId, Guid subTaskId)
        {
            _logger.LogInformation("删除子任务id为：{subTaskId}，主任务id为：{mainTaskId}", subTaskId, mainTaskId);
            var command = $"DELETE FROM SubTasks WHERE Id = '{subTaskId}' AND ParentTask = '{mainTaskId}';";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        #endregion
        #region 无人机-子任务相关操作
        public Task<bool> AssignSubTaskToDroneAsync(Guid subTaskId, Guid droneId)
        {
            _logger.LogInformation("将子任务id为：{subTaskId}分配给无人机id为：{droneId}", subTaskId, droneId);
            var command = $"INSERT INTO DroneSubTasks (DroneId, SubTaskId) VALUES ('{droneId}', '{subTaskId}');";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public Task<bool> UnassignSubTaskFromDroneAsync(Guid subTaskId, Guid droneId)
        {   _logger.LogInformation("将子任务id为：{subTaskId}从无人机id为：{droneId}中移除", subTaskId, droneId);
            var command = $"DELETE FROM DroneSubTasks WHERE DroneId = '{droneId}' AND SubTaskId = '{subTaskId}';";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public async Task<List<SubTask>> GetAssignedSubTasksAsync(Guid mainTaskId, Guid droneId)
        {
            _logger.LogInformation("获取无人机id为：{droneId}的已分配子任务", droneId);
            var results = await _sqlService.ExecuteQueryAsync($"SELECT * FROM SubTasks WHERE ParentTask = '{mainTaskId}' AND Id IN (SELECT SubTaskId FROM DroneSubTasks WHERE DroneId = '{droneId}');");
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        public async Task<List<SubTask>> GetUnassignedSubTasksAsync(Guid mainTaskId)
        {
            _logger.LogInformation("获取主任务id为：{mainTaskId}的未分配子任务", mainTaskId);
            var results = await _sqlService.ExecuteQueryAsync($"SELECT * FROM SubTasks WHERE ParentTask = '{mainTaskId}' AND Id NOT IN (SELECT SubTaskId FROM DroneSubTasks);");
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        #endregion
        #region 图片相关操作
        public Task<Guid> SaveImageAsync(Guid subTaskId, byte[] imageData, string fileName, int imageIndex = 1, string? description = null)
        {
            _logger.LogInformation("保存子任务id为：{subTaskId}的图片", subTaskId);
            var imageId = Guid.NewGuid();
            var command = $"INSERT INTO SubTaskImages (Id, SubTaskId, ImageData, FileName, ImageIndex, Description) VALUES ('{imageId}', '{subTaskId}', @ImageData, '{fileName}', {imageIndex}, '{description}');";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => imageId);
        }
        public async Task<List<SubTaskImage>> GetImagesAsync(Guid subTaskId)
        {
            _logger.LogInformation("获取子任务id为：{subTaskId}的所有图片", subTaskId);
            var results=await _sqlService.ExecuteQueryAsync($"SELECT * FROM SubTaskImages WHERE SubTaskId = '{subTaskId}';");
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        public async Task<SubTaskImage?> GetImageAsync(Guid imageId)
        {
            _logger.LogInformation("获取图片id为：{imageId}的图片", imageId);
            var results = await _sqlService.ExecuteQueryAsync($"SELECT * FROM SubTaskImages WHERE Id = '{imageId}';");
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        public Task<bool> DeleteImageAsync(Guid imageId)
        {
            _logger.LogInformation("删除图片id为：{imageId}的图片", imageId);
            var command = $"DELETE FROM SubTaskImages WHERE Id = '{imageId}';";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        #endregion
        #region 批量操作
        public Task<bool> BulkUpdateDronesAsync(IEnumerable<Drone> drones)
        {
            _logger.LogInformation("批量更新无人机数据");
            var command = "BEGIN TRANSACTION;";
            foreach (var drone in drones)
            {
                command += $"UPDATE Drones SET Name = '{drone.Name}', ModelStatus = '{drone.ModelStatus}', ModelType = '{drone.ModelType}' WHERE Id = '{drone.Id}';";
            }
            command += "COMMIT;";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        public Task<bool> BulkUpdateTasksAsync(IEnumerable<MainTask> tasks)
        {
            _logger.LogInformation("批量更新任务数据");
            var command = "BEGIN TRANSACTION;";
            foreach (var task in tasks)
            {
                command += $"UPDATE Tasks SET Name = '{task.Name}', Description = '{task.Description}' WHERE Id = '{task.Id}';";
            }
            command += "COMMIT;";
            return _sqlService.ExecuteCommandAsync(command).ContinueWith(t => t.Result > 0);
        }
        #endregion
    }
}