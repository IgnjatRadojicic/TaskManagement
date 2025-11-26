using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.DTO.Groups
{
    public class ChangeRoleDto
    {
        public GroupRole NewRole { get; set; }
    }
}
