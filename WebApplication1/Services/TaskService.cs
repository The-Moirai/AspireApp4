using ClassLibrary1.Data;
using ClassLibrary1.Tasks;
using WebApplication1.Middleware.Interfaces;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services
{
    /// <summary>
    /// 任务服务实现
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly IDataService _dataService;
        private readonly IHistoryService _historyService;
        private readonly WebApplication1.Middleware.Interfaces.IDataSourceMiddleware _dataSourceMiddleware;
        private readonly ILogger<TaskService> _logger;

        // 事件
        public event EventHandler<TaskChangedEventArgs>? TaskChanged;

        public TaskService(IDataService dataService, IHistoryService historyService, WebApplication1.Middleware.Interfaces.IDataSourceMiddleware dataSourceMiddleware, ILogger<TaskService> logger)
        {
            _dataService = dataService;
            _historyService = historyService;
            _dataSourceMiddleware = dataSourceMiddleware;
            _logger = logger;
        }

        #region 主任务管理
        public async Task<List<MainTask>> GetMainTasksAsync()
        {
            _logger.LogInformation("获取所有主任务");
            // 对于列表查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            return await _dataService.GetTasksAsync();
        }

        public async Task<MainTask?> GetMainTaskAsync(Guid id)
        {
            _logger.LogInformation("获取主任务: {TaskId}", id);
            // 对于单个查询，使用混合策略
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.Hybrid);
            return await _dataService.GetTaskAsync(id);
        }

        public async Task<bool> AddMainTaskAsync(MainTask task, string createdBy)
        {
            _logger.LogInformation("添加主任务: {TaskName}", task.Name);
            // 对于写操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            OnTaskChanged("Add", task); // 触发任务变更事件
            return await _dataService.AddTaskAsync(task, createdBy);
        }

        public async Task<bool> UpdateMainTaskAsync(MainTask task)
        {
            _logger.LogInformation("更新主任务: {TaskName}", task.Name);
            // 对于更新操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            return await _dataService.UpdateTaskAsync(task);
        }

        public async Task<bool> DeleteMainTaskAsync(Guid id)
        {
            _logger.LogInformation("删除主任务: {TaskId}", id);
            // 对于删除操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            return await _dataService.DeleteTaskAsync(id);
        }

        public async Task<int> GetMainTaskCountAsync()
        {
            _logger.LogInformation("获取主任务数量");
            // 对于统计查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            return await _dataService.GetTaskCountAsync();
        }
        /// <summary>
        /// 将子任务列表加载至主任务
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <param name="subTasks">子任务列表</param>
        /// <returns>操作结果</returns>
        public async Task<bool> LoadSubTasksToMainTaskAsync(Guid mainTaskId, SubTask subTask)
        {
            _logger.LogInformation("加载子任务至主任务: {MainTaskId}", mainTaskId, subTask);

            if (subTask == null )
            {
                _logger.LogWarning("子任务列表为空: {MainTaskId}", mainTaskId);
                return false;
            }
            var success = true;
            // 设置子任务所属的主任务
            subTask.ParentTask = mainTaskId;

            if (!await _dataService.AddSubTaskAsync(subTask))
            {
             _logger.LogError("添加子任务失败: {SubTaskId}", subTask.Id);
             success = false;
             }
            return success;
        }
        #endregion

        #region 子任务管理
        public async Task<List<SubTask>> GetSubTasksAsync(Guid mainTaskId)
        {
            _logger.LogInformation("获取主任务的子任务: {MainTaskId}", mainTaskId);
            // 对于列表查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            return await _dataService.GetSubTasksAsync(mainTaskId);
        }

        public async Task<SubTask?> GetSubTaskAsync(Guid mainTaskId, Guid subTaskId)
        {
            _logger.LogInformation("获取子任务: {MainTaskId}/{SubTaskId}", mainTaskId, subTaskId);
            // 对于单个查询，使用混合策略
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.Hybrid);
            return await _dataService.GetSubTaskAsync(mainTaskId, subTaskId);
        }

        public async Task<bool> AddSubTaskAsync(SubTask subTask)
        {
            _logger.LogInformation("添加子任务: {SubTaskDescription}", subTask.Description);
            // 对于写操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            return await _dataService.AddSubTaskAsync(subTask);
        }

        public async Task<bool> UpdateSubTaskAsync(SubTask subTask)
        {
            _logger.LogInformation("更新子任务: {SubTaskDescription}", subTask.Description);
            // 对于更新操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            return await _dataService.UpdateSubTaskAsync(subTask);
        }

        public async Task<bool> DeleteSubTaskAsync(Guid mainTaskId, Guid subTaskId)
        {
            _logger.LogInformation("删除子任务: {MainTaskId}/{SubTaskId}", mainTaskId, subTaskId);
            // 对于删除操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            return await _dataService.DeleteSubTaskAsync(mainTaskId, subTaskId);
        }

        public async Task<int> GetSubTaskCountAsync(Guid mainTaskId)
        {
            _logger.LogInformation("获取子任务数量: {MainTaskId}", mainTaskId);
            // 对于统计查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            var subTasks = await _dataService.GetSubTasksAsync(mainTaskId);
            return subTasks.Count;
        }
        #endregion

        #region 任务状态管理
        public async Task<bool> StartMainTaskAsync(Guid mainTaskId)
        {
            _logger.LogInformation("启动主任务: {TaskId}", mainTaskId);
            var task = await _dataService.GetTaskAsync(mainTaskId);
            if (task == null)
            {
                _logger.LogWarning("主任务不存在: {TaskId}", mainTaskId);
                return false;
            }

            task.Status = System.Threading.Tasks.TaskStatus.Running;
            task.StartTime = DateTime.Now;
            return await _dataService.UpdateTaskAsync(task);
        }

        public async Task<bool> CompleteMainTaskAsync(Guid mainTaskId)
        {
            _logger.LogInformation("完成主任务: {TaskId}", mainTaskId);
            var task = await _dataService.GetTaskAsync(mainTaskId);
            if (task == null)
            {
                _logger.LogWarning("主任务不存在: {TaskId}", mainTaskId);
                return false;
            }

            task.Status = System.Threading.Tasks.TaskStatus.RanToCompletion;
            task.CompletedTime = DateTime.Now;
            return await _dataService.UpdateTaskAsync(task);
        }

        public async Task<bool> CancelMainTaskAsync(Guid mainTaskId)
        {
            _logger.LogInformation("取消主任务: {TaskId}", mainTaskId);
            var task = await _dataService.GetTaskAsync(mainTaskId);
            if (task == null)
            {
                _logger.LogWarning("主任务不存在: {TaskId}", mainTaskId);
                return false;
            }

            task.Status = System.Threading.Tasks.TaskStatus.Canceled;
            task.CompletedTime = DateTime.Now;
            return await _dataService.UpdateTaskAsync(task);
        }

        public async Task<bool> StartSubTaskAsync(Guid subTaskId)
        {
            _logger.LogInformation("启动子任务: {SubTaskId}", subTaskId);
            // 需要先找到子任务所属的主任务
            var mainTasks = await _dataService.GetTasksAsync();
            foreach (var mainTask in mainTasks)
            {
                var subTask = mainTask.SubTasks.FirstOrDefault(st => st.Id == subTaskId);
                if (subTask != null)
                {
                    subTask.Status = System.Threading.Tasks.TaskStatus.Running;
                    subTask.AssignedTime = DateTime.Now;
                    return await _dataService.UpdateSubTaskAsync(subTask);
                }
            }

            _logger.LogWarning("子任务不存在: {SubTaskId}", subTaskId);
            return false;
        }

        public async Task<bool> CompleteSubTaskAsync(Guid subTaskId)
        {
            _logger.LogInformation("完成子任务: {SubTaskId}", subTaskId);
            var mainTasks = await _dataService.GetTasksAsync();
            foreach (var mainTask in mainTasks)
            {
                var subTask = mainTask.SubTasks.FirstOrDefault(st => st.Id == subTaskId);
                if (subTask != null)
                {
                    subTask.Status = System.Threading.Tasks.TaskStatus.RanToCompletion;
                    subTask.CompletedTime = DateTime.Now;
                    return await _dataService.UpdateSubTaskAsync(subTask);
                }
            }

            _logger.LogWarning("子任务不存在: {SubTaskId}", subTaskId);
            return false;
        }

        public async Task<bool> CancelSubTaskAsync(Guid subTaskId)
        {
            _logger.LogInformation("取消子任务: {SubTaskId}", subTaskId);
            var mainTasks = await _dataService.GetTasksAsync();
            foreach (var mainTask in mainTasks)
            {
                var subTask = mainTask.SubTasks.FirstOrDefault(st => st.Id == subTaskId);
                if (subTask != null)
                {
                    subTask.Status = System.Threading.Tasks.TaskStatus.Canceled;
                    subTask.CompletedTime = DateTime.Now;
                    return await _dataService.UpdateSubTaskAsync(subTask);
                }
            }

            _logger.LogWarning("子任务不存在: {SubTaskId}", subTaskId);
            return false;
        }

        public async Task<bool> ReassignSubTaskAsync(Guid subTaskId, Guid newDroneId)
        {
            _logger.LogInformation("重新分配子任务: {SubTaskId} -> {NewDroneId}", subTaskId, newDroneId);

            // 先取消当前分配
            await _dataService.UnassignSubTaskFromDroneAsync(subTaskId, Guid.Empty);

            // 重新分配
            return await _dataService.AssignSubTaskToDroneAsync(subTaskId, newDroneId);
        }
        #endregion

        #region 任务分配管理
        public async Task<bool> AssignSubTaskToDroneAsync(Guid subTaskId, Guid droneId)
        {
            _logger.LogInformation("分配子任务到无人机: {SubTaskId} -> {DroneId}", subTaskId, droneId);
            // 对于分配操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            return await _dataService.AssignSubTaskToDroneAsync(subTaskId, droneId);
        }

        public async Task<bool> UnassignSubTaskFromDroneAsync(Guid subTaskId, Guid droneId)
        {
            _logger.LogInformation("从无人机取消分配子任务: {SubTaskId} <- {DroneId}", subTaskId, droneId);
            // 对于取消分配操作，使用数据库优先策略确保数据一致性
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.DatabaseFirst);
            return await _dataService.UnassignSubTaskFromDroneAsync(subTaskId, droneId);
        }

        public async Task<List<SubTask>> GetAssignedSubTasksAsync(Guid mainTaskId, Guid droneId)
        {
            _logger.LogInformation("获取无人机分配的子任务: {MainTaskId}/{DroneId}", mainTaskId, droneId);
            // 对于查询操作，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            return await _dataService.GetAssignedSubTasksAsync(mainTaskId, droneId);
        }

        public async Task<List<SubTask>> GetUnassignedSubTasksAsync(Guid mainTaskId)
        {
            _logger.LogInformation("获取未分配的子任务: {MainTaskId}", mainTaskId);
            // 对于查询操作，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            return await _dataService.GetUnassignedSubTasksAsync(mainTaskId);
        }

        public async Task<List<SubTask>> GetSubTasksByStatusAsync(Guid mainTaskId, System.Threading.Tasks.TaskStatus status)
        {
            _logger.LogInformation("根据状态获取子任务: {MainTaskId}/{Status}", mainTaskId, status);
            // 对于查询操作，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            var subTasks = await _dataService.GetSubTasksAsync(mainTaskId);
            return subTasks.Where(st => st.Status == status).ToList();
        }
        #endregion

        #region 任务图片管理
        public async Task<Guid> SaveSubTaskImageAsync(Guid subTaskId, byte[] imageData, string fileName, int imageIndex = 1, string? description = null)
        {
            _logger.LogInformation("保存子任务图片: {SubTaskId}, {FileName}, 序号{ImageIndex}", subTaskId, fileName, imageIndex);
            return await _dataService.SaveImageAsync(subTaskId, imageData, fileName, imageIndex, description);
        }

        public async Task<List<SubTaskImage>> GetSubTaskImagesAsync(Guid subTaskId)
        {
            _logger.LogInformation("获取子任务图片: {SubTaskId}", subTaskId);
            return await _dataService.GetImagesAsync(subTaskId);
        }

        public async Task<SubTaskImage?> GetSubTaskImageAsync(Guid imageId)
        {
            _logger.LogInformation("获取图片: {ImageId}", imageId);
            return await _dataService.GetImageAsync(imageId);
        }

        public async Task<bool> DeleteSubTaskImageAsync(Guid imageId)
        {
            _logger.LogInformation("删除图片: {ImageId}", imageId);
            return await _dataService.DeleteImageAsync(imageId);
        }
        #endregion

        #region 任务历史记录
        public async Task<bool> AddSubTaskHistoryAsync(SubTaskDataPoint dataPoint)
        {
            _logger.LogInformation("添加子任务历史记录: {SubTaskId}", dataPoint.SubTaskId);
            return await _historyService.AddSubTaskDataAsync(dataPoint);
        }

        public async Task<List<SubTaskDataPoint>> GetSubTaskHistoryAsync(Guid subTaskId)
        {
            _logger.LogInformation("获取子任务历史记录: {SubTaskId}", subTaskId);
            return await _historyService.GetSubTaskHistoryAsync(subTaskId);
        }

        public async Task<List<SubTaskDataPoint>> GetSubTaskHistoryAsync(Guid subTaskId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取子任务历史记录: {SubTaskId}, {StartTime} - {EndTime}", subTaskId, startTime, endTime);
            return await _historyService.GetSubTaskHistoryAsync(subTaskId, startTime, endTime);
        }
        #endregion

        #region 任务统计
        public async Task<Dictionary<System.Threading.Tasks.TaskStatus, int>> GetMainTaskStatusStatisticsAsync()
        {
            _logger.LogInformation("获取主任务状态统计");
            // 对于统计查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            var tasks = await _dataService.GetTasksAsync();
            return tasks.GroupBy(t => t.Status)
                       .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<System.Threading.Tasks.TaskStatus, int>> GetSubTaskStatusStatisticsAsync(Guid mainTaskId)
        {
            _logger.LogInformation("获取子任务状态统计: {MainTaskId}", mainTaskId);
            // 对于统计查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            var subTasks = await _dataService.GetSubTasksAsync(mainTaskId);
            return subTasks.GroupBy(st => st.Status)
                          .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<double> GetMainTaskCompletionRateAsync(Guid mainTaskId)
        {
            _logger.LogInformation("获取主任务完成率: {MainTaskId}", mainTaskId);
            // 对于统计查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            var subTasks = await _dataService.GetSubTasksAsync(mainTaskId);
            if (subTasks.Count == 0) return 0.0;

            var completedCount = subTasks.Count(st => st.Status == System.Threading.Tasks.TaskStatus.RanToCompletion);
            return (double)completedCount / subTasks.Count * 100;
        }

        public async Task<TimeSpan> GetMainTaskDurationAsync(Guid mainTaskId)
        {
            _logger.LogInformation("获取主任务持续时间: {MainTaskId}", mainTaskId);
            // 对于统计查询，使用缓存优先策略以提高性能
            await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
            var task = await _dataService.GetTaskAsync(mainTaskId);
            if (task == null || !task.StartTime.HasValue) return TimeSpan.Zero;

            var endTime = task.CompletedTime ?? DateTime.Now;
            return endTime - task.StartTime.Value;
        }
        #endregion

        #region 批量操作
        public async Task<bool> BulkUpdateMainTasksAsync(IEnumerable<MainTask> tasks)
        {
            _logger.LogInformation("批量更新主任务: {Count}个", tasks.Count());
            return await _dataService.BulkUpdateTasksAsync(tasks);
        }

        public async Task<bool> BulkUpdateSubTasksAsync(IEnumerable<SubTask> subTasks)
        {
            _logger.LogInformation("批量更新子任务: {Count}个", subTasks.Count());
            var success = true;
            foreach (var subTask in subTasks)
            {
                if (!await _dataService.UpdateSubTaskAsync(subTask))
                {
                    success = false;
                }
            }
            return success;
        }

        public async Task<bool> BulkAssignSubTasksAsync(Guid mainTaskId, Dictionary<Guid, Guid> subTaskDroneAssignments)
        {
            _logger.LogInformation("批量分配子任务: {MainTaskId}, {Count}个", mainTaskId, subTaskDroneAssignments.Count);
            var success = true;
            foreach (var assignment in subTaskDroneAssignments)
            {
                if (!await _dataService.AssignSubTaskToDroneAsync(assignment.Key, assignment.Value))
                {
                    success = false;
                }
            }
            return success;
        }
        #endregion
        private void OnTaskChanged(string action, MainTask task)
        {
            try
            {
                TaskChanged?.Invoke(this, new TaskChangedEventArgs
                {
                    Action = action,
                    MainTask = task,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "触发任务变更事件失败: {Action}", action);
            }
        }
    }
}
