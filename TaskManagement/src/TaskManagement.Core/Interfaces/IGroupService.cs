using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTO.Groups;

namespace TaskManagement.Core.Interfaces
{
    public interface IGroupService
    {
        Task<GroupDto> CreateGroupAsync(CreateGroupDto createGroupDto, Guid userId);
        Task<GroupDto> JoinGroupAsync(JoinGroupDto joinGroupDto, Guid userId);
        Task<List<GroupDto>> GetMyGroupsAsync(Guid userId);
        Task<GroupDetailsDto> GetGroupDetailsAsync(Guid groupId, Guid userId);
        Task UpdateMemberRoleAsync(Guid groupId, Guid memberId, UpdateMemberRoleDto updateRoleDto, Guid requestingUserId);
        Task LeaveGroupAsync(Guid groupId, Guid userId);
        Task DeleteGroupAsync(Guid groupId, Guid userId);
    }
}
