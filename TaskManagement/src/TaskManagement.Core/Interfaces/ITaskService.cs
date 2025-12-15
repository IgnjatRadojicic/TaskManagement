using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTO.Tasks;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(Guid groupId, CreateTaskDto createTaskDto, Guid userId);
        Task<List<TaskDto>> GetGroupTasksAsync(Guid groupId, TaskFilterDto? filter, Guid userId);
        Task<TaskDto> GetTaskByIdAsync(Guid taskId, Guid userId);
        Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto updateTaskDto, Guid userId);
        Task<TaskDto> ChangeTaskStatusAsync(Guid taskId, ChangeTaskStatusDto statusDto, Guid userId);
        Task<TaskDto> ChangeTaskPriorityAsync(Guid taskId, int newPriorityId, Guid userId);
        Task AssignTaskAsync(Guid taskId, AssignTaskDto assignDto, Guid userId);
        Task UnassignTaskAsync(Guid taskId, Guid userId);
        Task DeleteTaskAsync(Guid taskId, Guid userId);
    }
}
