using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManagement.Core.Configuration;
using TaskManagement.Core.DTO.Attachments;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly FileStorageSettings _settings;
        private readonly ILogger<AttachmentService> _logger;

        public AttachmentService(
            IApplicationDbContext context,
            IFileStorageService fileStorage,
            IOptions<FileStorageSettings> settings,
            ILogger<AttachmentService> logger)
        {
            _context = context;
            _fileStorage = fileStorage;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<AttachmentDto> UploadAttachmentAsync(Guid taskId, IFormFile file, Guid userId)
        {
            _logger.LogInformation("User {UserId} uploading attachment to task {TaskId}", userId, taskId);

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
                throw new UnauthorizedAccessException("You must be a member of the group to upload attachments");
            }

            ValidateFile(file);

            string storagePath;
            using (var stream = file.OpenReadStream())
            {
                storagePath = await _fileStorage.UploadFileAsync(stream, file.FileName, file.ContentType);
            }

            var attachment = new TaskAttachment
            {
                TaskId = taskId,
                FileName = file.FileName,
                FilePath = storagePath,
                FileSize = file.Length,
                ContentType = file.ContentType,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Attachment {AttachmentId} uploaded to task {TaskId}", attachment.Id, taskId);

            return await GetAttachmentByIdAsync(attachment.Id, userId);
        }

        public async Task<List<AttachmentDto>> GetTaskAttachmentsAsync(Guid taskId, Guid userId)
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
                throw new UnauthorizedAccessException("You must be a member of the group to view attachments");
            }

            var attachments = await _context.TaskAttachments
                .Where(a => a.TaskId == taskId)
                .Include(a => a.Uploader)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    FileName = a.FileName,
                    FileSize = a.FileSize,
                    ContentType = a.ContentType,
                    DownloadUrl = _fileStorage.GetFileUrl(a.FilePath),
                    UploadedAt = a.CreatedAt,
                    UploadedByUserName = a.Uploader.UserName
                })
            .ToListAsync();

            return attachments;
        }

        public async Task<AttachmentDto> GetAttachmentByIdAsync(Guid attachmentId, Guid userId)
        {
            var attachment = await _context.TaskAttachments
                .Include(a => a.Task)
                .ThenInclude(t => t.Group)
                .Include(a => a.Uploader)
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
            {
                throw new KeyNotFoundException("Attachment not found");
            }

            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == attachment.Task.GroupId && gm.UserId == userId);

            if (!isMember)
            {
                throw new UnauthorizedAccessException("You must be a member of the group to view this attachment");
            }

            return new AttachmentDto
            {
                Id = attachment.Id,
                TaskId = attachment.TaskId,
                FileName = attachment.FileName,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                DownloadUrl = _fileStorage.GetFileUrl(attachment.FilePath),
                UploadedAt = attachment.CreatedAt,
                UploadedByUserName = attachment.Uploader.UserName
            };
        }

        public async Task<(Stream FileStream, string FileName, string ContentType)> DownloadAttachmentAsync(Guid attachmentId, Guid userId)
        {
            var attachment = await _context.TaskAttachments
                .Include(a => a.Task)
                .ThenInclude(t => t.Group)
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
            {
                throw new KeyNotFoundException("Attachment not found");
            }

            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == attachment.Task.GroupId && gm.UserId == userId);

            if (!isMember)
            {
                throw new UnauthorizedAccessException("You must be a member of the group to download this attachment");
            }

            var fileStream = await _fileStorage.DownloadFileAsync(attachment.FilePath);

            return (fileStream, attachment.FileName, attachment.ContentType);

        }
        public async Task DeleteAttachmentAsync(Guid attachmentId, Guid userId)
        {
            var attachment = await _context.TaskAttachments
                  .Include(a => a.Task)
                  .ThenInclude(t => t.Group)
                  .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
            {
                throw new KeyNotFoundException("Attachment not found");

            }

            var membership = await _context.GroupMembers
                .Include(gm => gm.Role)
                .FirstOrDefaultAsync(gm => gm.GroupId == attachment.Task.GroupId && gm.UserId == userId);

            if (membership == null)
            {
                throw new UnauthorizedAccessException("You must be a member of the group");
            }

            var canDelete = membership.Role.PermissionLevel >= 75 || attachment.CreatedBy == userId;

            if (!canDelete)
            {
                throw new UnauthorizedAccessException("Only Managers, Owners, or the uploader can delete attachments");
            }

            await _fileStorage.DeleteFileAsync(attachment.FilePath);

            attachment.IsDeleted = true;
            attachment.DeletedAt = DateTime.UtcNow;
            attachment.DeletedBy = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Attachment {AttachmentId} deleted by user {UserId}", attachmentId, userId);

        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("File is empty");
            }

            var maxSizeBytes = _settings.MaxFileSizeInMB * 1024 * 1024;
            if (file.Length > maxSizeBytes)
            {
                throw new InvalidOperationException($"File size exceeds maximum allowed size of {_settings.MaxFileSizeInMB}MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_settings.AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _settings.AllowedExtensions)}");
            }
        }







    }
}
