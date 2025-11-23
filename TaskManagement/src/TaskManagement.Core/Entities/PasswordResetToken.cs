using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Common;

namespace TaskManagement.Core.Entities
{
    public class PasswordResetToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty; 
        public DateTime ExpiresAt { get; set; }
        public bool IsUSed { get; set; } = false;
        public DateTime? UsedAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        
        public virtual User User { get; set; } = null!;

    }
}
