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
        public string Token { get; set; } = string.Empty; 
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }

        
        public virtual User User { get; set; } = null!;

       
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsUsed => UsedAt != null;
        public bool IsValid => !IsExpired && !IsUsed;
    }
}
