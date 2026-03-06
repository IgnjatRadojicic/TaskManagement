using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.DTO.Dashboard;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : BaseApiController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PersonalDashboardDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPersonalDashboard()
        {
            var userId = GetUserId();
            var dashboard = await _dashboardService.GetPersonalDashboardAsync(userId);
            return Ok(dashboard);
        }

        [HttpGet("field")]
        [ProducesResponseType(typeof(List<FieldTreeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFieldData()
        {
            var userId = GetUserId();
            var trees = await _dashboardService.GetFieldDataAsync(userId);
            return Ok(trees);
        }

        [HttpGet("groups/{groupId}")]
        [ProducesResponseType(typeof(GroupStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupStatistics(Guid groupId)
        {
            var userId = GetUserId();
            var statistics = await _dashboardService.GetGroupStatisticsAsync(groupId, userId);
            return Ok(statistics);
        }
    }
}