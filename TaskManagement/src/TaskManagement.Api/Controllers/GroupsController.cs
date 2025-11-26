using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.DTO.Groups;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class GroupsController : BaseApiController
    {
        private readonly IGroupService _groupService;
        private readonly IAuditService _auditService;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(
            IGroupService groupService,
            IAuditService auditService,
            ILogger<GroupsController> logger)
        {
            _groupService = groupService;
            _auditService = auditService;
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

                await LogAuditAsync(
                    _auditService,
                    entityType: "Group",
                    entityId: group.Id,
                    action: "Created",
                    groupId: group.Id);

                return Ok(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group");
                return StatusCode(500, new { message = "An error occurred while creating the group" });
            }
        }


        [HttpPost("join")]
        [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> JoinGroup([FromBody] JoinGroupDto joinGroupDto)
        {
            try
            {
                var userId = GetUserId();
                var group = await _groupService.JoinGroupAsync(joinGroupDto, userId);

                await LogAuditAsync(
                    _auditService,
                    entityType: "GroupMember",
                    entityId: userId,
                    action: "JoinedGroup",
                    groupId: group.Id);

                return Ok(group);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
                _logger.LogError(ex, "Error joining group");
                return StatusCode(500, new { message = "An error occurred while joining the group" });
            }
        }


        [HttpGet]
        [ProducesResponseType(typeof(List<GroupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserGroups()
        {
            try
            {
                var userId = GetUserId();
                var groups = await _groupService.GetUserGroupsAsync(userId);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user groups");
                return StatusCode(500, new { message = "An error occurred while retrieving groups" });
            }
        }


        [HttpGet("{groupId}")]
        [ProducesResponseType(typeof(GroupDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGroupDetails(Guid groupId)
        {
            try
            {
                var userId = GetUserId();
                var groupDetails = await _groupService.GetGroupDetailsAsync(groupId, userId);
                return Ok(groupDetails);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group details");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("{groupId}")]
        [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateGroup(Guid groupId, [FromBody] UpdateGroupDto updateGroupDto)
        {
            try
            {
                var userId = GetUserId();
                var group = await _groupService.UpdateGroupAsync(groupId, updateGroupDto, userId);


                await LogAuditAsync(
                    _auditService,
                    entityType: "Group",
                    entityId: groupId,
                    action: "Updated",
                    groupId: groupId);

                return Ok(group);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }


        [HttpPut("{groupId}/members/{memberId}/role")]
        [ProducesResponseType(typeof(GroupMemberDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangeUserRole(
            Guid groupId,
            Guid memberId,
            [FromBody] ChangeRoleDto changeRoleDto)
        {
            try
            {
                var userId = GetUserId();
                var member = await _groupService.ChangeUserRoleAsync(groupId, memberId, changeRoleDto, userId);

                // Log audit WITH groupId
                await LogAuditAsync(
                    _auditService,
                    entityType: "GroupMember",
                    entityId: memberId,
                    action: "RoleChanged",
                    groupId: groupId,
                    propertyName: "Role",
                    oldValue: null,
                    newValue: changeRoleDto.NewRole.ToString());

                return Ok(member);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
                _logger.LogError(ex, "Error changing user role");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }


        [HttpDelete("{groupId}/members/{memberId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveUserFromGroup(Guid groupId, Guid memberId)
        {
            try
            {
                var userId = GetUserId();
                await _groupService.RemoveUserFromGroupAsync(groupId, memberId, userId);


                await LogAuditAsync(
                    _auditService,
                    entityType: "GroupMember",
                    entityId: memberId,
                    action: "RemovedFromGroup",
                    groupId: groupId);

                return Ok(new { message = "Member removed successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
                _logger.LogError(ex, "Error removing member from group");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }


        [HttpPost("{groupId}/leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LeaveGroup(Guid groupId)
        {
            try
            {
                var userId = GetUserId();
                await _groupService.LeaveGroupAsync(groupId, userId);


                await LogAuditAsync(
                    _auditService,
                    entityType: "GroupMember",
                    entityId: userId,
                    action: "LeftGroup",
                    groupId: groupId);

                return Ok(new { message = "Left group successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving group");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
