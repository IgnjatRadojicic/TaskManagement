using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Common;

namespace TaskManagement.Core.Entities
{
    public class TaskAttachment : BaseEntity
    {
        public Guid TaskId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize  { get; set; }
        public string ContentType { get; set; } = string.Empty;

        public virtual TaskItem Task { get; set; } = null!;
        public User Uploader { get; set; } = null!;

    }
}
