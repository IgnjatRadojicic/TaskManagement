using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Core.Interfaces
{
    public interface IKanbanTreeBroadcaster
    {
        Task BroadcastKanbanTreeUpdateAsync(Guid groupId);
    }
}
