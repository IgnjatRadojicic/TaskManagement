using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Core.Enums
{
    public enum TaskStatus
    {

        NotStarted = 0,
        InProgress = 1,
        UnderReview = 2,
        Completed = 3,
        Cancelled = 4
    }
}
