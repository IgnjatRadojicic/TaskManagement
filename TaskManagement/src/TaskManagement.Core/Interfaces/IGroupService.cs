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
        Task<List<GroupDto>> GetUserGroupsAsync(Guid userId);
        Task<GroupDetailsDto> GetGroupDetailsAsync(Guid groupId, Guid userId);
        Task<GroupDto> UpdateGroupAsync(Guid groupId, UpdateGroupDto updateGroupDto, Guid userId);
        Task<GroupMemberDto> ChangeUserRoleAsync(
                    Guid groupId,
                    Guid memberId,
                    ChangeRoleDto changeRoleDto,
                    Guid userId);
        Task RemoveUserFromGroupAsync(Guid groupId, Guid memberId, Guid userId);
        Task LeaveGroupAsync(Guid groupId, Guid userId);
    }
}
