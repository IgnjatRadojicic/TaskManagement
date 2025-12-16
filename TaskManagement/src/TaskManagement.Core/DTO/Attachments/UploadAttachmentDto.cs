using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TaskManagement.Core.DTO.Attachments
{
    public class UploadAttachmentDto
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
