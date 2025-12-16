using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.DTO.Attachments;
using Microsoft.AspNetCore.Http;

namespace TaskManagement.Core.Interfaces
{
    public interface IAttachmentService
    {
        Task<AttachmentDto> UploadAttachmentAsync(Guid taskId, IFormFile file, Guid userId);
        Task<List<AttachmentDto>> GetTaskAttachmentsAsync(Guid taskId, Guid userId);
        Task<AttachmentDto> GetAttachmentByIdAsync(Guid attachmentId, Guid userId);
        Task<(Stream FileStream, string FileName, string ContentType)> DownloadAttachmentAsync(Guid attachmentId, Guid userId);
        Task DeleteAttachmentAsync(Guid attachmentId, Guid userId);
    }
}
