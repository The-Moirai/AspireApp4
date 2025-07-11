using ClassLibrary1.Data;
using ClassLibrary1.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// 任务管理API控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        #region 主任务管理
        /// <summary>
        /// 获取所有主任务
        /// </summary>
        /// <returns>主任务列表</returns>
        [HttpGet]
        public async Task<ActionResult<List<MainTask>>> GetMainTasks()
        {
            try
            {
                var tasks = await _taskService.GetMainTasksAsync();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取主任务列表失败");
                return StatusCode(500, new { error = "获取主任务列表失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 根据ID获取主任务
        /// </summary>
        /// <param name="id">主任务ID</param>
        /// <returns>主任务信息</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MainTask>> GetMainTask(Guid id)
        {
            try
            {
                var task = await _taskService.GetMainTaskAsync(id);
                if (task == null)
                {
                    return NotFound(new { error = "主任务不存在", id });
                }
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取主任务失败: {TaskId}", id);
                return StatusCode(500, new { error = "获取主任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 添加主任务
        /// </summary>
        /// <param name="request">添加任务请求</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<ActionResult<bool>> AddMainTask([FromBody] AddMainTaskRequest request)
        {
            try
            {
                if (request?.Task == null)
                {
                    return BadRequest(new { error = "任务信息不能为空" });
                }

                var result = await _taskService.AddMainTaskAsync(request.Task, request.CreatedBy);
                if (result)
                {
                    return CreatedAtAction(nameof(GetMainTask), new { id = request.Task.Id }, new { success = true, id = request.Task.Id });
                }
                return BadRequest(new { error = "添加主任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加主任务失败: {TaskName}", request?.Task?.Name);
                return StatusCode(500, new { error = "添加主任务失败", message = ex.Message });
            }
        }
        /// <summary>
        /// 创建任务（支持文件上传）
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<MainTask>> Create([FromForm] string? Description, [FromForm] string? Id, [FromForm] string? CreationTime, [FromForm] string? Notes, [FromForm] IFormFile? VideoFile)
        {
            try
            {
                // 解析任务ID
                Guid taskId;
                if (!string.IsNullOrEmpty(Id) && Guid.TryParse(Id, out var parsedId))
                {
                    taskId = parsedId;
                }
                else
                {
                    taskId = Guid.NewGuid();
                }

                // 解析创建时间
                DateTime creationTime;
                if (!string.IsNullOrEmpty(CreationTime) && DateTime.TryParse(CreationTime, out var parsedTime))
                {
                    creationTime = parsedTime;
                }
                else
                {
                    creationTime = DateTime.Now;
                }

                // 创建任务对象
                var task = new MainTask
                {
                    Id = taskId,
                    Description = Description ?? "未命名任务",
                    Status = System.Threading.Tasks.TaskStatus.Created,
                    CreationTime = creationTime
                };

                // 如果有文件上传，保存文件信息
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    _logger.LogInformation("收到文件上传: {FileName}, 大小: {FileSize} 字节",
                        VideoFile.FileName, VideoFile.Length);

                    // 保存文件到TaskVideos目录
                    var fileName = $"{task.Id}_{VideoFile.FileName}";
                    var filePath = Path.Combine("TaskVideos", fileName);

                    // 确保目录存在
                    Directory.CreateDirectory("TaskVideos");

                    // 保存文件
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await VideoFile.CopyToAsync(stream);
                    }

                    _logger.LogInformation("文件保存成功: {FilePath}", filePath);

                }

                // 创建任务
                var success = await _taskService.AddMainTaskAsync(task, "System");
                if (success)
                {
                    _logger.LogInformation("任务创建成功: {TaskId}, {Description}", task.Id, task.Description);
                    return CreatedAtAction(nameof(GetMainTask), new { id = task.Id }, task);
                }

                return BadRequest(new { error = "创建任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建任务失败");
                return StatusCode(500, new { error = "创建任务失败", message = ex.Message });
            }
        }
        /// <summary>
        /// 更新主任务
        /// </summary>
        /// <param name="id">主任务ID</param>
        /// <param name="task">主任务信息</param>
        /// <returns>操作结果</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<bool>> UpdateMainTask(Guid id, [FromBody] MainTask task)
        {
            try
            {
                if (task == null)
                {
                    return BadRequest(new { error = "任务信息不能为空" });
                }

                if (id != task.Id)
                {
                    return BadRequest(new { error = "ID不匹配" });
                }

                var result = await _taskService.UpdateMainTaskAsync(task);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "更新主任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新主任务失败: {TaskId}", id);
                return StatusCode(500, new { error = "更新主任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 删除主任务
        /// </summary>
        /// <param name="id">主任务ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<bool>> DeleteMainTask(Guid id)
        {
            try
            {
                var result = await _taskService.DeleteMainTaskAsync(id);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return NotFound(new { error = "主任务不存在或删除失败", id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除主任务失败: {TaskId}", id);
                return StatusCode(500, new { error = "删除主任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取主任务数量
        /// </summary>
        /// <returns>主任务数量</returns>
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetMainTaskCount()
        {
            try
            {
                var count = await _taskService.GetMainTaskCountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取主任务数量失败");
                return StatusCode(500, new { error = "获取主任务数量失败", message = ex.Message });
            }
        }
        #endregion

        #region 子任务管理
        /// <summary>
        /// 获取主任务的所有子任务
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <returns>子任务列表</returns>
        [HttpGet("{mainTaskId:guid}/subtasks")]
        public async Task<ActionResult<List<SubTask>>> GetSubTasks(Guid mainTaskId)
        {
            try
            {
                var subTasks = await _taskService.GetSubTasksAsync(mainTaskId);
                return Ok(subTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取子任务列表失败: {MainTaskId}", mainTaskId);
                return StatusCode(500, new { error = "获取子任务列表失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 根据ID获取子任务
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <param name="subTaskId">子任务ID</param>
        /// <returns>子任务信息</returns>
        [HttpGet("{mainTaskId:guid}/subtasks/{subTaskId:guid}")]
        public async Task<ActionResult<SubTask>> GetSubTask(Guid mainTaskId, Guid subTaskId)
        {
            try
            {
                var subTask = await _taskService.GetSubTaskAsync(mainTaskId, subTaskId);
                if (subTask == null)
                {
                    return NotFound(new { error = "子任务不存在", mainTaskId, subTaskId });
                }
                return Ok(subTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取子任务失败: {MainTaskId}/{SubTaskId}", mainTaskId, subTaskId);
                return StatusCode(500, new { error = "获取子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 添加子任务
        /// </summary>
        /// <param name="subTask">子任务信息</param>
        /// <returns>操作结果</returns>
        [HttpPost("subtasks")]
        public async Task<ActionResult<bool>> AddSubTask([FromBody] SubTask subTask)
        {
            try
            {
                if (subTask == null)
                {
                    return BadRequest(new { error = "子任务信息不能为空" });
                }

                var result = await _taskService.AddSubTaskAsync(subTask);
                if (result)
                {
                    return CreatedAtAction(nameof(GetSubTask), new { mainTaskId = subTask.ParentTask, subTaskId = subTask.Id }, new { success = true, id = subTask.Id });
                }
                return BadRequest(new { error = "添加子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加子任务失败: {SubTaskDescription}", subTask?.Description);
                return StatusCode(500, new { error = "添加子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 更新子任务
        /// </summary>
        /// <param name="subTask">子任务信息</param>
        /// <returns>操作结果</returns>
        [HttpPut("subtasks")]
        public async Task<ActionResult<bool>> UpdateSubTask([FromBody] SubTask subTask)
        {
            try
            {
                if (subTask == null)
                {
                    return BadRequest(new { error = "子任务信息不能为空" });
                }

                var result = await _taskService.UpdateSubTaskAsync(subTask);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "更新子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新子任务失败: {SubTaskId}", subTask?.Id);
                return StatusCode(500, new { error = "更新子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 删除子任务
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <param name="subTaskId">子任务ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("{mainTaskId:guid}/subtasks/{subTaskId:guid}")]
        public async Task<ActionResult<bool>> DeleteSubTask(Guid mainTaskId, Guid subTaskId)
        {
            try
            {
                var result = await _taskService.DeleteSubTaskAsync(mainTaskId, subTaskId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return NotFound(new { error = "子任务不存在或删除失败", mainTaskId, subTaskId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除子任务失败: {MainTaskId}/{SubTaskId}", mainTaskId, subTaskId);
                return StatusCode(500, new { error = "删除子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取子任务数量
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <returns>子任务数量</returns>
        [HttpGet("{mainTaskId:guid}/subtasks/count")]
        public async Task<ActionResult<int>> GetSubTaskCount(Guid mainTaskId)
        {
            try
            {
                var count = await _taskService.GetSubTaskCountAsync(mainTaskId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取子任务数量失败: {MainTaskId}", mainTaskId);
                return StatusCode(500, new { error = "获取子任务数量失败", message = ex.Message });
            }
        }
        #endregion

        #region 任务状态管理
        /// <summary>
        /// 启动主任务
        /// </summary>
        /// <param name="id">主任务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("{id:guid}/start")]
        public async Task<ActionResult<bool>> StartMainTask(Guid id)
        {
            try
            {
                var result = await _taskService.StartMainTaskAsync(id);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "启动主任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动主任务失败: {TaskId}", id);
                return StatusCode(500, new { error = "启动主任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 完成主任务
        /// </summary>
        /// <param name="id">主任务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("{id:guid}/complete")]
        public async Task<ActionResult<bool>> CompleteMainTask(Guid id)
        {
            try
            {
                var result = await _taskService.CompleteMainTaskAsync(id);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "完成主任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "完成主任务失败: {TaskId}", id);
                return StatusCode(500, new { error = "完成主任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 取消主任务
        /// </summary>
        /// <param name="id">主任务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("{id:guid}/cancel")]
        public async Task<ActionResult<bool>> CancelMainTask(Guid id)
        {
            try
            {
                var result = await _taskService.CancelMainTaskAsync(id);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "取消主任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消主任务失败: {TaskId}", id);
                return StatusCode(500, new { error = "取消主任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 启动子任务
        /// </summary>
        /// <param name="subTaskId">子任务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("subtasks/{subTaskId:guid}/start")]
        public async Task<ActionResult<bool>> StartSubTask(Guid subTaskId)
        {
            try
            {
                var result = await _taskService.StartSubTaskAsync(subTaskId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "启动子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动子任务失败: {SubTaskId}", subTaskId);
                return StatusCode(500, new { error = "启动子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 完成子任务
        /// </summary>
        /// <param name="subTaskId">子任务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("subtasks/{subTaskId:guid}/complete")]
        public async Task<ActionResult<bool>> CompleteSubTask(Guid subTaskId)
        {
            try
            {
                var result = await _taskService.CompleteSubTaskAsync(subTaskId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "完成子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "完成子任务失败: {SubTaskId}", subTaskId);
                return StatusCode(500, new { error = "完成子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 取消子任务
        /// </summary>
        /// <param name="subTaskId">子任务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("subtasks/{subTaskId:guid}/cancel")]
        public async Task<ActionResult<bool>> CancelSubTask(Guid subTaskId)
        {
            try
            {
                var result = await _taskService.CancelSubTaskAsync(subTaskId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "取消子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消子任务失败: {SubTaskId}", subTaskId);
                return StatusCode(500, new { error = "取消子任务失败", message = ex.Message });
            }
        }
        #endregion

        #region 任务分配管理
        /// <summary>
        /// 分配子任务到无人机
        /// </summary>
        /// <param name="subTaskId">子任务ID</param>
        /// <param name="droneId">无人机ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("subtasks/{subTaskId:guid}/assign/{droneId:guid}")]
        public async Task<ActionResult<bool>> AssignSubTask(Guid subTaskId, Guid droneId)
        {
            try
            {
                var result = await _taskService.AssignSubTaskToDroneAsync(subTaskId, droneId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "分配子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分配子任务失败: {SubTaskId} -> {DroneId}", subTaskId, droneId);
                return StatusCode(500, new { error = "分配子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 取消分配子任务
        /// </summary>
        /// <param name="subTaskId">子任务ID</param>
        /// <param name="droneId">无人机ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("subtasks/{subTaskId:guid}/assign/{droneId:guid}")]
        public async Task<ActionResult<bool>> UnassignSubTask(Guid subTaskId, Guid droneId)
        {
            try
            {
                var result = await _taskService.UnassignSubTaskFromDroneAsync(subTaskId, droneId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "取消分配子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消分配子任务失败: {SubTaskId} <- {DroneId}", subTaskId, droneId);
                return StatusCode(500, new { error = "取消分配子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 重新分配子任务
        /// </summary>
        /// <param name="subTaskId">子任务ID</param>
        /// <param name="newDroneId">新无人机ID</param>
        /// <returns>操作结果</returns>
        [HttpPut("subtasks/{subTaskId:guid}/reassign/{newDroneId:guid}")]
        public async Task<ActionResult<bool>> ReassignSubTask(Guid subTaskId, Guid newDroneId)
        {
            try
            {
                var result = await _taskService.ReassignSubTaskAsync(subTaskId, newDroneId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "重新分配子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新分配子任务失败: {SubTaskId} -> {NewDroneId}", subTaskId, newDroneId);
                return StatusCode(500, new { error = "重新分配子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取无人机分配的子任务
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <param name="droneId">无人机ID</param>
        /// <returns>子任务列表</returns>
        [HttpGet("{mainTaskId:guid}/assigned/{droneId:guid}")]
        public async Task<ActionResult<List<SubTask>>> GetAssignedSubTasks(Guid mainTaskId, Guid droneId)
        {
            try
            {
                var subTasks = await _taskService.GetAssignedSubTasksAsync(mainTaskId, droneId);
                return Ok(subTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分配的子任务失败: {MainTaskId}/{DroneId}", mainTaskId, droneId);
                return StatusCode(500, new { error = "获取分配的子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取未分配的子任务
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <returns>子任务列表</returns>
        [HttpGet("{mainTaskId:guid}/unassigned")]
        public async Task<ActionResult<List<SubTask>>> GetUnassignedSubTasks(Guid mainTaskId)
        {
            try
            {
                var subTasks = await _taskService.GetUnassignedSubTasksAsync(mainTaskId);
                return Ok(subTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取未分配的子任务失败: {MainTaskId}", mainTaskId);
                return StatusCode(500, new { error = "获取未分配的子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 根据状态获取子任务
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <param name="status">任务状态</param>
        /// <returns>子任务列表</returns>
        [HttpGet("{mainTaskId:guid}/status/{status}")]
        public async Task<ActionResult<List<SubTask>>> GetSubTasksByStatus(Guid mainTaskId, System.Threading.Tasks.TaskStatus status)
        {
            try
            {
                var subTasks = await _taskService.GetSubTasksByStatusAsync(mainTaskId, status);
                return Ok(subTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据状态获取子任务失败: {MainTaskId}/{Status}", mainTaskId, status);
                return StatusCode(500, new { error = "根据状态获取子任务失败", message = ex.Message });
            }
        }
        #endregion

        #region 任务图片管理
        /// <summary>
        /// 保存子任务图片
        /// </summary>
        /// <param name="subTaskId">子任务ID</param>
        /// <param name="request">图片信息</param>
        /// <returns>图片ID</returns>
        [HttpPost("subtasks/{subTaskId:guid}/images")]
        public async Task<ActionResult<Guid>> SaveSubTaskImage(Guid subTaskId, [FromBody] SaveImageRequest request)
        {
            try
            {
                if (request?.ImageData == null || request.ImageData.Length == 0)
                {
                    return BadRequest(new { error = "图片数据不能为空" });
                }

                var imageId = await _taskService.SaveSubTaskImageAsync(subTaskId, request.ImageData, request.FileName, request.ImageIndex, request.Description);
                return Ok(new { success = true, imageId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存子任务图片失败: {SubTaskId}", subTaskId);
                return StatusCode(500, new { error = "保存子任务图片失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取子任务图片列表
        /// </summary>
        /// <param name="subTaskId">子任务ID</param>
        /// <returns>图片列表</returns>
        [HttpGet("subtasks/{subTaskId:guid}/images")]
        public async Task<ActionResult<List<SubTaskImage>>> GetSubTaskImages(Guid subTaskId)
        {
            try
            {
                var images = await _taskService.GetSubTaskImagesAsync(subTaskId);
                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取子任务图片失败: {SubTaskId}", subTaskId);
                return StatusCode(500, new { error = "获取子任务图片失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="imageId">图片ID</param>
        /// <returns>图片信息</returns>
        [HttpGet("images/{imageId:guid}")]
        public async Task<ActionResult<SubTaskImage>> GetImage(Guid imageId)
        {
            try
            {
                var image = await _taskService.GetSubTaskImageAsync(imageId);
                if (image == null)
                {
                    return NotFound(new { error = "图片不存在", imageId });
                }
                return Ok(image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取图片失败: {ImageId}", imageId);
                return StatusCode(500, new { error = "获取图片失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 删除图片
        /// </summary>
        /// <param name="imageId">图片ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("images/{imageId:guid}")]
        public async Task<ActionResult<bool>> DeleteImage(Guid imageId)
        {
            try
            {
                var result = await _taskService.DeleteSubTaskImageAsync(imageId);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return NotFound(new { error = "图片不存在或删除失败", imageId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除图片失败: {ImageId}", imageId);
                return StatusCode(500, new { error = "删除图片失败", message = ex.Message });
            }
        }
        #endregion

        #region 任务历史记录
        /// <summary>
        /// 获取子任务历史记录
        /// </summary>
        /// <param name="subTaskId">子任务ID</param>
        /// <returns>历史记录列表</returns>
        [HttpGet("subtasks/{subTaskId:guid}/history")]
        public async Task<ActionResult<List<SubTaskDataPoint>>> GetSubTaskHistory(Guid subTaskId)
        {
            try
            {
                var history = await _taskService.GetSubTaskHistoryAsync(subTaskId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取子任务历史记录失败: {SubTaskId}", subTaskId);
                return StatusCode(500, new { error = "获取子任务历史记录失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取子任务历史记录（时间范围）
        /// </summary>
        /// <param name="subTaskId">子任务ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>历史记录列表</returns>
        [HttpGet("subtasks/{subTaskId:guid}/history/range")]
        public async Task<ActionResult<List<SubTaskDataPoint>>> GetSubTaskHistoryRange(
            Guid subTaskId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            try
            {
                var history = await _taskService.GetSubTaskHistoryAsync(subTaskId, startTime, endTime);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取子任务历史记录失败: {SubTaskId}", subTaskId);
                return StatusCode(500, new { error = "获取子任务历史记录失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 添加子任务历史记录
        /// </summary>
        /// <param name="dataPoint">历史数据点</param>
        /// <returns>操作结果</returns>
        [HttpPost("history")]
        public async Task<ActionResult<bool>> AddSubTaskHistory([FromBody] SubTaskDataPoint dataPoint)
        {
            try
            {
                if (dataPoint == null)
                {
                    return BadRequest(new { error = "历史数据点不能为空" });
                }

                var result = await _taskService.AddSubTaskHistoryAsync(dataPoint);
                if (result)
                {
                    return Ok(new { success = true });
                }
                return BadRequest(new { error = "添加历史记录失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加子任务历史记录失败: {SubTaskId}", dataPoint?.SubTaskId);
                return StatusCode(500, new { error = "添加历史记录失败", message = ex.Message });
            }
        }
        #endregion

        #region 任务统计
        /// <summary>
        /// 获取主任务状态统计
        /// </summary>
        /// <returns>状态统计</returns>
        [HttpGet("statistics/main")]
        public async Task<ActionResult<Dictionary<System.Threading.Tasks.TaskStatus, int>>> GetMainTaskStatusStatistics()
        {
            try
            {
                var statistics = await _taskService.GetMainTaskStatusStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取主任务状态统计失败");
                return StatusCode(500, new { error = "获取主任务状态统计失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取子任务状态统计
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <returns>状态统计</returns>
        [HttpGet("{mainTaskId:guid}/statistics/sub")]
        public async Task<ActionResult<Dictionary<System.Threading.Tasks.TaskStatus, int>>> GetSubTaskStatusStatistics(Guid mainTaskId)
        {
            try
            {
                var statistics = await _taskService.GetSubTaskStatusStatisticsAsync(mainTaskId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取子任务状态统计失败: {MainTaskId}", mainTaskId);
                return StatusCode(500, new { error = "获取子任务状态统计失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取主任务完成率
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <returns>完成率</returns>
        [HttpGet("{mainTaskId:guid}/completion-rate")]
        public async Task<ActionResult<double>> GetMainTaskCompletionRate(Guid mainTaskId)
        {
            try
            {
                var completionRate = await _taskService.GetMainTaskCompletionRateAsync(mainTaskId);
                return Ok(new { completionRate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取主任务完成率失败: {MainTaskId}", mainTaskId);
                return StatusCode(500, new { error = "获取主任务完成率失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 获取主任务持续时间
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <returns>持续时间</returns>
        [HttpGet("{mainTaskId:guid}/duration")]
        public async Task<ActionResult<TimeSpan>> GetMainTaskDuration(Guid mainTaskId)
        {
            try
            {
                var duration = await _taskService.GetMainTaskDurationAsync(mainTaskId);
                return Ok(new { duration = duration.TotalSeconds });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取主任务持续时间失败: {MainTaskId}", mainTaskId);
                return StatusCode(500, new { error = "获取主任务持续时间失败", message = ex.Message });
            }
        }
        #endregion

        #region 批量操作
        /// <summary>
        /// 批量更新主任务
        /// </summary>
        /// <param name="tasks">主任务列表</param>
        /// <returns>操作结果</returns>
        [HttpPut("bulk/main")]
        public async Task<ActionResult<bool>> BulkUpdateMainTasks([FromBody] List<MainTask> tasks)
        {
            try
            {
                if (tasks == null || !tasks.Any())
                {
                    return BadRequest(new { error = "主任务列表不能为空" });
                }

                var result = await _taskService.BulkUpdateMainTasksAsync(tasks);
                if (result)
                {
                    return Ok(new { success = true, count = tasks.Count });
                }
                return BadRequest(new { error = "批量更新主任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新主任务失败: {Count}个", tasks?.Count);
                return StatusCode(500, new { error = "批量更新主任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 批量更新子任务
        /// </summary>
        /// <param name="subTasks">子任务列表</param>
        /// <returns>操作结果</returns>
        [HttpPut("bulk/sub")]
        public async Task<ActionResult<bool>> BulkUpdateSubTasks([FromBody] List<SubTask> subTasks)
        {
            try
            {
                if (subTasks == null || !subTasks.Any())
                {
                    return BadRequest(new { error = "子任务列表不能为空" });
                }

                var result = await _taskService.BulkUpdateSubTasksAsync(subTasks);
                if (result)
                {
                    return Ok(new { success = true, count = subTasks.Count });
                }
                return BadRequest(new { error = "批量更新子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新子任务失败: {Count}个", subTasks?.Count);
                return StatusCode(500, new { error = "批量更新子任务失败", message = ex.Message });
            }
        }

        /// <summary>
        /// 批量分配子任务
        /// </summary>
        /// <param name="mainTaskId">主任务ID</param>
        /// <param name="assignments">分配字典</param>
        /// <returns>操作结果</returns>
        [HttpPost("{mainTaskId:guid}/bulk-assign")]
        public async Task<ActionResult<bool>> BulkAssignSubTasks(Guid mainTaskId, [FromBody] Dictionary<Guid, Guid> assignments)
        {
            try
            {
                if (assignments == null || !assignments.Any())
                {
                    return BadRequest(new { error = "分配信息不能为空" });
                }

                var result = await _taskService.BulkAssignSubTasksAsync(mainTaskId, assignments);
                if (result)
                {
                    return Ok(new { success = true, count = assignments.Count });
                }
                return BadRequest(new { error = "批量分配子任务失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量分配子任务失败: {MainTaskId}, {Count}个", mainTaskId, assignments?.Count);
                return StatusCode(500, new { error = "批量分配子任务失败", message = ex.Message });
            }
        }
        #endregion
    }

    #region 请求模型
    /// <summary>
    /// 添加主任务请求
    /// </summary>
    public class AddMainTaskRequest
    {
        public MainTask Task { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// 保存图片请求
    /// </summary>
    public class SaveImageRequest
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public int ImageIndex { get; set; } = 1;
        public string? Description { get; set; }
    }
    #endregion
}
