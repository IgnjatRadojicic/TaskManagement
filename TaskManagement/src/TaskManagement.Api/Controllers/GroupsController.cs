using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.DTO.Groups;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
        {
            _groupService = groupService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto createGroupDto)
        {
            try
            {
                var userId = GetUserId();
                var group = await _groupService.CreateGroupAsync(createGroupDto, userId);
                return Ok(group);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating group");
                return StatusCode(500, new { message = "An error occurred while creating the group" });
            }
        }
        [HttpPost("join")]
        [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> JoinGroup([FromBody] JoinGroupDto joinGroupDto)
        {
            try
            {
                var userId = GetUserId();
                var group = await _groupService.JoinGroupAsync(joinGroupDto, userId);
                return Ok(group);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining group");
                return StatusCode(500, new { message = "An error occurred while joining the group" });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<GroupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyGroups()
        {
            try
            {
                var userId = GetUserId();
                var groups = await _groupService.GetMyGroupsAsync(userId);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user groups");
                return StatusCode(500, new { message = "An error occurred while retreving groups" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GroupDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupDetails(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var groupDetails = await _groupService.GetGroupDetailsAsync(id, userId);
                return Ok(groupDetails);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group details");
                return StatusCode(500, new { message = "An error occurred while retrieving group details" });
            }
        }
        [HttpPut("{groupId}/members/{memberId}/role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMemberRole(Guid groupId, Guid memberId, [FromBody] UpdateMemberRoleDto updateRoleDto)
        {
            try
            {
                var userId = GetUserId();
                await _groupService.UpdateMemberRoleAsync(groupId, memberId, updateRoleDto, userId);
                return Ok(new { message = "Member role updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member role");
                return StatusCode(500, new { message = "An error occurred while updating member role" });
            }
        }

        [HttpDelete("{id}/leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LeaveGroup(Guid id)
        {
            try
            {
                var userId = GetUserId();
                await _groupService.LeaveGroupAsync(id, userId);
                return Ok(new { message = "Successfully left the group" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving group");
                return StatusCode(500, new { message = "An error occurred while leaving the group" });
            }
        }
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            try
            {
                var userId = GetUserId();
                await _groupService.DeleteGroupAsync(id, userId);
                return Ok(new { message = "Group deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group");
                return StatusCode(500, new { message = "An error occurred while deleting the group" });
            }
        }



        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User Id was not found in token");
            }
            return userId;
        }
    }
}
