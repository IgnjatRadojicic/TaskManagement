using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTO.Comments;

namespace TaskManagement.Core.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto> AddCommentAsync(Guid taskId, CreateCommentDto createCommentDto, Guid userId);
        Task<List<CommentDto>> GetTaskCommentsAsync(Guid taskId, Guid userId);
        Task<CommentDto> UpdateCommentAsync(Guid commentId, UpdateCommentDto updateCommentDto, Guid userId);
        Task DeleteCommentAsync(Guid commentId, Guid userId);
    }
}