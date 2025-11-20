using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Enums;
namespace TaskManagement.Core.DTO.Groups
{
    public class GroupMemberDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public GroupRole Role { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
