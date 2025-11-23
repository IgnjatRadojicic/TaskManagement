
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Core.DTO.Audit;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        public Task<List<AuditLogDto>> GetEntityHistoryAsync(string entityType, Guid entityId)
        {
            throw new NotImplementedException();
        }

        public Task<List<AuditLogDto>> GetUserHistoryAsync(Guid userId, int pageNumber = 1, int pageSize = 50)
        {
            throw new NotImplementedException();
        }

        public Task LogAsync(string entityType, Guid entityId, string action, Guid userId, string? propertyName = null, string? oldValue = null, string? newValue = null, string? reason = null)
        {
            throw new NotImplementedException();
        }
    }
}
