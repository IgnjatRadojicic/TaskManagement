using TaskManagement.Web.Interfaces;
using TaskManagement.Web.Models;

namespace TaskManagement.Web.Services
{
    public class DashboardService : BaseApiService, IDashboardService
    {
        public DashboardService(HttpClient http) : base(http) {}

        public async Task<ServiceResult<List<FieldTreeDto>>> GetFieldDataAsync()
        {
            return await GetAsync<List<FieldTreeDto>>("api/dashboard/field");
        }
    }
}
