using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Core.DTO.Comments;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        IApplicationDbContext context,
        ILogger<CommentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CommentDto> AddCommentAsync(Guid taskId, CreateCommentDto createCommentDto, Guid userId)
    {
        _logger.LogInformation("User {UserId} adding comment to task {TaskId}", userId, taskId);

        var task = await _context.Tasks
            .Include(t => t.Group)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == task.GroupId && gm.UserId == userId);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You must be a member of the group to comment on tasks");
        }

        var comment = new TaskComment
        {
            TaskId = taskId,
            Content = createCommentDto.Content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} added to task {TaskId} by user {UserId}",
            comment.Id, taskId, userId);

        return await GetCommentByIdAsync(comment.Id, userId);
    }

    public async Task<List<CommentDto>> GetTaskCommentsAsync(Guid taskId, Guid userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Group)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == task.GroupId && gm.UserId == userId);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You must be a member of the group to view task comments");
        }

        var comments = await _context.TaskComments
            .Where(tc => tc.TaskId == taskId)
            .Include(tc => tc.User)
            .OrderBy(tc => tc.CreatedAt)
            .Select(tc => new CommentDto
            {
                Id = tc.Id,
                TaskId = tc.TaskId,
                Content = tc.Content,
                UserId = tc.UserId,
                UserName = tc.User.UserName,
                CreatedAt = tc.CreatedAt,
                UpdatedAt = tc.UpdatedAt
            })
            .ToListAsync();

        return comments;
    }

    public async Task<CommentDto> UpdateCommentAsync(Guid commentId, UpdateCommentDto updateCommentDto, Guid userId)
    {
        var comment = await _context.TaskComments
            .Include(tc => tc.Task)
            .ThenInclude(t => t.Group)
            .FirstOrDefaultAsync(tc => tc.Id == commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException("Comment not found");
        }

        if (comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only edit your own comments");
        }

        comment.Content = updateCommentDto.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        comment.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} updated by user {UserId}", commentId, userId);

        return await GetCommentByIdAsync(commentId, userId);
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.TaskComments
            .Include(tc => tc.Task)
            .ThenInclude(t => t.Group)
            .FirstOrDefaultAsync(tc => tc.Id == commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException("Comment not found");
        }

        var membership = await _context.GroupMembers
            .Include(gm => gm.Role)
            .FirstOrDefaultAsync(gm => gm.GroupId == comment.Task.GroupId && gm.UserId == userId);

        if (membership == null)
        {
            throw new UnauthorizedAccessException("You must be a member of the group");
        }

        var canDelete = comment.UserId == userId || membership.Role.PermissionLevel >= 75;

        if (!canDelete)
        {
            throw new UnauthorizedAccessException("You can only delete your own comments or you must be a Manager or Owner");
        }

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        comment.DeletedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} deleted by user {UserId}", commentId, userId);
    }

    private async Task<CommentDto> GetCommentByIdAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.TaskComments
            .Include(tc => tc.User)
            .FirstOrDefaultAsync(tc => tc.Id == commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException("Comment not found");
        }

        return new CommentDto
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            Content = comment.Content,
            UserId = comment.UserId,
            UserName = comment.User.UserName,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}