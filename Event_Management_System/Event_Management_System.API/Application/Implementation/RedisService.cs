using Event_Management_System.API.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Event_Management_System.API.Application.Implementation
{
    public class RedisService : IRedisService
    {
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<RedisService> _logger;
        public RedisService(IDistributedCache cache, ILogger<RedisService> logger)
        {
            _cache = cache;
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true,
                AllowTrailingCommas = true
            };
            _logger = logger;
        }
        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(key);
                if (cachedData == null)
                {
                    _logger.LogInformation($"Cache miss for key: {key}");
                    return default;
                }
                _logger.LogInformation($"Cache hit for key: {key}");
                return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cache for key: {key}");
                return default;
            }
        }
        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiration,
                    SlidingExpiration = slidingExpiration
                };
                var jsonData = JsonSerializer.Serialize(value, _jsonOptions);
                await _cache.SetStringAsync(key, jsonData, options);
                _logger.LogInformation($"Cache set for key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting cache for key: {key}");
            }
        }
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogInformation($"Cache removed for key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing cache for key: {key}");
            }
        }
        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            try
            {
                var cachedData = await GetAsync<T>(key);
                if (cachedData != null)
                {
                    return cachedData;
                }
                var value = await valueFactory();
                await SetAsync(key, value, absoluteExpiration, slidingExpiration);

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting or adding cache for key: {key}");
                return default;
            }
        }
    }
}
