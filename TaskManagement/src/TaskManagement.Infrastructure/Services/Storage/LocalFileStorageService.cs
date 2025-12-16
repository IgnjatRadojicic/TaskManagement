using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManagement.Core.Configuration;
using TaskManagement.Core.Interfaces;
using System.IO;
namespace TaskManagement.Infrastructure.Services.Storage
{
    public class LocalFileStorageService
    {
        private readonly FileStorageSettings _settings;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(
            IOptions<FileStorageSettings> settings,
            ILogger<LocalFileStorageService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            if (!Directory.Exists(_settings.LocalStorage.BasePath))
            {
                Directory.CreateDirectory(_settings.LocalStorage.BasePath);
                _logger.LogInformation("Created base directory: {Path}", _settings.LocalStorage.BasePath);
            }
        }
    }


}

