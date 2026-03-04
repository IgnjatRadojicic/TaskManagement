using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTO.Kanban;

namespace TaskManagement.Core.Interfaces
{
    public interface IKanbanBroadcaster
    {
        Task BroadcastTaskMovedAsync(Guid groupId, Guid taskId, MoveTaskDto moveDto, Guid movedByUserId);
    }
}
