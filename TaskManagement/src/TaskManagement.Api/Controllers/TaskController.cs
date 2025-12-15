using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.DTO.Tasks;
using TaskManagement.Core.Interfaces;
namespace TaskManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : BaseApiController
    {
        private readonly ITaskService _taskService;
        private readonly IAuditService _auditService;
        private readonly ILogger<TaskController> _logger;

        public TaskController(
            ITaskService taskService,
            IAuditService auditService,
            ILogger<TaskController> logger)
        {
            _taskService = taskService;
            _auditService = auditService;
            _logger = logger;
        }

        [HttpPost("groups/{groupId}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> CreateTask(Guid groupId, [FromBody] CreateTaskDto createTaskDto)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.CreateTaskAsync(groupId, createTaskDto, userId);

                await LogAuditAsync(
                    _auditService,
                    entityType: "TaskItem",
                    entityId: task.Id,
                    action: "Created",
                    groupId: groupId);

                return CreatedAtAction(
                    nameof(GetTaskById),
                    new { taskId = task.Id },
                    task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task in group {GroupId}", groupId);
                return StatusCode(500, new { message = "An error occurred while creating the task" });
            }

        }
        [HttpGet("groups/{groupId}")]
        [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGroupTasks(
            Guid groupId,
            [FromQuery] int? statusId = null,
            [FromQuery] int? priorityId = null,
            [FromQuery] Guid? assignedToUserId = null,
            [FromQuery] bool? isOverDue = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var userId = GetUserId();

                var filter = new TaskFilterDto
                {
                    StatusId = statusId,
                    PriorityId = priorityId,
                    AssignedToUserId = assignedToUserId,
                    IsOverDue = isOverDue,
                    SearchTerm = searchTerm
                };

                var tasks = await _taskService.GetGroupTasksAsync(groupId, filter, userId);

                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks for group {GroupId}", groupId);
                return StatusCode(500, new { message = "An error occurred while retrieving tasks" });
            }
        }

        [HttpGet("{taskId}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.GetTaskByIdAsync(taskId, userId);

                return Ok(task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while retrieving the task" });
            }
        }

        [HttpPut("{taskId}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateTaskDto updateTaskDto)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.UpdateTaskAsync(taskId, updateTaskDto, userId);

                await LogAuditAsync(
                    _auditService,
                    entityType: "TaskItem",
                    entityId: taskId,
                    action: "Updated",
                    groupId: task.GroupId);

                return Ok(task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while updating the task" });
            }
        }

        [HttpPut("{taskId}/status")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeTaskStatus(Guid taskId, [FromBody] ChangeTaskStatusDto statusDto)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.ChangeTaskStatusAsync(taskId, statusDto, userId);

                await LogAuditAsync(
                    _auditService,
                    entityType: "TaskItem",
                    entityId: taskId,
                    action: "StatusChanged",
                    groupId: task.GroupId,
                    propertyName: "Status",
                    newValue: task.StatusDisplayName);

                return Ok(task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing status for task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while changing task status" });
            }
        }

        [HttpPut("{taskId}/priority")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeTaskPriority(Guid taskId, [FromBody] int newPriorityId)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.ChangeTaskPriorityAsync(taskId, newPriorityId, userId);

                await LogAuditAsync(
                    _auditService,
                    entityType: "TaskItem",
                    entityId: taskId,
                    action: "PriorityChanged",
                    groupId: task.GroupId,
                    propertyName: "Priority",
                    newValue: task.PriorityName);

                return Ok(task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing priority for task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while changing task priority" });
            }
        }

        [HttpPost("{taskId}/assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignTask(Guid taskId, [FromBody] AssignTaskDto assignDto)
        {
            try
            {
                var userId = GetUserId();
                await _taskService.AssignTaskAsync(taskId, assignDto, userId);

                var task = await _taskService.GetTaskByIdAsync(taskId, userId);

                await LogAuditAsync(
                    _auditService,
                    entityType: "TaskItem",
                    entityId: taskId,
                    action: "Assigned",
                    groupId: task.GroupId,
                    propertyName: "AssignedTo",
                    newValue: task.AssignedToUserName);

                return Ok(new { message = "Task assigned successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while assigning the task" });
            }
        }

        [HttpPost("{taskId}/unassign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnassignTask(Guid taskId)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.GetTaskByIdAsync(taskId, userId);

                await _taskService.UnassignTaskAsync(taskId, userId);

                await LogAuditAsync(
                    _auditService,
                    entityType: "TaskItem",
                    entityId: taskId,
                    action: "Unassigned",
                    groupId: task.GroupId,
                    propertyName: "AssignedTo",
                    oldValue: task.AssignedToUserName);

                return Ok(new { message = "Task unassigned successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while unassigning the task" });
            }
        }

        [HttpDelete("{taskId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.GetTaskByIdAsync(taskId, userId);

                await _taskService.DeleteTaskAsync(taskId, userId);

                await LogAuditAsync(
                    _auditService,
                    entityType: "TaskItem",
                    entityId: taskId,
                    action: "Deleted",
                    groupId: task.GroupId);

                return Ok(new { message = "Task deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", taskId);
                return StatusCode(500, new { message = "An error occurred while deleting the task" });
            }
        }
    }

}

    