using Microsoft.AspNetCore.SignalR;
using TaskManagement.Api.Hubs;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Api.Services
{
    public class KanbanTreeBroadcaster : IKanbanTreeBroadcaster
    {
        private readonly IHubContext<KanbanHub> _hubContext;
        private readonly IDashboardService _dashboardService;

        public KanbanTreeBroadcaster(
            IHubContext<KanbanHub> hubContext,
            IDashboardService dashboardService)
        {
            _hubContext = hubContext;
            _dashboardService = dashboardService;
        }

        public async Task BroadcastKanbanTreeUpdateAsync(Guid groupId)
        {
            var result = await _dashboardService.GetGroupTreeProgressAsync(groupId);

            if (result.IsFailure) return;

            var tree = result.Value!;

            await _hubContext.Clients
                    .Group($"kanban-{groupId}")
                    .SendAsync("TreeUpdated",
                    groupId.ToString(),
                    (int)tree.CurrentTreeStage,
                    tree.CompletionPercentage);
        }

    }
}
