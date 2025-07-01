using ClassLibrary1.Data;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly ILogger<SqlService> _logger;
        private readonly ISqlService _sqlService;
        public HistoryService(ILogger<SqlService> logger, ISqlService sqlService)
        {
            _logger = logger;
            _sqlService = sqlService;
        }
        #region 无人机数据点相关操作
        public async Task<bool> AddDroneDataAsync(DroneDataPoint dataPoint)
        {
            _logger.LogInformation("添加无人机数据点：{DataPoint}", dataPoint);
            string command = $"INSERT INTO DroneData (DroneId, Timestamp, Latitude, Longitude, Altitude) VALUES ('{dataPoint.Id}', '{dataPoint.Timestamp:yyyy-MM-dd HH:mm:ss}', {dataPoint.Latitude}, {dataPoint.Longitude})";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<bool> DeleteDroneDataAsync(Guid dataPointId)
        {
            _logger.LogInformation("删除无人机数据点：DataPointId={DataPointId}", dataPointId);
            string command = $"DELETE FROM DroneData WHERE Id = '{dataPointId}'";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<bool> UpdateDroneDataAsync(DroneDataPoint dataPoint)
        {
            _logger.LogInformation("更新无人机数据点：{DataPoint}", dataPoint);
            string command = $"UPDATE DroneData SET Timestamp = '{dataPoint.Timestamp:yyyy-MM-dd HH:mm:ss}', Latitude = {dataPoint.Latitude}, Longitude = {dataPoint.Longitude} WHERE Id = '{dataPoint.Id}'";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<IEnumerable<dynamic>> GetHistoryDataAsync(string droneId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取无人机数据点：DroneId={DroneId}, StartTime={StartTime}, EndTime={EndTime}", droneId, startTime, endTime);
            string query = $"SELECT * FROM DroneData WHERE DroneId = '{droneId}' AND Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
            return await _sqlService.ExecuteQueryAsync(query);
        }
        public async Task<DroneDataPoint?> GetDroneDataByIdAsync(Guid dataPointId)
        {
            _logger.LogInformation("获取无人机数据点：DataPointId={DataPointId}", dataPointId);
            string query = $"SELECT * FROM DroneData WHERE Id = '{dataPointId}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            return results.FirstOrDefault() as DroneDataPoint;
        }
        public async Task<int> GetDroneDataCountAsync(Guid droneId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取无人机数据点数量：DroneId={DroneId}, StartTime={StartTime}, EndTime={EndTime}", droneId, startTime, endTime);
            string query = $"SELECT COUNT(*) FROM DroneData WHERE DroneId = '{droneId}' AND Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            return results.FirstOrDefault()?.Count ?? 0;
        }
        public async Task<int> GetTaskDataCountAsync(Guid taskId, Guid droneId)
        {
            _logger.LogInformation("获取任务下无人机数据点数量：TaskId={TaskId}, DroneId={DroneId}", taskId, droneId);
            string query = $"SELECT COUNT(*) FROM DroneData WHERE TaskId = '{taskId}' AND DroneId = '{droneId}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            return results.FirstOrDefault()?.Count ?? 0;
        }
        public async Task<int> GetAllDronesDataCountAsync(DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取所有无人机数据点数量：StartTime={StartTime}, EndTime={EndTime}", startTime, endTime);
            string query = $"SELECT COUNT(*) FROM DroneData WHERE Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            return results.FirstOrDefault()?.Count ?? 0;
        }
        public async Task<List<DroneDataPoint>> GetDroneDataAsync(Guid droneId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取无人机数据点：DroneId={DroneId}, StartTime={StartTime}, EndTime={EndTime}", droneId, startTime, endTime);
            string query = $"SELECT * FROM DroneData WHERE DroneId = '{droneId}' AND Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            return results.Select(r => r as DroneDataPoint).ToList();
        }
        public async Task<List<DroneDataPoint>> GetTaskDataAsync(Guid taskId, Guid droneId)
        {
            _logger.LogInformation("获取无人机数据点：DroneId={DroneId},taskId={taskId}", droneId, taskId);
            string query_time = $"SELECT StartTime，CompletedTime FROM MainTasks WHERE Id='{taskId}'";
            var times = await _sqlService.ExecuteQueryAsync(query_time);
            var startTime = times?[0];
            var endTime = times?[1];
            string query = $"SELECT COUNT(*) FROM DroneData WHERE Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        public async Task<List<DroneDataPoint>> GetAllDronesDataAsync(DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取所有无人机数据点：StartTime={StartTime}, EndTime={EndTime}", startTime, endTime);
            string query = $"SELECT * FROM DroneData WHERE Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }
        public async Task<List<DroneDataPoint>> GetAllDronesDataAsync(DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            _logger.LogInformation("获取所有无人机数据点：StartTime={StartTime}, EndTime={EndTime}", startTime, endTime);
            string query = $"SELECT COUNT(*) FROM DroneData WHERE Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            return results.FirstOrDefault()?.Tolist() ?? 0;
        }

        #endregion
        #region 任务数据点相关操作
        public async Task<bool> AddSubTaskDataAsync(SubTaskDataPoint dataPoint)
        {
            _logger.LogInformation("添加子任务数据点：{DataPoint}", dataPoint);
            string command = $"INSERT INTO DroneData (DroneId, Timestamp, Latitude, Longitude, Altitude) VALUES ('{dataPoint.SubTaskId}', '{dataPoint.Timestamp:yyyy-MM-dd HH:mm:ss}', {dataPoint.OldStatus}, {dataPoint.NewStatus}, {dataPoint.Reason})";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<bool> DeleteSubTaskDataAsync(Guid dataPointId)
        {
            _logger.LogInformation("删除子任务数据点：{dataPointId}", dataPointId);
            string command = $"DELETE FROM DroneData WHERE Id = '{dataPointId}'";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<bool> UpdateSubTaskDataAsync(SubTaskDataPoint dataPoint)
        {
            _logger.LogInformation("更新任务数据点：{DataPoint}", dataPoint);
            string command = $"UPDATE DroneData SET Timestamp = '{dataPoint.Timestamp:yyyy-MM-dd HH:mm:ss}',  WHERE Id = '{dataPoint.SubTaskId}'";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<SubTaskDataPoint?> GetSubTaskDataAsync(Guid dataPointId)
        {
            _logger.LogInformation("获取任务数据点：DataPointId={DataPointId}", dataPointId);
            string query = $"SELECT * FROM DroneData WHERE Id = '{dataPointId}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            return results.FirstOrDefault() as SubTaskDataPoint;
        }
        public async Task<int> GetSubTaskDataCountAsync(Guid subTaskId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取任务数据点：DataPointId={DataPointId}", subTaskId);
            string query = $"SELECT * FROM DroneData WHERE Id = '{subTaskId}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            return results.FirstOrDefault()?.Count ?? 0;
        }
        #endregion
    }
}
// End of namespace WebApplication1.Services