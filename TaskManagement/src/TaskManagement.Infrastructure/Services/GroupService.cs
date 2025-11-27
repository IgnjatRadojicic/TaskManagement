using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Core.DTO.Groups;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Services
{
    public class GroupService : IGroupService
    {
        private readonly IApplicationDbContext _context;
        private readonly IGroupCodeGenerator _codeGenerator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<GroupService> _logger;

        public GroupService(
            IApplicationDbContext context,
            IGroupCodeGenerator codeGenerator,
            IPasswordHasher passwordHasher,
            ILogger<GroupService> logger
            )
        {
            _context = context;
            _codeGenerator = codeGenerator;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<GroupDto> CreateGroupAsync(CreateGroupDto createGroupDto, Guid userId)
        {
            var groupCode = await GenerateUniqueGroupCode(createGroupDto.Name);

            string? passwordHash = null;
            if (!string.IsNullOrEmpty(createGroupDto.Password))
            {
                passwordHash = _passwordHasher.HashPassword(createGroupDto.Password);
            }

            var group = new Group
            {
                Name = createGroupDto.Name,
                GroupCode = groupCode,
                PasswordHash = passwordHash,
                IsActive = true,
                OwnerId = userId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);


            var ownerMember = new GroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                RoleId = (int)GroupRole.Owner,
                JoinedAt = DateTime.UtcNow,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.GroupMembers.Add(ownerMember);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Group created: {GroupName} with code {GroupCode} by user {UserId}",
                group.Name, group.GroupCode, userId);

            return MapToGroupDto(group, userId);

        }

        public async Task<GroupDto> JoinGroupAsync(JoinGroupDto joinGroupDto, Guid userId)
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.GroupCode == joinGroupDto.GroupCode);
            if (group == null)
            {
                throw new InvalidOperationException("Invalid group code");

            }
            if (!group.IsActive)
            {
                throw new InvalidOperationException("This group is no longer active");
            }

            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == group.Id && gm.UserId == userId);

            if (existingMember != null)
            {
                throw new InvalidOperationException("This user already exists");
            }

            if (!string.IsNullOrEmpty(group.PasswordHash))
            {
                if (string.IsNullOrEmpty(joinGroupDto.Password))
                {
                    throw new UnauthorizedAccessException("This group requires a password");
                }
                if (!_passwordHasher.VerifyPassword(joinGroupDto.Password, group.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Incorrect group password");
                }
            }

            var newMember = new GroupMember {
                GroupId = group.Id,
                UserId = userId,
                RoleId = (int)GroupRole.Member,
                JoinedAt = DateTime.UtcNow,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow

            };

            _context.GroupMembers.Add(newMember);

            await _context.SaveChangesAsync();

            return MapToGroupDto(group, userId);
        }

        public async Task<List<GroupDto>> GetUserGroupsAsync(Guid userId)
        {
            // Databse -> DTO -> List
            var groups = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                .Include(gm => gm.Role)

                .Select(gm => new GroupDto
                {
                    Id = gm.Group.Id,
                    Name = gm.Group.Name,
                    GroupCode = gm.Group.GroupCode,
                    IsPasswordProtected = !string.IsNullOrEmpty(gm.Group.PasswordHash),
                    MemberCount = _context.GroupMembers.Count(m => m.GroupId == gm.GroupId),
                    UserRole = (GroupRole)gm.RoleId,
                    JoinedAt = gm.JoinedAt,
                    CreatedAt = gm.Group.CreatedAt,

                }).ToListAsync();

            return groups;
        }

        public async Task<GroupDetailsDto> GetGroupDetailsAsync(Guid groupId, Guid userId)
        {
            var membership = await _context.GroupMembers
                .Include(gm => gm.Role)
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
            if (membership == null)
            {
                throw new UnauthorizedAccessException("You are not a member of this group");
            }

            var group = await _context.Groups
                .Include(g => g.Owner)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                throw new UnauthorizedAccessException("Group not found");
            }

            var members = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .Include(gm => gm.User)
                .Include(gm => gm.Role)
                .Select(gm => new GroupMemberDto
                {
                    UserId = gm.UserId,
                    UserName = gm.User.UserName,
                    Email = gm.User.Email,
                    Role = (GroupRole)gm.RoleId,
                    JoinedAt = gm.JoinedAt


                }).ToListAsync();


            return new GroupDetailsDto
            {
                Id = group.Id,
                Name = group.Name,
                GroupCode = group.GroupCode,
                IsPasswordProtected = !string.IsNullOrEmpty(group.PasswordHash),
                OwnerId = group.OwnerId,
                OwnerName = group.Owner.UserName,
                CreatedAt = group.CreatedAt,
                Members = members
            };
        }

        public async Task<GroupDto> UpdateGroupAsync(Guid groupId, UpdateGroupDto updateGroupDto, Guid userId)
        {
            var group = await _context.Groups.FindAsync(groupId);

            if (group == null)
            {
                throw new KeyNotFoundException("Group not found");
            }

            var membership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (membership == null)
            {
                throw new UnauthorizedAccessException("You are not a member of this group");
            }

            var userRole = (GroupRole)membership.RoleId;


            if (userRole != GroupRole.Owner && userRole != GroupRole.Manager)
            {
                throw new UnauthorizedAccessException("You don't have permission to update member roles");
            }

            if (!string.IsNullOrEmpty(updateGroupDto.Name))
            {
                group.Name = updateGroupDto.Name;
            }

            if (updateGroupDto.Password != null)
            {
                group.PasswordHash = string.IsNullOrEmpty(updateGroupDto.Password)
                    ? null : _passwordHasher.HashPassword(updateGroupDto.Password);
            }

            group.UpdatedBy = userId;
            group.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToGroupDto(group, userId);

        }
        public async Task<GroupMemberDto> ChangeUserRoleAsync(
            Guid groupId,
            Guid memberId,
            ChangeRoleDto changeRoleDto,
            Guid userId)
        {
            var requestingMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (requestingMembership == null)
            {
                throw new UnauthorizedAccessException("You are not a member of this group");
            }

            var requestingRole = (GroupRole)requestingMembership.RoleId;
            if (requestingRole != GroupRole.Owner && requestingRole != GroupRole.Manager)
            {
                throw new UnauthorizedAccessException("Only Owner or Manager can change roles");
            }

            var targetMembership = await _context.GroupMembers
                .Include(gm => gm.User)
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (targetMembership == null)
            {
                throw new KeyNotFoundException("Member not found in this group");
            }

            if ((GroupRole)targetMembership.RoleId == GroupRole.Owner)
            {
                throw new InvalidOperationException("Cannot change the role of group Owner");
            }

            targetMembership.RoleId = (int)changeRoleDto.NewRole;
            targetMembership.UpdatedBy = userId;
            targetMembership.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Role changed for member {MemberId} to {NewRole}", memberId, changeRoleDto.NewRole);

            return new GroupMemberDto
            {
                UserId = targetMembership.UserId,
                UserName = targetMembership.User.UserName,
                Email = targetMembership.User.Email,
                Role = changeRoleDto.NewRole,
                JoinedAt = targetMembership.JoinedAt
            };
        }

        public async Task RemoveUserFromGroupAsync(Guid groupId, Guid memberId, Guid userId)
        {
            _logger.LogInformation("User {UserId} removing member {MemberId} from group {GroupId}",
                userId, memberId, groupId);

            var requestingMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (requestingMembership == null)
            {
                throw new UnauthorizedAccessException("You are not a member of this group");
            }

            var requestingRole = (GroupRole)requestingMembership.RoleId;
            if (requestingRole != GroupRole.Owner && requestingRole != GroupRole.Manager)
            {
                throw new UnauthorizedAccessException("Only Owner or Manager can remove members");
            }

            
            var targetMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == memberId);

            if (targetMembership == null)
            {
                throw new KeyNotFoundException("Member not found in this group");
            }

            
            if ((GroupRole)targetMembership.RoleId == GroupRole.Owner)
            {
                throw new InvalidOperationException("Cannot remove the group owner");
            }

            targetMembership.IsDeleted = true;
            targetMembership.DeletedAt = DateTime.UtcNow;
            targetMembership.DeletedBy = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Member {MemberId} removed from group {GroupId}", memberId, groupId);


        }

        public async Task LeaveGroupAsync(Guid groupId, Guid userId)
        {
            var membership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if(membership == null)
            {
                throw new InvalidOperationException("You are not a member of this group");
            }

            if ((GroupRole)membership.RoleId == GroupRole.Owner)
            {
                throw new InvalidOperationException("Group owner cannot leave. Delete the group instead or transfer ownership.");
            }

            membership.IsDeleted = true;
            membership.DeletedAt = DateTime.UtcNow;
            membership.DeletedBy = userId;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} left group {GroupId}", userId, groupId);
        }
        
        private GroupDto MapToGroupDto(Group group, Guid userId)
        {
            var memberCount = _context.GroupMembers.Count(gm => gm.GroupId == group.Id);
            var userMembership = _context.GroupMembers
                .FirstOrDefault(gm => gm.GroupId == group.Id && gm.UserId == userId);

            return new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                GroupCode = group.GroupCode,
                IsPasswordProtected = !string.IsNullOrEmpty(group.PasswordHash),
                MemberCount = memberCount,
                UserRole = userMembership != null ? (GroupRole)userMembership.RoleId : GroupRole.Member,
                JoinedAt = userMembership?.JoinedAt ?? DateTime.UtcNow,
                CreatedAt = group.CreatedAt
            };
        }
        public async Task<string> GenerateUniqueGroupCode(string groupName)
        {
            string code;
            bool codeExists;

            do
            {
                code = _codeGenerator.Generate(groupName);
                codeExists = await _context.Groups.AnyAsync(g => g.GroupCode == code);

            }
            while (codeExists);
            return code;
        }
    }
}
