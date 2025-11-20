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
            if(!string.IsNullOrEmpty(createGroupDto.Password))
            {
                passwordHash = _passwordHasher.HashPassword(createGroupDto.Password);
            }

            var group = new Group
            {
                Name = createGroupDto.Name,
                GroupCode = groupCode,
                PasswordHash = passwordHash,
                OwnerId = userId,
                IsActive = true
            };

            _context.Groups.Add(group);


            var ownerMember = new GroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                Role = GroupRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            _context.GroupMembers.Add(ownerMember);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Group created: {GroupName} with code {GroupCode} by user {UserId}",
                group.Name, group.GroupCode, userId);

            var owner = await _context.Users.FindAsync(userId);

            return new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                GroupCode = group.GroupCode,
                OwnerId = group.OwnerId,
                OwnerName = owner.UserName ?? "Unknown",
                IsActive = group.IsActive,
                CreatedAt = group.CreatedAt,
                MemberCount = 1,
                MyRole = GroupRole.Owner.ToString(),
            };
        }

       public async Task<GroupDto> JoinGroupAsync(JoinGroupDto joinGroupDto, Guid userId)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .Include(g => g.Owner)
                .FirstOrDefaultAsync(g => g.GroupCode == joinGroupDto.GroupCode);
            if (group == null)
            {
                throw new InvalidOperationException("Invalid group code");

            }
            if(!group.IsActive)
            {
                throw new InvalidOperationException("This group is no longer active");
            }

            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == group.Id && gm.UserId == userId);
            
            if(existingMember != null)
            {
                throw new InvalidOperationException("This user already exists");
            }

            if (!string.IsNullOrEmpty(group.PasswordHash))
            {
                if (string.IsNullOrEmpty(joinGroupDto.Password))
                {
                    throw new UnauthorizedAccessException("This group requires a password");
                }
                if(!_passwordHasher.VerifyPassword(joinGroupDto.Password, group.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Incorrect group password");
                }
            }

            var newMember = new GroupMember {
                GroupId = group.Id,
                UserId = userId,
                Role = GroupRole.Member,
                JoinedAt = DateTime.UtcNow

            };

            _context.GroupMembers.Add(newMember);

            await _context.SaveChangesAsync();

            return new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                GroupCode = group.GroupCode,
                OwnerId = group.OwnerId,
                OwnerName = group.Owner.UserName,
                IsActive = group.IsActive,
                CreatedAt = DateTime.UtcNow,
                MemberCount = group.Members.Count + 1,
                MyRole = GroupRole.Member.ToString()
            };
        }

        public async Task<List<GroupDto>> GetMyGroupsAsync(Guid userId)
        {
            // Databse -> DTO -> List
            var groups = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                    .ThenInclude(g => g.Owner)
                .Include(gm => gm.Group)
                    .ThenInclude(g => g.Members)

                .Select(gm => new GroupDto
                {
                    Id = gm.Group.Id,
                    Name = gm.Group.Name,
                    GroupCode = gm.Group.GroupCode,
                    OwnerId = gm.Group.OwnerId,
                    OwnerName = gm.Group.Owner.UserName,
                    IsActive = gm.Group.IsActive,
                    CreatedAt = gm.Group.CreatedAt,
                    MemberCount = gm.Group.Members.Count,
                    MyRole = gm.Role.ToString()

                }).ToListAsync();

            return groups;
        }

        public async Task<GroupDetailsDto> GetGroupDetailsAsync(Guid groupId, Guid userId)
        {
            var membership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
            if(membership == null)
            {
                throw new UnauthorizedAccessException("You are not a member of this group");
            }

            var group = await _context.Groups
                .Include(g => g.Owner)
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if(group == null)
            {
                throw new UnauthorizedAccessException("Group not found");
            }

            var members = group.Members.Select(m => new GroupMemberDto
            {
                UserId = m.UserId,
                UserName = m.User.UserName,
                Email = m.User.Email,
                Role = m.Role,
                JoinedAt = m.JoinedAt


            }).ToList();


            return new GroupDetailsDto
            {
                Id = group.Id,
                Name = group.Name,
                GroupCode = group.GroupCode,
                OwnerId = group.OwnerId,
                IsActive = group.IsActive,
                HasPassword = !string.IsNullOrEmpty(group.PasswordHash),
                CreatedAt = group.CreatedAt,
                Members = members
            };
        }

        public async Task UpdateMemberRoleAsync(Guid groupId, Guid memberId, UpdateMemberRoleDto updateRoleDto, Guid requestingUserId)
        {
            var requestingMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == requestingUserId);

            if (requestingMembership == null)
            {
                throw new UnauthorizedAccessException("You are not a member of this group");
            }

            if(requestingMembership.Role != GroupRole.Owner && requestingMembership.Role != GroupRole.Manager)
            {
                throw new UnauthorizedAccessException("You don't have permission to update member roles");
            }

            var targetMembers = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.UserId == memberId && gm.GroupId == groupId);


            if(targetMembers ==  null)
            {
                throw new InvalidOperationException("Member not found in this group");
            }
            if (targetMembers.Role == GroupRole.Owner)
            {
                throw new InvalidOperationException("Cannot change the owner's role");
            }

            
            if (updateRoleDto.Role == GroupRole.Owner)
            {
                throw new InvalidOperationException("Cannot promote member to owner");
            }

          
            if (requestingMembership.Role == GroupRole.Manager)
            {
                if (targetMembers.Role == GroupRole.Manager || updateRoleDto.Role == GroupRole.Manager)
                {
                    throw new UnauthorizedAccessException("Managers cannot manage other managers");
                }
            }

            targetMembers.Role = updateRoleDto.Role;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {RequestingUserId} updated role of user {MemberId} to {NewRole} in group {GroupId}",
                requestingUserId, memberId, updateRoleDto.Role, groupId);
        }

        public async Task LeaveGroupAsync(Guid groupId, Guid userId)
        {
            var membership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if(membership == null)
            {
                throw new InvalidOperationException("You are not a member of this group");
            }

            if (membership.Role == GroupRole.Owner)
            {
                throw new InvalidOperationException("Group owner cannot leave. Delete the group instead or transfer ownership.");
            }

            _context.GroupMembers.Remove(membership);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} left group {GroupId}", userId, groupId);
        }
        public async Task DeleteGroupAsync(Guid groupId, Guid userId)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                throw new InvalidOperationException("Group not found");
            }

            if(group.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("Only the group owner can delete the group");
            }

            _context.GroupMembers.RemoveRange(group.Members);
            _context.Groups.Remove(group);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Group {GroupId} deleted by owner {UserId}", groupId, userId);
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
