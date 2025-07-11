using ClassLibrary1.Data;
using ClassLibrary1.Drone;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly ILogger<HistoryService> _logger;
        private readonly ISqlService _sqlService;

        public HistoryService(ILogger<HistoryService> logger, ISqlService sqlService)
        {
            _logger = logger;
            _sqlService = sqlService;
        }
        #region 无人机数据点相关操作
        public async Task<bool> AddDroneDataAsync(DroneDataPoint dataPoint)
        {
            _logger.LogInformation("添加无人机数据点：{DataPoint}", dataPoint);
            
            // 使用参数化查询避免 SQL 注入和算术溢出
            string command = @"
                INSERT INTO DroneStatusHistory (DroneId, Status, Timestamp, CpuUsage, BandwidthAvailable, MemoryUsage, Latitude, Longitude) 
                VALUES (@DroneId, @Status, @Timestamp, @CpuUsage, @BandwidthAvailable, @MemoryUsage, @Latitude, @Longitude)";
            
            // 安全地转换 GPS 坐标，避免溢出
            var latitude = Math.Round((double)dataPoint.Latitude, 7);
            var longitude = Math.Round((double)dataPoint.Longitude, 7);
            
            // 确保数值在合理范围内
            var cpuUsage = Math.Max(0, Math.Min(100, dataPoint.cpu_used_rate));
            var bandwidth = Math.Max(0, Math.Min(1000, dataPoint.left_bandwidth));
            var memory = Math.Max(0, Math.Min(100, dataPoint.memory));
            
            var parameters = new Dictionary<string, object>
            {
                ["@DroneId"] = dataPoint.Id, // 直接传递Guid，不转换为字符串
                ["@Status"] = (int)dataPoint.Status,
                ["@Timestamp"] = dataPoint.Timestamp,
                ["@CpuUsage"] = cpuUsage,
                ["@BandwidthAvailable"] = bandwidth,
                ["@MemoryUsage"] = memory,
                ["@Latitude"] = latitude,
                ["@Longitude"] = longitude
            };
            
            _logger.LogInformation("插入参数：DroneId={DroneId}, Status={Status}, CpuUsage={CpuUsage}, Bandwidth={Bandwidth}, Memory={Memory}, Lat={Latitude}, Lng={Longitude}", 
                dataPoint.Id, (int)dataPoint.Status, cpuUsage, bandwidth, memory, latitude, longitude);
            
            int affectedRows = await _sqlService.ExecuteCommandWithParametersAsync(command, parameters);
            _logger.LogInformation("插入结果：影响行数={AffectedRows}", affectedRows);
            return affectedRows > 0;
        }
        public async Task<bool> DeleteDroneDataAsync(Guid dataPointId)
        {
            _logger.LogInformation("删除无人机数据点：DataPointId={DataPointId}", dataPointId);
            string command = $"DELETE FROM DroneStatusHistory WHERE Id = {dataPointId}";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<bool> UpdateDroneDataAsync(DroneDataPoint dataPoint)
        {
            _logger.LogInformation("更新无人机数据点：{DataPoint}", dataPoint);
            string command = $@"
                UPDATE DroneStatusHistory 
                SET Status = {(int)dataPoint.Status}, Timestamp = '{dataPoint.Timestamp:yyyy-MM-dd HH:mm:ss}', 
                    CpuUsage = {dataPoint.cpu_used_rate}, BandwidthAvailable = {dataPoint.left_bandwidth}, 
                    MemoryUsage = {dataPoint.memory}, Latitude = {dataPoint.Latitude}, Longitude = {dataPoint.Longitude} 
                WHERE DroneId = '{dataPoint.Id}' AND Timestamp = '{dataPoint.Timestamp:yyyy-MM-dd HH:mm:ss}'";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<IEnumerable<dynamic>> GetHistoryDataAsync(string droneId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取无人机数据点：DroneId={DroneId}, StartTime={StartTime}, EndTime={EndTime}", droneId, startTime, endTime);
            string query = $@"
                SELECT * FROM DroneStatusHistory 
                WHERE DroneId = '{droneId}' AND Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'
                ORDER BY Timestamp DESC";
            return await _sqlService.ExecuteQueryAsync(query);
        }
        public async Task<DroneDataPoint?> GetDroneDataByIdAsync(Guid dataPointId)
        {
            _logger.LogInformation("获取无人机数据点：DataPointId={DataPointId}", dataPointId);
            string query = $"SELECT * FROM DroneStatusHistory WHERE Id = {dataPointId}";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var row = results.FirstOrDefault();

            if (row == null) return null;

            return new DroneDataPoint
            {
                PointId = Guid.Parse(row["Id"].ToString()),
                Id = Guid.Parse(row["DroneId"].ToString()),
                Status = (DroneStatus)Convert.ToInt32(row["Status"]),
                Timestamp = Convert.ToDateTime(row["Timestamp"]),
                Latitude = Convert.ToDecimal(row["Latitude"]),
                Longitude = Convert.ToDecimal(row["Longitude"]),
                cpu_used_rate = Convert.ToDouble(row["CpuUsage"]),
                left_bandwidth = Convert.ToDouble(row["BandwidthAvailable"]),
                memory = Convert.ToDouble(row["MemoryUsage"])
            };
        }
        public async Task<int> GetDroneDataCountAsync(Guid droneId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取无人机数据点数量：DroneId={DroneId}, StartTime={StartTime}, EndTime={EndTime}", droneId, startTime, endTime);
            string query = $"SELECT COUNT(*) as Count FROM DroneStatusHistory WHERE DroneId = '{droneId}' AND Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var row = results.FirstOrDefault();
            return row != null ? Convert.ToInt32(row["Count"]) : 0;
        }
        public async Task<int> GetTaskDataCountAsync(Guid taskId, Guid droneId)
        {
            _logger.LogInformation("获取任务下无人机数据点数量：TaskId={TaskId}, DroneId={DroneId}", taskId, droneId);
            // 通过主任务的时间范围来查询无人机数据点
            string query = $@"
                SELECT COUNT(*) as Count FROM DroneStatusHistory dsh
                INNER JOIN MainTasks mt ON dsh.Timestamp BETWEEN mt.StartTime AND mt.CompletedTime
                WHERE mt.Id = '{taskId}' AND dsh.DroneId = '{droneId}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var row = results.FirstOrDefault();
            return row != null ? Convert.ToInt32(row["Count"]) : 0;
        }
        public async Task<int> GetAllDronesDataCountAsync(DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取所有无人机数据点数量：StartTime={StartTime}, EndTime={EndTime}", startTime, endTime);
            string query = $"SELECT COUNT(*) as Count FROM DroneStatusHistory WHERE Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var row = results.FirstOrDefault();
            return row != null ? Convert.ToInt32(row["Count"]) : 0;
        }
        public async Task<List<DroneDataPoint>> GetDroneDataAsync(Guid droneId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取无人机数据点：DroneId={DroneId}, StartTime={StartTime}, EndTime={EndTime}", droneId, startTime, endTime);
            string query = $@"
                SELECT * FROM DroneStatusHistory 
                WHERE DroneId = '{droneId}' AND Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'
                ORDER BY Timestamp DESC";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var dataPoints = new List<DroneDataPoint>();

            foreach (var row in results)
            {
                dataPoints.Add(new DroneDataPoint
                {
                    PointId = Guid.Parse(row["Id"].ToString()), // 修复：使用Guid.Parse
                    Id = Guid.Parse(row["DroneId"].ToString()),
                    Status = (DroneStatus)Convert.ToInt32(row["Status"]),
                    Timestamp = Convert.ToDateTime(row["Timestamp"]),
                    Latitude = Convert.ToDecimal(row["Latitude"]),
                    Longitude = Convert.ToDecimal(row["Longitude"]),
                    cpu_used_rate = Convert.ToDouble(row["CpuUsage"]),
                    left_bandwidth = Convert.ToDouble(row["BandwidthAvailable"]),
                    memory = Convert.ToDouble(row["MemoryUsage"])
                });
            }
            return dataPoints;
        }
        public async Task<List<DroneDataPoint>> GetTaskDataAsync(Guid taskId, Guid droneId)
        {
            _logger.LogInformation("获取无人机数据点：DroneId={DroneId},taskId={taskId}", droneId, taskId);
            string query_time = $"SELECT StartTime, CompletedTime FROM MainTasks WHERE Id='{taskId}'";
            var times = await _sqlService.ExecuteQueryAsync(query_time);
            var row = times.FirstOrDefault();

            if (row == null) return new List<DroneDataPoint>();

            var startTime = row["StartTime"] != DBNull.Value ? Convert.ToDateTime(row["StartTime"]) : DateTime.MinValue;
            var endTime = row["CompletedTime"] != DBNull.Value ? Convert.ToDateTime(row["CompletedTime"]) : DateTime.MaxValue;

            string query = $@"
                SELECT * FROM DroneStatusHistory 
                WHERE DroneId = '{droneId}' AND Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'
                ORDER BY Timestamp DESC";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var dataPoints = new List<DroneDataPoint>();

            foreach (var resultRow in results)
            {
                dataPoints.Add(new DroneDataPoint
                {
                    PointId = Guid.Parse(resultRow["Id"].ToString()), // 修复：使用Guid.Parse
                    Id = Guid.Parse(resultRow["DroneId"].ToString()),
                    Status = (DroneStatus)Convert.ToInt32(resultRow["Status"]),
                    Timestamp = Convert.ToDateTime(resultRow["Timestamp"]),
                    Latitude = Convert.ToDecimal(resultRow["Latitude"]),
                    Longitude = Convert.ToDecimal(resultRow["Longitude"]),
                    cpu_used_rate = Convert.ToDouble(resultRow["CpuUsage"]),
                    left_bandwidth = Convert.ToDouble(resultRow["BandwidthAvailable"]),
                    memory = Convert.ToDouble(resultRow["MemoryUsage"])
                });
            }
            return dataPoints;
        }
        public async Task<List<DroneDataPoint>> GetAllDronesDataAsync(DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取所有无人机数据点：StartTime={StartTime}, EndTime={EndTime}", startTime, endTime);
            string query = $@"
                SELECT * FROM DroneStatusHistory 
                WHERE Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'
                ORDER BY Timestamp DESC";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var dataPoints = new List<DroneDataPoint>();

            foreach (var row in results)
            {
                dataPoints.Add(new DroneDataPoint
                {
                    PointId = Guid.Parse(row["Id"].ToString()), // 修复：使用Guid.Parse
                    Id = Guid.Parse(row["DroneId"].ToString()),
                    Status = (DroneStatus)Convert.ToInt32(row["Status"]),
                    Timestamp = Convert.ToDateTime(row["Timestamp"]),
                    Latitude = Convert.ToDecimal(row["Latitude"]),
                    Longitude = Convert.ToDecimal(row["Longitude"]),
                    cpu_used_rate = Convert.ToDouble(row["CpuUsage"]),
                    left_bandwidth = Convert.ToDouble(row["BandwidthAvailable"]),
                    memory = Convert.ToDouble(row["MemoryUsage"])
                });
            }
            return dataPoints;
        }
        public async Task<List<DroneDataPoint>> GetAllDronesDataAsync(DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            _logger.LogInformation("获取所有无人机数据点：StartTime={StartTime}, EndTime={EndTime}, PageIndex={PageIndex}, PageSize={PageSize}", startTime, endTime, pageIndex, pageSize);
            int offset = (pageIndex - 1) * pageSize;
            string query = $@"
                SELECT * FROM DroneStatusHistory 
                WHERE Timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'
                ORDER BY Timestamp DESC
                OFFSET {offset} ROWS
                FETCH NEXT {pageSize} ROWS ONLY";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var dataPoints = new List<DroneDataPoint>();

            foreach (var row in results)
            {
                dataPoints.Add(new DroneDataPoint
                {
                    PointId = Guid.Parse(row["Id"].ToString()), // 修复：使用Guid.Parse
                    Id = Guid.Parse(row["DroneId"].ToString()),
                    Status = (DroneStatus)Convert.ToInt32(row["Status"]),
                    Timestamp = Convert.ToDateTime(row["Timestamp"]),
                    Latitude = Convert.ToDecimal(row["Latitude"]),
                    Longitude = Convert.ToDecimal(row["Longitude"]),
                    cpu_used_rate = Convert.ToDouble(row["CpuUsage"]),
                    left_bandwidth = Convert.ToDouble(row["BandwidthAvailable"]),
                    memory = Convert.ToDouble(row["MemoryUsage"])
                });
            }
            return dataPoints;
        }

        public async Task<DroneDataPoint?> GetLatestDroneDataPointAsync(Guid droneId)
        {
            _logger.LogInformation("获取最新无人机数据点：DroneId={DroneId}", droneId);
            
            string query = $@"
                SELECT TOP 1 * FROM DroneStatusHistory 
                WHERE DroneId = '{droneId}'
                ORDER BY Timestamp DESC";
                
            var results = await _sqlService.ExecuteQueryAsync(query);
            var row = results.FirstOrDefault();

            if (row == null) return null;

            return new DroneDataPoint
            {
                PointId = Guid.Parse(row["Id"].ToString()), // 修复：使用Guid.Parse
                Id = Guid.Parse(row["DroneId"].ToString()),
                Status = (DroneStatus)Convert.ToInt32(row["Status"]),
                Timestamp = Convert.ToDateTime(row["Timestamp"]),
                Latitude = Convert.ToDecimal(row["Latitude"]),
                Longitude = Convert.ToDecimal(row["Longitude"]),
                cpu_used_rate = Convert.ToDouble(row["CpuUsage"]),
                left_bandwidth = Convert.ToDouble(row["BandwidthAvailable"]),
                memory = Convert.ToDouble(row["MemoryUsage"])
            };
        }

        public async Task<List<DroneDataPoint>> GetAllDronesLatestDataPointsAsync()
        {
            _logger.LogInformation("获取所有无人机最新数据点");
            
            // 使用窗口函数获取每个无人机的最新数据点
            string query = @"
                WITH LatestDataPoints AS (
                    SELECT *,
                           ROW_NUMBER() OVER (PARTITION BY DroneId ORDER BY Timestamp DESC) as rn
                    FROM DroneStatusHistory
                )
                SELECT * FROM LatestDataPoints 
                WHERE rn = 1
                ORDER BY Timestamp DESC";
                
            var results = await _sqlService.ExecuteQueryAsync(query);
            var dataPoints = new List<DroneDataPoint>();

            foreach (var row in results)
            {
                dataPoints.Add(new DroneDataPoint
                {
                    PointId = Guid.Parse(row["Id"].ToString()), // 修复：使用Guid.Parse而不是Convert.ToInt64
                    Id = Guid.Parse(row["DroneId"].ToString()),
                    Status = (DroneStatus)Convert.ToInt32(row["Status"]),
                    Timestamp = Convert.ToDateTime(row["Timestamp"]),
                    Latitude = Convert.ToDecimal(row["Latitude"]),
                    Longitude = Convert.ToDecimal(row["Longitude"]),
                    cpu_used_rate = Convert.ToDouble(row["CpuUsage"]),
                    left_bandwidth = Convert.ToDouble(row["BandwidthAvailable"]),
                    memory = Convert.ToDouble(row["MemoryUsage"])
                });
            }
            
            _logger.LogInformation("获取到 {Count} 个无人机最新数据点", dataPoints.Count);
            return dataPoints;
        }

        #endregion
        #region 子任务历史数据相关操作
        public async Task<bool> AddSubTaskDataAsync(SubTaskDataPoint dataPoint)
        {
            _logger.LogInformation("添加子任务数据点：{DataPoint}", dataPoint);
            string command = $@"
                INSERT INTO SubTaskHistory (SubTaskId, OldStatus, NewStatus, ChangeTime, ChangedBy, DroneId, Reason, AdditionalInfo) 
                VALUES ('{dataPoint.SubTaskId}', {(int)dataPoint.OldStatus}, {(int)dataPoint.NewStatus}, '{dataPoint.Timestamp:yyyy-MM-dd HH:mm:ss}', SUSER_SNAME(), {(dataPoint.DroneId.HasValue ? $"'{dataPoint.DroneId}'" : "NULL")}, '{dataPoint.Reason}', '{dataPoint.AdditionalInfo ?? ""}')";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<bool> DeleteSubTaskDataAsync(Guid dataPointId)
        {
            _logger.LogInformation("删除子任务数据点：{dataPointId}", dataPointId);
            string command = $"DELETE FROM SubTaskHistory WHERE Id = {dataPointId}";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<bool> UpdateSubTaskDataAsync(SubTaskDataPoint dataPoint)
        {
            _logger.LogInformation("更新子任务数据点：{DataPoint}", dataPoint);
            string command = $@"
                UPDATE SubTaskHistory 
                SET OldStatus = {(int)dataPoint.OldStatus}, NewStatus = {(int)dataPoint.NewStatus}, 
                    ChangeTime = '{dataPoint.Timestamp:yyyy-MM-dd HH:mm:ss}', 
                    DroneId = {(dataPoint.DroneId.HasValue ? $"'{dataPoint.DroneId}'" : "NULL")}, 
                    Reason = '{dataPoint.Reason}', AdditionalInfo = '{dataPoint.AdditionalInfo ?? ""}'
                WHERE SubTaskId = '{dataPoint.SubTaskId}' AND ChangeTime = '{dataPoint.Timestamp:yyyy-MM-dd HH:mm:ss}'";
            int affectedRows = await _sqlService.ExecuteCommandAsync(command);
            return affectedRows > 0;
        }
        public async Task<SubTaskDataPoint?> GetSubTaskDataAsync(Guid dataPointId)
        {
            _logger.LogInformation("获取子任务数据点：DataPointId={DataPointId}", dataPointId);
            string query = $"SELECT * FROM SubTaskHistory WHERE Id = {dataPointId}";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var row = results.FirstOrDefault();

            if (row == null) return null;

            return new SubTaskDataPoint
            {
                PointId = Convert.ToInt64(row["Id"]),
                SubTaskId = Guid.Parse(row["SubTaskId"].ToString()),
                Timestamp = Convert.ToDateTime(row["ChangeTime"]),
                OldStatus = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["OldStatus"]),
                NewStatus = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["NewStatus"]),
                DroneId = row["DroneId"] != DBNull.Value ? Guid.Parse(row["DroneId"].ToString()) : null,
                Reason = row["Reason"].ToString(),
                AdditionalInfo = row["AdditionalInfo"]?.ToString()
            };
        }
        public async Task<int> GetSubTaskDataCountAsync(Guid subTaskId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取子任务数据点数量：SubTaskId={SubTaskId}, StartTime={StartTime}, EndTime={EndTime}", subTaskId, startTime, endTime);
            string query = $"SELECT COUNT(*) as Count FROM SubTaskHistory WHERE SubTaskId = '{subTaskId}' AND ChangeTime BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var row = results.FirstOrDefault();
            return row != null ? Convert.ToInt32(row["Count"]) : 0;
        }

        public async Task<List<SubTaskDataPoint>> GetSubTaskHistoryAsync(Guid subTaskId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("获取子任务历史记录：SubTaskId={SubTaskId}, StartTime={StartTime}, EndTime={EndTime}", subTaskId, startTime, endTime);
            string query = $@"
                SELECT * FROM SubTaskHistory 
                WHERE SubTaskId = '{subTaskId}' AND ChangeTime BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' AND '{endTime:yyyy-MM-dd HH:mm:ss}'
                ORDER BY ChangeTime DESC";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var historyPoints = new List<SubTaskDataPoint>();

            foreach (var row in results)
            {
                historyPoints.Add(new SubTaskDataPoint
                {
                    PointId = Convert.ToInt64(row["Id"]),
                    SubTaskId = Guid.Parse(row["SubTaskId"].ToString()),
                    Timestamp = Convert.ToDateTime(row["ChangeTime"]),
                    OldStatus = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["OldStatus"]),
                    NewStatus = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["NewStatus"]),
                    DroneId = row["DroneId"] != DBNull.Value ? Guid.Parse(row["DroneId"].ToString()) : null,
                    Reason = row["Reason"].ToString(),
                    AdditionalInfo = row["AdditionalInfo"]?.ToString()
                });
            }
            return historyPoints;
        }

        public async Task<List<SubTaskDataPoint>> GetSubTaskHistoryAsync(Guid subTaskId)
        {
            _logger.LogInformation("获取子任务所有历史记录：SubTaskId={SubTaskId}", subTaskId);
            string query = $@"
                SELECT * FROM SubTaskHistory 
                WHERE SubTaskId = '{subTaskId}'
                ORDER BY ChangeTime DESC";
            var results = await _sqlService.ExecuteQueryAsync(query);
            var historyPoints = new List<SubTaskDataPoint>();

            foreach (var row in results)
            {
                historyPoints.Add(new SubTaskDataPoint
                {
                    PointId = Convert.ToInt64(row["Id"]),
                    SubTaskId = Guid.Parse(row["SubTaskId"].ToString()),
                    Timestamp = Convert.ToDateTime(row["ChangeTime"]),
                    OldStatus = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["OldStatus"]),
                    NewStatus = (System.Threading.Tasks.TaskStatus)Convert.ToInt32(row["NewStatus"]),
                    DroneId = row["DroneId"] != DBNull.Value ? Guid.Parse(row["DroneId"].ToString()) : null,
                    Reason = row["Reason"].ToString(),
                    AdditionalInfo = row["AdditionalInfo"]?.ToString()
                });
            }
            return historyPoints;
        }
        #endregion
    }
}