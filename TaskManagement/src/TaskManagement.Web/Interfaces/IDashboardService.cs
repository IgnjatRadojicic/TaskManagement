using TaskManagement.Web.Models;

namespace TaskManagement.Web.Interfaces
{
    public interface IDashboardService
    {
        Task<ServiceResult<List<FieldTreeDto>>> GetFieldDataAsync();
    }
}
