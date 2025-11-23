using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Common;
using TaskManagement.Core.Entities.Lookups;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Entities
{
    public class GroupMember : BaseEntity
    {

        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public virtual Group Group { get; set; } = null!;
        public GroupRoleLookup Role { get; set; } = null!;
        public virtual User User { get; set; } = null!; 
    }
}
