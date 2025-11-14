using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaskManagement.Core.Common;
using TaskManagement.Core.Enums;
using TaskStatus = TaskManagement.Core.Enums.TaskStatus;

namespace TaskManagement.Core.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Guid GroupId { get; set; }
        public Guid? AssignedToId { get; set; }
        public TaskStatus Stauts { get; set; } = TaskStatus.NotStarted;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        public virtual Group Group { get; set; } = null!;
        public virtual User? AssignedTo { get; set; }
        public virtual ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
    }
}
