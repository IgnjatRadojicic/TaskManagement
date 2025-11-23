using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTO.Audit;

namespace TaskManagement.Core.Interfaces
{
    public interface IAuditService
        {
            Task LogAsync(
                string entityType,
                Guid entityId,
                string action,
                Guid userId,
                string? propertyName = null,
                string? oldValue = null,
                string? newValue = null,
                string? reason = null);


            Task<List<AuditLogDto>> GetEntityHistoryAsync(string entityType, Guid entityId);


            Task<List<AuditLogDto>> GetUserHistoryAsync(Guid userId, int pageNumber = 1, int pageSize = 50);
        }
}
