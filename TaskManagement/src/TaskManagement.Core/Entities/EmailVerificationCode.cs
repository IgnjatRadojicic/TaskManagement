using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Common;

namespace TaskManagement.Core.Entities
{
    public class EmailVerificationCode : ImmutableEntity
    {
        public string Email { get; set; } = string.Empty;
        public string CodeHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }
    }
}
