using ClassLibrary1.Data;
using ClassLibrary1.Drone;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IHistoryService _historyService;
        private readonly IDataService _dataService;
        private readonly ILogger<TestController> _logger;

        public TestController(IHistoryService historyService, IDataService dataService, ILogger<TestController> logger)
        {
            _historyService = historyService;
            _dataService = dataService;
            _logger = logger;
        }

        /// <summary>
        /// 添加测试无人机和数据点
        /// </summary>
        [HttpPost("add-test-data")]
        public async Task<ActionResult<string>> AddTestData()
        {
            try
            {
                // 1. 添加测试无人机
                var droneId = Guid.NewGuid();
                var drone = new Drone
                {
                    Id = droneId,
                    Name = "TestDrone-001",
                    ModelStatus = ModelStatus.True,
                    ModelType = "Quadcopter",
                    Status = DroneStatus.Idle
                };

                var droneResult = await _dataService.AddDroneAsync(drone);
                if (!droneResult)
                {
                    return BadRequest("添加无人机失败");
                }

                // 2. 添加多个测试数据点
                var testDataPoints = new List<DroneDataPoint>
                {
                    new DroneDataPoint
                    {
                        Id = droneId,
                        Status = DroneStatus.Idle,
                        Timestamp = DateTime.UtcNow.AddHours(-2),
                        Latitude = 39.9042m,
                        Longitude = 116.4074m,
                        cpu_used_rate = 25.5,
                        left_bandwidth = 85.0,
                        memory = 45.2
                    },
                    new DroneDataPoint
                    {
                        Id = droneId,
                        Status = DroneStatus.InMission,
                        Timestamp = DateTime.UtcNow.AddHours(-1),
                        Latitude = 39.9142m,
                        Longitude = 116.4174m,
                        cpu_used_rate = 67.8,
                        left_bandwidth = 45.5,
                        memory = 78.9
                    },
                    new DroneDataPoint
                    {
                        Id = droneId,
                        Status = DroneStatus.InMission,
                        Timestamp = DateTime.UtcNow.AddMinutes(-30),
                        Latitude = 39.9242m,
                        Longitude = 116.4274m,
                        cpu_used_rate = 89.2,
                        left_bandwidth = 32.8,
                        memory = 82.1
                    },
                    new DroneDataPoint
                    {
                        Id = droneId,
                        Status = DroneStatus.Returning,
                        Timestamp = DateTime.UtcNow.AddMinutes(-15),
                        Latitude = 39.9342m,
                        Longitude = 116.4374m,
                        cpu_used_rate = 45.6,
                        left_bandwidth = 67.2,
                        memory = 38.7
                    },
                    new DroneDataPoint
                    {
                        Id = droneId,
                        Status = DroneStatus.Idle,
                        Timestamp = DateTime.UtcNow,
                        Latitude = 39.9442m,
                        Longitude = 116.4474m,
                        cpu_used_rate = 23.4,
                        left_bandwidth = 92.5,
                        memory = 42.1
                    }
                };

                var successCount = 0;
                foreach (var dataPoint in testDataPoints)
                {
                    var result = await _historyService.AddDroneDataAsync(dataPoint);
                    if (result)
                    {
                        successCount++;
                        _logger.LogInformation("成功添加数据点: {Timestamp} - {Status}", dataPoint.Timestamp, dataPoint.Status);
                    }
                    else
                    {
                        _logger.LogError("添加数据点失败: {Timestamp} - {Status}", dataPoint.Timestamp, dataPoint.Status);
                    }
                }

                return Ok(new
                {
                    message = "测试数据添加完成",
                    droneId = droneId.ToString(),
                    totalDataPoints = testDataPoints.Count,
                    successCount = successCount,
                    testUrls = new
                    {
                        latestDatapoints = $"/api/drones/latest-datapoints",
                        singleDroneLatest = $"/api/drones/{droneId}/latest-datapoint",
                        droneDataPoints = $"/api/drones/{droneId}/datapoints?startTime={DateTime.UtcNow.AddDays(-1):yyyy-MM-ddTHH:mm:ss}Z&endTime={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}Z"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加测试数据失败");
                return StatusCode(500, new { error = "添加测试数据失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 检查数据库连接
        /// </summary>
        [HttpGet("check-db")]
        public async Task<ActionResult<object>> CheckDatabase()
        {
            try
            {
                // 检查Drones表
                var drones = await _dataService.GetDronesAsync();
                
                // 检查DroneStatusHistory表（通过获取最新数据点）
                var latestDataPoints = await _historyService.GetAllDronesLatestDataPointsAsync();

                return Ok(new
                {
                    dronesCount = drones.Count,
                    dataPointsCount = latestDataPoints.Count,
                    drones = drones.Select(d => new { d.Id, d.Name, d.Status }),
                    latestDataPoints = latestDataPoints.Select(dp => new { dp.Id, dp.Status, dp.Timestamp })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查数据库失败");
                return StatusCode(500, new { error = "检查数据库失败", message = ex.Message });
            }
        }
    }
} 