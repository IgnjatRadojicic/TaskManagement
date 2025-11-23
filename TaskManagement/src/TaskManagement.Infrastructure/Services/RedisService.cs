using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Models;

namespace TaskManagement.Infrastructure.Services
{
    public class RedisService : IRedisService
    {

        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
        {
            _redis = redis;
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists in Redis {Key}", key);
                throw;
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var json = await _db.StringGetAsync(key);

                if (json.IsNullOrEmpty)
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(json!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from Redis for key {Key}", key);
                throw;
            }
        }
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _db.StringSetAsync(key, json, expiration, When.NotExists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in Redis for key {Key}", key);
                throw;
            }
        }

        public async Task RemoveAsync(string key)
        {
           try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error removing key from Redis {Key}", key);
                throw;
            }
        }

        public async Task<RefreshTokenModel?> GetRefreshTokenAsync(string token)
        {
            try
            {
                var key = GetRefreshTokenKey(token);
                var json = await _db.StringGetAsync(key);

                if (json.IsNullOrEmpty)
                {
                    return null;
                }

                return JsonSerializer.Deserialize<RefreshTokenModel>(json!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refresh token from Redis");
                throw;
            }
        }

        public async Task SetRefreshTokenAsync(string token, RefreshTokenModel model, TimeSpan expiration)
        {
            try
            {
                var key = GetRefreshTokenKey(token);
                var json = JsonSerializer.Serialize(model);
                await _db.StringSetAsync(key, json, expiration);

                var userTokensKey = GetUserTokensKey(model.UserId);
                await _db.SetAddAsync(userTokensKey, token);
                _logger.LogInformation("Refresh token stored in Redis for user {UserId}", model.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing refresh token in Redis");
                throw;
            }
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            try
            {
                var userTokensKey = GetUserTokensKey(userId);

                var tokens = await _db.SetMembersAsync(userTokensKey);

                foreach(var token in tokens)
                {
                    var key = GetRefreshTokenKey(token!);
                    await _db.KeyDeleteAsync(key);
                }

                await _db.KeyDeleteAsync(userTokensKey);
                _logger.LogInformation("Refresh token revoked");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                throw;
            }
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            try
            {
                var key = GetRefreshTokenKey(token);

                var model = await GetRefreshTokenAsync(token);

                await _db.KeyDeleteAsync(key);

                if (model != null)
                {
                    var userTokensKey = GetUserTokensKey(model.UserId);
                    await _db.SetRemoveAsync(userTokensKey, token);
                }

                _logger.LogInformation("Refresh token revoked");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                throw;
            }
        }



        public static string GetRefreshTokenKey(string token) => $"refresh_token:{token}";
        public static string GetUserTokensKey(Guid userId) => $"user_tokens:{userId}";
    }
}
