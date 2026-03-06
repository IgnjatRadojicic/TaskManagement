using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTO.Dashboard;

namespace TaskManagement.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<PersonalDashboardDto> GetPersonalDashboardAsync(Guid userId);
        Task<List<FieldTreeDto>> GetFieldDataAsync(Guid userId);
        Task<GroupStatisticsDto> GetGroupStatisticsAsync(Guid groupId, Guid userId);
    }
}
