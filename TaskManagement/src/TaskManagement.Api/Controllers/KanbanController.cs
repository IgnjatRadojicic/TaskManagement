using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.DTO.Kanban;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KanbanController : BaseApiController
    {
        private readonly ITaskService _taskService;
        private readonly IAuditService _auditService;

        public KanbanController(ITaskService taskService, IAuditService auditService)
        {
            _taskService = taskService;
            _auditService = auditService;
        }

        [HttpGet("{groupId}")]
        [ProducesResponseType(typeof(KanbanBoardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetBoard(Guid groupId)
        {
            var userId = GetUserId();
            var board = await _taskService.GetKanbanBoardAsync(groupId, userId);
            return Ok(board);
        }

        [HttpPut("{taskId}/move")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MoveTask(Guid taskId, [FromBody] MoveTaskDto moveDto)
        {
            var userId = GetUserId();
            await _taskService.MoveTaskAsync(taskId, moveDto, userId);

            await LogAuditAsync(
                _auditService,
                entityType: "TaskItem",
                entityId: taskId,
                action: "Moved",
                propertyName: "StatusId",
                newValue: moveDto.NewStatusId.ToString());

            return Ok(new { message = "Task moved successfully" });
        }
    }
}