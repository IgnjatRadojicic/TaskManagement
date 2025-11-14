using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Core.Common
{
    public class EmailSettings
    {
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string SmptServer { get; set; } = string.Empty;
        public int SmptPort { get; set; }
        public string SmptUser { get; set; } = string.Empty;
        public string SmptPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
    }
}
