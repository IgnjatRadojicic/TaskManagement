using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Models;

namespace TaskManagement.Core.Interfaces
{
    public interface IRedisService
    {

        Task SetRefreshTokenAsync(string token, RefreshTokenModel model, TimeSpan expiration);


        Task<RefreshTokenModel?> GetRefreshTokenAsync(string token);


        Task RevokeRefreshTokenAsync(string token);

        Task RevokeAllUserTokensAsync(Guid userId);


        // Generic cache operations

        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}
