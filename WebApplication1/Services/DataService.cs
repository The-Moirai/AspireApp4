using ClassLibrary1.Data;
using ClassLibrary1.Drone;
using ClassLibrary1.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;
using WebApplication1.Middleware.Interfaces;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class DataService : IDataService
    {
        private readonly ILogger<DataService> _logger;
        private readonly ISqlService _sqlService;
        private readonly ICacheService _cacheService;
        private readonly IDataSourceMiddleware _dataSourceMiddleware;

        public DataService(ILogger<DataService> logger, ISqlService sqlService, ICacheService cacheService, IDataSourceMiddleware dataSourceMiddleware)
        {
            _logger = logger;
            _sqlService = sqlService;
            _cacheService = cacheService;
            _dataSourceMiddleware = dataSourceMiddleware;
        }

        #region 无人机数据相关
        public async Task<List<ClassLibrary1.Drone.Drone>> GetDronesAsync()
        {
            _logger.LogInformation("获取所有无人机");
            var config = _dataSourceMiddleware.GetDataSourceConfig();
            var cacheKey = "drones:all";
            
            // 根据数据源配置决定是否使用缓存
            if (config.EnableCache)
            {
                var cached = await _cacheService.GetAsync<List<ClassLibrary1.Drone.Drone>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("命中无人机列表缓存");
                    return cached;
                }
            }
            
            // 如果禁用数据库，返回空列表
            if (!config.EnableDatabase)
            {
                _logger.LogWarning("数据库已禁用，无法获取无人机数据");
                return new List<ClassLibrary1.Drone.Drone>();
            }
            
            var query = "SELECT Id, Name, ModelStatus, ModelType, RegistrationDate FROM Drones";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var drones = new List<ClassLibrary1.Drone.Drone>();

            foreach (var row in results)
            {
                drones.Add(new ClassLibrary1.Drone.Drone
                {
                    Id = Guid.Parse(row["Id"].ToString()),
                    Name = row["Name"].ToString(),
                    ModelStatus = (ModelStatus)Convert.ToInt32(row["ModelStatus"]),
                    ModelType = row["ModelType"].ToString()
                });
            }
            
            // 如果启用缓存，将结果存入缓存
            if (config.EnableCache)
            {
                await _cacheService.SetAsync(cacheKey, drones, TimeSpan.FromMinutes(config.CacheExpiryMinutes));
            }
            
            return drones;
        }

        public async Task<ClassLibrary1.Drone.Drone?> GetDroneAsync(Guid id)
        {
            _logger.LogInformation("获取无人机id为：{id}", id);
            var cacheKey = $"drone:{id}";
            var cached = await _cacheService.GetAsync<ClassLibrary1.Drone.Drone>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("命中无人机缓存: {id}", id);
                return cached;
            }
            var query = $"SELECT Id, Name, ModelStatus, ModelType, RegistrationDate, LastHeartbeat FROM Drones WHERE Id = '{id}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var row = results.FirstOrDefault();

            if (row == null) return null;

            var drone = new ClassLibrary1.Drone.Drone
            {
                Id = Guid.Parse(row["Id"].ToString()),
                Name = row["Name"].ToString(),
                ModelStatus = (ModelStatus)Convert.ToInt32(row["ModelStatus"]),
                ModelType = row["ModelType"].ToString()
            };
            await _cacheService.SetAsync(cacheKey, drone, TimeSpan.FromMinutes(5));
            return drone;
        }

        public async Task<ClassLibrary1.Drone.Drone?> GetDroneByNameAsync(string droneName)
        {
            _logger.LogInformation("查询无人机名称为：{droneName}", droneName);
            var cacheKey = $"drone:byname:{droneName}";
            var cached = await _cacheService.GetAsync<ClassLibrary1.Drone.Drone>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("命中无人机名称缓存: {droneName}", droneName);
                return cached;
            }
            var query = $"SELECT Id, Name, ModelStatus, ModelType, RegistrationDate, LastHeartbeat FROM Drones WHERE Name = '{droneName}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var row = results.FirstOrDefault();

            if (row == null) return null;

            var drone = new ClassLibrary1.Drone.Drone
            {
                Id = Guid.Parse(row["Id"].ToString()),
                Name = row["Name"].ToString(),
                ModelStatus = (ModelStatus)Convert.ToInt32(row["ModelStatus"]),
                ModelType = row["ModelType"].ToString()
            };
            await _cacheService.SetAsync(cacheKey, drone, TimeSpan.FromMinutes(5));
            return drone;
        }

        public async Task<bool> AddDroneAsync(ClassLibrary1.Drone.Drone drone)
        {
            _logger.LogInformation("添加无人机：{drone}", drone);
            var command = $"INSERT INTO Drones (Id, Name, ModelStatus, ModelType, RegistrationDate) VALUES ('{drone.Id}', '{drone.Name}', {(int)drone.ModelStatus}, '{drone.ModelType}', GETUTCDATE())";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                await _cacheService.RemoveAsync("drones:all");
                await _cacheService.RemoveAsync($"drone:{drone.Id}");
                await _cacheService.RemoveAsync($"drone:byname:{drone.Name}");
            }
            return result > 0;
        }

        public async Task<bool> UpdateDroneAsync(ClassLibrary1.Drone.Drone drone)
        {
            _logger.LogInformation("更新无人机：{drone}", drone);
            var command = $"UPDATE Drones SET Name = '{drone.Name}', ModelStatus = {(int)drone.ModelStatus}, ModelType = '{drone.ModelType}' WHERE Id = '{drone.Id}'";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                await _cacheService.RemoveAsync("drones:all");
                await _cacheService.RemoveAsync($"drone:{drone.Id}");
                await _cacheService.RemoveAsync($"drone:byname:{drone.Name}");
            }
            return result > 0;
        }

        public async Task<bool> DeleteDroneAsync(Guid id)
        {
            _logger.LogInformation("删除无人机id为：{id}", id);
            var command = $"DELETE FROM Drones WHERE Id = '{id}'";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                await _cacheService.RemoveAsync("drones:all");
                await _cacheService.RemoveAsync($"drone:{id}");
            }
            return result > 0;
        }

        public async Task<int> GetDroneCountAsync()
        {
            _logger.LogInformation("获取无人机数量");
            var results = await _sqlService.ExecuteQueryAsync("SELECT COUNT(*) as Count FROM Drones");
            var row = results.FirstOrDefault();
            return row != null ? Convert.ToInt32(row["Count"]) : 0;
        }
        #endregion

        #region 任务数据相关
        public async Task<List<MainTask>> GetTasksAsync()
        {
            _logger.LogInformation("获取所有任务");
            var cacheKey = "tasks:all";
            var cached = await _cacheService.GetAsync<List<MainTask>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("命中主任务列表缓存");
                return cached;
            }
            var results = await _sqlService.ExecuteQueryAsync("SELECT Id, Name, Description, Status, CreationTime, StartTime, CompletedTime, CreatedBy FROM MainTasks");
            var tasks = new List<MainTask>();

            foreach (var row in results)
            {
                tasks.Add(new MainTask
                {
                    Id = Guid.Parse(row["Id"].ToString()),
                    Name = row["Name"]?.ToString() ?? "",
                    Description = row["Description"].ToString(),
                    Status = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["Status"]),
                    CreationTime = Convert.ToDateTime(row["CreationTime"]),
                    StartTime = row["StartTime"] != DBNull.Value ? Convert.ToDateTime(row["StartTime"]) : null,
                    CompletedTime = row["CompletedTime"] != DBNull.Value ? Convert.ToDateTime(row["CompletedTime"]) : null
                });
            }
            await _cacheService.SetAsync(cacheKey, tasks, TimeSpan.FromMinutes(5));
            return tasks;
        }

        public async Task<MainTask?> GetTaskAsync(Guid id)
        {
            _logger.LogInformation("获取任务id为：{id}", id);
            var cacheKey = $"task:{id}";
            var cached = await _cacheService.GetAsync<MainTask>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("命中主任务缓存: {id}", id);
                return cached;
            }
            var results = await _sqlService.ExecuteQueryAsync($"SELECT Id, Name, Description, Status, CreationTime, StartTime, CompletedTime, CreatedBy FROM MainTasks WHERE Id = '{id}'");
            var row = results.FirstOrDefault();

            if (row == null) return null;

            var task = new MainTask
            {
                Id = Guid.Parse(row["Id"].ToString()),
                Name = row["Name"]?.ToString() ?? "",
                Description = row["Description"].ToString(),
                Status = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["Status"]),
                CreationTime = Convert.ToDateTime(row["CreationTime"]),
                StartTime = row["StartTime"] != DBNull.Value ? Convert.ToDateTime(row["StartTime"]) : null,
                CompletedTime = row["CompletedTime"] != DBNull.Value ? Convert.ToDateTime(row["CompletedTime"]) : null
            };
            await _cacheService.SetAsync(cacheKey, task, TimeSpan.FromMinutes(5));
            return task;
        }

        public async Task<bool> AddTaskAsync(MainTask task, string createdBy)
        {
            _logger.LogInformation("添加任务：{task}", task);
            var command = $"INSERT INTO MainTasks (Id, Name, Description, Status, CreationTime, CreatedBy) VALUES ('{task.Id}', '{task.Name}', '{task.Description}', {(int)task.Status}, GETUTCDATE(), '{createdBy}')";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                await _cacheService.RemoveAsync("tasks:all");
                await _cacheService.RemoveAsync($"task:{task.Id}");
            }
            return result > 0;
        }

        public async Task<bool> UpdateTaskAsync(MainTask task)
        {
            _logger.LogInformation("更新任务：{task}", task);
            var command = $"UPDATE MainTasks SET Name = '{task.Name}', Description = '{task.Description}', Status = {(int)task.Status}, StartTime = {(task.StartTime.HasValue ? $"'{task.StartTime.Value:yyyy-MM-dd HH:mm:ss}'" : "NULL")}, CompletedTime = {(task.CompletedTime.HasValue ? $"'{task.CompletedTime.Value:yyyy-MM-dd HH:mm:ss}'" : "NULL")} WHERE Id = '{task.Id}'";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                await _cacheService.RemoveAsync("tasks:all");
                await _cacheService.RemoveAsync($"task:{task.Id}");
            }
            return result > 0;
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            _logger.LogInformation("删除任务id为：{id}", id);
            var command = $"DELETE FROM MainTasks WHERE Id = '{id}'";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                await _cacheService.RemoveAsync("tasks:all");
                await _cacheService.RemoveAsync($"task:{id}");
            }
            return result > 0;
        }

        public async Task<int> GetTaskCountAsync()
        {
            _logger.LogInformation("获取任务数量");
            var results = await _sqlService.ExecuteQueryAsync("SELECT COUNT(*) as Count FROM MainTasks");
            var row = results.FirstOrDefault();
            return row != null ? Convert.ToInt32(row["Count"]) : 0;
        }
        #endregion

        #region 子任务数据相关
        public async Task<List<SubTask>> GetSubTasksAsync(Guid mainTaskId)
        {
            _logger.LogInformation("获取指定主任务的所有子任务");
            var cacheKey = $"subtasks:{mainTaskId}";
            var cached = await _cacheService.GetAsync<List<SubTask>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("命中子任务列表缓存: {MainTaskId}", mainTaskId);
                return cached;
            }
            var results = await _sqlService.ExecuteQueryAsync($"SELECT Id, Description, Status, CreationTime, AssignedTime, CompletedTime, ParentTask, ReassignmentCount, AssignedDrone FROM SubTasks WHERE ParentTask = '{mainTaskId}'");
            var subTasks = new List<SubTask>();

            foreach (var row in results)
            {
                subTasks.Add(new SubTask
                {
                    Id = Guid.Parse(row["Id"].ToString()),
                    Description = row["Description"].ToString(),
                    Status = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["Status"]),
                    CreationTime = Convert.ToDateTime(row["CreationTime"]),
                    AssignedTime = row["AssignedTime"] != DBNull.Value ? Convert.ToDateTime(row["AssignedTime"]) : null,
                    CompletedTime = row["CompletedTime"] != DBNull.Value ? Convert.ToDateTime(row["CompletedTime"]) : null,
                    ParentTask = Guid.Parse(row["ParentTask"].ToString()),
                    ReassignmentCount = Convert.ToInt32(row["ReassignmentCount"]),
                    AssignedDrone = row["AssignedDrone"]?.ToString() ?? ""
                });
            }
            await _cacheService.SetAsync(cacheKey, subTasks, TimeSpan.FromMinutes(5));
            return subTasks;
        }

        public async Task<SubTask?> GetSubTaskAsync(Guid mainTaskId, Guid subTaskId)
        {
            _logger.LogInformation("获取指定主任务的指定子任务");
            var cacheKey = $"subtask:{mainTaskId}:{subTaskId}";
            var cached = await _cacheService.GetAsync<SubTask>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("命中子任务缓存: {MainTaskId}/{SubTaskId}", mainTaskId, subTaskId);
                return cached;
            }
            var results = await _sqlService.ExecuteQueryAsync($"SELECT Id, Description, Status, CreationTime, AssignedTime, CompletedTime, ParentTask, ReassignmentCount, AssignedDrone FROM SubTasks WHERE ParentTask = '{mainTaskId}' AND Id = '{subTaskId}'");
            var row = results.FirstOrDefault();

            if (row == null) return null;

            var subTask = new SubTask
            {
                Id = Guid.Parse(row["Id"].ToString()),
                Description = row["Description"].ToString(),
                Status = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["Status"]),
                CreationTime = Convert.ToDateTime(row["CreationTime"]),
                AssignedTime = row["AssignedTime"] != DBNull.Value ? Convert.ToDateTime(row["AssignedTime"]) : null,
                CompletedTime = row["CompletedTime"] != DBNull.Value ? Convert.ToDateTime(row["CompletedTime"]) : null,
                ParentTask = Guid.Parse(row["ParentTask"].ToString()),
                ReassignmentCount = Convert.ToInt32(row["ReassignmentCount"]),
                AssignedDrone = row["AssignedDrone"]?.ToString() ?? ""
            };
            await _cacheService.SetAsync(cacheKey, subTask, TimeSpan.FromMinutes(5));
            return subTask;
        }

        public async Task<bool> AddSubTaskAsync(SubTask subTask)
        {
            _logger.LogInformation("新子任务生成");
            var command = $"INSERT INTO SubTasks (Id, Description, Status, CreationTime, ParentTask, ReassignmentCount, AssignedDrone) VALUES ('{subTask.Id}', '{subTask.Description}', {(int)subTask.Status}, GETUTCDATE(), '{subTask.ParentTask}', {subTask.ReassignmentCount}, '{subTask.AssignedDrone}')";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                // 清理相关缓存
                await _cacheService.RemoveAsync($"subtasks:{subTask.ParentTask}");
                await _cacheService.RemoveAsync($"subtask:{subTask.ParentTask}:{subTask.Id}");
                await _cacheService.RemoveAsync($"assigned:{subTask.ParentTask}:*");
                await _cacheService.RemoveAsync($"unassigned:{subTask.ParentTask}");
            }
            return result > 0;
        }

        public async Task<bool> UpdateSubTaskAsync(SubTask subTask)
        {
            _logger.LogInformation("更新子任务：{subTask}", subTask);
            var command = $"UPDATE SubTasks SET Description = '{subTask.Description}', Status = {(int)subTask.Status}, AssignedTime = {(subTask.AssignedTime.HasValue ? $"'{subTask.AssignedTime.Value:yyyy-MM-dd HH:mm:ss}'" : "NULL")}, CompletedTime = {(subTask.CompletedTime.HasValue ? $"'{subTask.CompletedTime.Value:yyyy-MM-dd HH:mm:ss}'" : "NULL")}, ReassignmentCount = {subTask.ReassignmentCount}, AssignedDrone = '{subTask.AssignedDrone}' WHERE Id = '{subTask.Id}'";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                // 清理相关缓存
                await _cacheService.RemoveAsync($"subtasks:{subTask.ParentTask}");
                await _cacheService.RemoveAsync($"subtask:{subTask.ParentTask}:{subTask.Id}");
                await _cacheService.RemoveAsync($"assigned:{subTask.ParentTask}:*");
                await _cacheService.RemoveAsync($"unassigned:{subTask.ParentTask}");
            }
            return result > 0;
        }

        public async Task<bool> DeleteSubTaskAsync(Guid mainTaskId, Guid subTaskId)
        {
            _logger.LogInformation("删除子任务id为：{subTaskId}，主任务id为：{mainTaskId}", subTaskId, mainTaskId);
            var command = $"DELETE FROM SubTasks WHERE Id = '{subTaskId}' AND ParentTask = '{mainTaskId}'";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                // 清理相关缓存
                await _cacheService.RemoveAsync($"subtasks:{mainTaskId}");
                await _cacheService.RemoveAsync($"subtask:{mainTaskId}:{subTaskId}");
                await _cacheService.RemoveAsync($"assigned:{mainTaskId}:*");
                await _cacheService.RemoveAsync($"unassigned:{mainTaskId}");
                await _cacheService.RemoveAsync($"images:{subTaskId}");
            }
            return result > 0;
        }
        #endregion

        #region 无人机-子任务相关操作
        public async Task<bool> AssignSubTaskToDroneAsync(Guid subTaskId, Guid droneId)
        {
            _logger.LogInformation("将子任务id为：{subTaskId}分配给无人机id为：{droneId}", subTaskId, droneId);
            var command = $"INSERT INTO DroneSubTasks (DroneId, SubTaskId, AssignmentTime, IsActive) VALUES ('{droneId}', '{subTaskId}', GETUTCDATE(), 1)";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                // 清理相关缓存
                await _cacheService.RemoveAsync($"assigned:*:{droneId}");
                await _cacheService.RemoveAsync($"unassigned:*");
                // 清理子任务相关缓存
                var subTask = await GetSubTaskAsync(Guid.Empty, subTaskId); // 需要先查询获取ParentTask
                if (subTask != null)
                {
                    await _cacheService.RemoveAsync($"subtasks:{subTask.ParentTask}");
                    await _cacheService.RemoveAsync($"subtask:{subTask.ParentTask}:{subTaskId}");
                }
            }
            return result > 0;
        }

        public async Task<bool> UnassignSubTaskFromDroneAsync(Guid subTaskId, Guid droneId)
        {
            _logger.LogInformation("将子任务id为：{subTaskId}从无人机id为：{droneId}中移除", subTaskId, droneId);
            var command = $"UPDATE DroneSubTasks SET IsActive = 0 WHERE DroneId = '{droneId}' AND SubTaskId = '{subTaskId}'";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                // 清理相关缓存
                await _cacheService.RemoveAsync($"assigned:*:{droneId}");
                await _cacheService.RemoveAsync($"unassigned:*");
                // 清理子任务相关缓存
                var subTask = await GetSubTaskAsync(Guid.Empty, subTaskId); // 需要先查询获取ParentTask
                if (subTask != null)
                {
                    await _cacheService.RemoveAsync($"subtasks:{subTask.ParentTask}");
                    await _cacheService.RemoveAsync($"subtask:{subTask.ParentTask}:{subTaskId}");
                }
            }
            return result > 0;
        }

        public async Task<List<SubTask>> GetAssignedSubTasksAsync(Guid mainTaskId, Guid droneId)
        {
            _logger.LogInformation("获取无人机id为：{droneId}的已分配子任务", droneId);
            var cacheKey = $"assigned:{mainTaskId}:{droneId}";
            var cached = await _cacheService.GetAsync<List<SubTask>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("命中已分配子任务缓存: {MainTaskId}/{DroneId}", mainTaskId, droneId);
                return cached;
            }
            var results = await _sqlService.ExecuteQueryAsync($@"
                SELECT s.Id, s.Description, s.Status, s.CreationTime, s.AssignedTime, s.CompletedTime, s.ParentTask, s.ReassignmentCount, s.AssignedDrone 
                FROM SubTasks s 
                INNER JOIN DroneSubTasks dst ON s.Id = dst.SubTaskId 
                WHERE s.ParentTask = '{mainTaskId}' AND dst.DroneId = '{droneId}' AND dst.IsActive = 1");

            var subTasks = new List<SubTask>();
            foreach (var row in results)
            {
                subTasks.Add(new SubTask
                {
                    Id = Guid.Parse(row["Id"].ToString()),
                    Description = row["Description"].ToString(),
                    Status = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["Status"]),
                    CreationTime = Convert.ToDateTime(row["CreationTime"]),
                    AssignedTime = row["AssignedTime"] != DBNull.Value ? Convert.ToDateTime(row["AssignedTime"]) : null,
                    CompletedTime = row["CompletedTime"] != DBNull.Value ? Convert.ToDateTime(row["CompletedTime"]) : null,
                    ParentTask = Guid.Parse(row["ParentTask"].ToString()),
                    ReassignmentCount = Convert.ToInt32(row["ReassignmentCount"]),
                    AssignedDrone = row["AssignedDrone"]?.ToString() ?? ""
                });
            }
            await _cacheService.SetAsync(cacheKey, subTasks, TimeSpan.FromMinutes(5));
            return subTasks;
        }

        public async Task<List<SubTask>> GetUnassignedSubTasksAsync(Guid mainTaskId)
        {
            _logger.LogInformation("获取主任务id为：{mainTaskId}的未分配子任务", mainTaskId);
            var cacheKey = $"unassigned:{mainTaskId}";
            var cached = await _cacheService.GetAsync<List<SubTask>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("命中未分配子任务缓存: {MainTaskId}", mainTaskId);
                return cached;
            }
            var results = await _sqlService.ExecuteQueryAsync($@"
                SELECT s.Id, s.Description, s.Status, s.CreationTime, s.AssignedTime, s.CompletedTime, s.ParentTask, s.ReassignmentCount, s.AssignedDrone 
                FROM SubTasks s 
                LEFT JOIN DroneSubTasks dst ON s.Id = dst.SubTaskId AND dst.IsActive = 1
                WHERE s.ParentTask = '{mainTaskId}' AND dst.SubTaskId IS NULL");

            var subTasks = new List<SubTask>();
            foreach (var row in results)
            {
                subTasks.Add(new SubTask
                {
                    Id = Guid.Parse(row["Id"].ToString()),
                    Description = row["Description"].ToString(),
                    Status = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["Status"]),
                    CreationTime = Convert.ToDateTime(row["CreationTime"]),
                    AssignedTime = row["AssignedTime"] != DBNull.Value ? Convert.ToDateTime(row["AssignedTime"]) : null,
                    CompletedTime = row["CompletedTime"] != DBNull.Value ? Convert.ToDateTime(row["CompletedTime"]) : null,
                    ParentTask = Guid.Parse(row["ParentTask"].ToString()),
                    ReassignmentCount = Convert.ToInt32(row["ReassignmentCount"]),
                    AssignedDrone = row["AssignedDrone"]?.ToString() ?? ""
                });
            }
            await _cacheService.SetAsync(cacheKey, subTasks, TimeSpan.FromMinutes(5));
            return subTasks;
        }
        #endregion

        #region 图片相关操作
        public async Task<Guid> SaveImageAsync(Guid subTaskId, byte[] imageData, string fileName, int imageIndex = 1, string? description = null)
        {
            _logger.LogInformation("保存子任务id为：{subTaskId}的图片", subTaskId);
            var imageId = Guid.NewGuid();
            var fileExtension = Path.GetExtension(fileName);
            var fileSize = imageData.Length;
            var contentType = GetContentType(fileExtension);

            var command = $"INSERT INTO SubTaskImages (Id, SubTaskId, ImageData, FileName, FileExtension, FileSize, ContentType, ImageIndex, UploadTime, Description) VALUES ('{imageId}', '{subTaskId}', @ImageData, '{fileName}', '{fileExtension}', {fileSize}, '{contentType}', {imageIndex}, GETUTCDATE(), '{description ?? ""}')";
            var result = await _sqlService.ExecuteCommandWithImageAsync(command, imageData);
            if (result > 0)
            {
                // 清理相关缓存
                await _cacheService.RemoveAsync($"images:{subTaskId}");
                await _cacheService.RemoveAsync($"image:{imageId}");
            }
            return result > 0 ? imageId : Guid.Empty;
        }

        public async Task<List<SubTaskImage>> GetImagesAsync(Guid subTaskId)
        {
            _logger.LogInformation("获取子任务id为：{subTaskId}的所有图片", subTaskId);
            var cacheKey = $"images:{subTaskId}";
            var cached = await _cacheService.GetAsync<List<SubTaskImage>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("命中图片列表缓存: {SubTaskId}", subTaskId);
                return cached;
            }
            var results = await _sqlService.ExecuteQueryAsync($"SELECT Id, SubTaskId, ImageData, FileName, FileExtension, FileSize, ContentType, ImageIndex, UploadTime, Description FROM SubTaskImages WHERE SubTaskId = '{subTaskId}' ORDER BY ImageIndex");

            var images = new List<SubTaskImage>();
            foreach (var row in results)
            {
                images.Add(new SubTaskImage
                {
                    Id = Guid.Parse(row["Id"].ToString()),
                    SubTaskId = Guid.Parse(row["SubTaskId"].ToString()),
                    ImageData = (byte[])row["ImageData"],
                    FileName = row["FileName"].ToString(),
                    FileExtension = row["FileExtension"].ToString(),
                    FileSize = Convert.ToInt64(row["FileSize"]),
                    ContentType = row["ContentType"].ToString(),
                    ImageIndex = Convert.ToInt32(row["ImageIndex"]),
                    UploadTime = Convert.ToDateTime(row["UploadTime"]),
                    Description = row["Description"]?.ToString()
                });
            }
            await _cacheService.SetAsync(cacheKey, images, TimeSpan.FromMinutes(10));
            return images;
        }

        public async Task<SubTaskImage?> GetImageAsync(Guid imageId)
        {
            _logger.LogInformation("获取图片id为：{imageId}的图片", imageId);
            var cacheKey = $"image:{imageId}";
            var cached = await _cacheService.GetAsync<SubTaskImage>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("命中图片缓存: {ImageId}", imageId);
                return cached;
            }
            var results = await _sqlService.ExecuteQueryAsync($"SELECT Id, SubTaskId, ImageData, FileName, FileExtension, FileSize, ContentType, ImageIndex, UploadTime, Description FROM SubTaskImages WHERE Id = '{imageId}'");
            var row = results.FirstOrDefault();

            if (row == null) return null;

            var image = new SubTaskImage
            {
                Id = Guid.Parse(row["Id"].ToString()),
                SubTaskId = Guid.Parse(row["SubTaskId"].ToString()),
                ImageData = (byte[])row["ImageData"],
                FileName = row["FileName"].ToString(),
                FileExtension = row["FileExtension"].ToString(),
                FileSize = Convert.ToInt64(row["FileSize"]),
                ContentType = row["ContentType"].ToString(),
                ImageIndex = Convert.ToInt32(row["ImageIndex"]),
                UploadTime = Convert.ToDateTime(row["UploadTime"]),
                Description = row["Description"]?.ToString()
            };
            await _cacheService.SetAsync(cacheKey, image, TimeSpan.FromMinutes(10));
            return image;
        }

        public async Task<bool> DeleteImageAsync(Guid imageId)
        {
            _logger.LogInformation("删除图片id为：{imageId}的图片", imageId);
            var command = $"DELETE FROM SubTaskImages WHERE Id = '{imageId}'";
            var result = await _sqlService.ExecuteCommandAsync(command);
            if (result > 0)
            {
                // 清理相关缓存
                await _cacheService.RemoveAsync($"image:{imageId}");
                // 清理图片列表缓存（需要先查询获取SubTaskId）
                var image = await GetImageAsync(imageId);
                if (image != null)
                {
                    await _cacheService.RemoveAsync($"images:{image.SubTaskId}");
                }
            }
            return result > 0;
        }

        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
        #endregion

        #region 批量操作
        public async Task<bool> BulkUpdateDronesAsync(IEnumerable<ClassLibrary1.Drone.Drone> drones)
        {
            _logger.LogInformation("批量更新无人机数据");
            var command = "BEGIN TRANSACTION;";
            foreach (var drone in drones)
            {
                command += $"UPDATE Drones SET Name = '{drone.Name}', ModelStatus = {(int)drone.ModelStatus}, ModelType = '{drone.ModelType}' WHERE Id = '{drone.Id}';";
            }
            command += "COMMIT;";
            var result = await _sqlService.ExecuteCommandAsync(command);
            return result > 0;
        }

        public async Task<bool> BulkUpdateTasksAsync(IEnumerable<MainTask> tasks)
        {
            _logger.LogInformation("批量更新任务数据");
            var command = "BEGIN TRANSACTION;";
            foreach (var task in tasks)
            {
                command += $"UPDATE MainTasks SET Name = '{task.Name}', Description = '{task.Description}', Status = {(int)task.Status} WHERE Id = '{task.Id}';";
            }
            command += "COMMIT;";
            var result = await _sqlService.ExecuteCommandAsync(command);
            return result > 0;
        }

        public async Task<bool> UpdateDroneLastHeartbeatAsync(Guid droneId)
        {
            _logger.LogInformation("更新无人机最后心跳时间：{droneId}", droneId);
            var command = $"UPDATE Drones SET LastHeartbeat = GETUTCDATE() WHERE Id = '{droneId}'";
            var result = await _sqlService.ExecuteCommandAsync(command);
            return result > 0;
        }

        public async Task<bool> UpdateDroneStatusAsync(Guid droneId, DroneStatus status)
        {
            _logger.LogInformation("更新无人机状态：{droneId} -> {status}", droneId, status);
            var command = $"UPDATE Drones SET LastHeartbeat = GETUTCDATE() WHERE Id = '{droneId}'";
            var result = await _sqlService.ExecuteCommandAsync(command);
            return result > 0;
        }
        #endregion
    }
}