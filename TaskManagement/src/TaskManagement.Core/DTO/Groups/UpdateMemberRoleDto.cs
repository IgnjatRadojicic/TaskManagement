using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.DTO.Groups
{
    public class UpdateMemberRoleDto
    {
        [Required]
        public GroupRole Role { get; set; }
    }
}
