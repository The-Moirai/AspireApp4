using WebApplication_Drone.Services;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services
{
    public class StartBackground : BackgroundService
    {
        private readonly SocketService _socketService;
        private readonly MissionSocketService _missionsocketService;
        private readonly ITaskService _taskService;
        private readonly ILogger<StartBackground> _logger;
        
        public StartBackground(
            SocketService socketService, 
            MissionSocketService missionsocketService, 
            ITaskService taskService, 
            ILogger<StartBackground> logger)
        {
            _socketService = socketService;
            _missionsocketService = missionsocketService;
            _taskService = taskService;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("正在启动 StartBackground...");

                // 启动MissionSocketService (图片接收服务)
                _logger.LogInformation("启动 MissionSocketService 在端口 5009...");
                await _missionsocketService.StartAsync(5009);

                // 启动SocketService (连接到Linux端)
                _logger.LogInformation("连接到 Linux 端 192.168.31.35:5007...");
                await _socketService.ConnectAsync("192.168.31.35", 5007);

                _logger.LogInformation("所有服务启动完成，SocketBackgroundService 正在运行");

                // 保持服务运行直到取消
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SocketBackgroundService 正常关闭");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SocketBackgroundService 启动失败: {Message}", ex.Message);
            }
        }
    }
}
