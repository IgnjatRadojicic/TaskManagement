using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.DTO.Attachments;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks/{taskId}/attachments")]
public class AttachmentsController : BaseApiController
{
    private readonly IAttachmentService _attachmentService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(
        IAttachmentService attachmentService,
        IAuditService auditService,
        ILogger<AttachmentsController> logger)
    {
        _attachmentService = attachmentService;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAttachment(Guid taskId, [FromForm] IFormFile file)
    {
        try
        {
            var userId = GetUserId();
            var attachment = await _attachmentService.UploadAttachmentAsync(taskId, file, userId);

            await LogAuditAsync(
                _auditService,
                entityType: "TaskAttachment",
                entityId: attachment.Id,
                action: "Uploaded",
                propertyName: "FileName",
                newValue: attachment.FileName);

            return CreatedAtAction(
                nameof(GetAttachment),
                new { taskId, attachmentId = attachment.Id },
                attachment);
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
            _logger.LogError(ex, "Error uploading attachment to task {TaskId}", taskId);
            return StatusCode(500, new { message = "An error occurred while uploading the file" });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskAttachments(Guid taskId)
    {
        try
        {
            var userId = GetUserId();
            var attachments = await _attachmentService.GetTaskAttachmentsAsync(taskId, userId);

            return Ok(attachments);
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
            _logger.LogError(ex, "Error getting attachments for task {TaskId}", taskId);
            return StatusCode(500, new { message = "An error occurred while retrieving attachments" });
        }
    }

    [HttpGet("{attachmentId}")]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttachment(Guid taskId, Guid attachmentId)
    {
        try
        {
            var userId = GetUserId();
            var attachment = await _attachmentService.GetAttachmentByIdAsync(attachmentId, userId);

            return Ok(attachment);
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
            _logger.LogError(ex, "Error getting attachment {AttachmentId}", attachmentId);
            return StatusCode(500, new { message = "An error occurred while retrieving the attachment" });
        }
    }

    [HttpGet("{attachmentId}/download")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAttachment(Guid taskId, Guid attachmentId)
    {
        try
        {
            var userId = GetUserId();
            var (fileStream, fileName, contentType) = await _attachmentService.DownloadAttachmentAsync(attachmentId, userId);

            return File(fileStream, contentType, fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = "File not found" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
            return StatusCode(500, new { message = "An error occurred while downloading the file" });
        }
    }

    [HttpDelete("{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttachment(Guid taskId, Guid attachmentId)
    {
        try
        {
            var userId = GetUserId();
            await _attachmentService.DeleteAttachmentAsync(attachmentId, userId);

            await LogAuditAsync(
                _auditService,
                entityType: "TaskAttachment",
                entityId: attachmentId,
                action: "Deleted");

            return Ok(new { message = "Attachment deleted successfully" });
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
            _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
            return StatusCode(500, new { message = "An error occurred while deleting the attachment" });
        }
    }
}