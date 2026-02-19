using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Core.Interfaces
{
    public interface IBackgroundJobService
    {

        string ScheduleTaskDueSoonNotification(Guid taskid, DateTime dueDate);

        void CancelScheduledJob(string jobId);

        void SetupRecurringJobs();

        void TriggerOverdueCheck();
    }
}
