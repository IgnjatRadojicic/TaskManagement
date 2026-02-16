using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Core.DTO.Notifications
{
    public class MarkAsReadDto
    {
        public List<Guid> NotificationIds { get; set; } = new();
    }
}
