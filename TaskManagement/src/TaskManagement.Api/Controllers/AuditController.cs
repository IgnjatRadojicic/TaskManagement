using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.DTO.Audit;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuditController : BaseApiController
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditService auditService, ILogger<AuditController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        [HttpGet("groups/{groupId}")]
        [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGroupHistory(Guid groupId, 
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = GetUserId();
                var logs = await _auditService.GetGroupHistoryAsync(groupId, userId, pageNumber, pageSize);
                return Ok(logs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group audit history");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
        [HttpGet("tasks/{taskId}")]
        [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTaskHistory(Guid taskId)
        {
            try
            {
                var userId = GetUserId();
                var logs = await _auditService.GetTaskHistoryAsync(taskId, userId);
                return Ok(logs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task audit history");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
        [HttpGet("users/{userId}")]
        [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserHistory(Guid userId, [FromQuery]
        int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var requestingUserId = GetUserId();
                var logs = await _auditService.GetUserHistoryAsync(userId, requestingUserId, pageNumber, pageSize);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user audit history");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
  
}
