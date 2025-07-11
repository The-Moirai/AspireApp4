using WebApplication1.Services.Interfaces;
using System.Collections.Concurrent;

namespace WebApplication1.Services
{
    /// <summary>
    /// 内存缓存服务实现
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
        private readonly ILogger<CacheService> _logger;

        public CacheService(ILogger<CacheService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var item = new CacheItem
                {
                    Value = value,
                    ExpiryTime = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : null
                };
                _cache.AddOrUpdate(key, item, (k, v) => item);
                _logger.LogDebug("设置缓存: {Key}", key);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置缓存失败: {Key}", key);
                return await Task.FromResult(false);
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (_cache.TryGetValue(key, out var item))
                {
                    if (item.ExpiryTime.HasValue && DateTime.UtcNow > item.ExpiryTime.Value)
                    {
                        _cache.TryRemove(key, out _);
                        _logger.LogDebug("缓存已过期: {Key}", key);
                        return await Task.FromResult<T?>(default);
                    }

                    _logger.LogDebug("命中缓存: {Key}", key);
                    return await Task.FromResult((T?)item.Value);
                }

                _logger.LogDebug("缓存未命中: {Key}", key);
                return await Task.FromResult<T?>(default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓存失败: {Key}", key);
                return await Task.FromResult<T?>(default);
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                var removed = _cache.TryRemove(key, out _);
                _logger.LogDebug("删除缓存: {Key}, 结果: {Removed}", key, removed);
                return await Task.FromResult(removed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除缓存失败: {Key}", key);
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var exists = _cache.TryGetValue(key, out var item);
                if (exists && item.ExpiryTime.HasValue && DateTime.UtcNow > item.ExpiryTime.Value)
                {
                    _cache.TryRemove(key, out _);
                    exists = false;
                }
                return await Task.FromResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查缓存存在失败: {Key}", key);
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> SetExpiryAsync(string key, TimeSpan expiry)
        {
            try
            {
                if (_cache.TryGetValue(key, out var item))
                {
                    item.ExpiryTime = DateTime.UtcNow.Add(expiry);
                    return await Task.FromResult(true);
                }
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置缓存过期时间失败: {Key}", key);
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> SetMultipleAsync<T>(Dictionary<string, T> keyValues, TimeSpan? expiry = null)
        {
            try
            {
                foreach (var kvp in keyValues)
                {
                    await SetAsync(kvp.Key, kvp.Value, expiry);
                }
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量设置缓存失败");
                return await Task.FromResult(false);
            }
        }

        public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys)
        {
            try
            {
                var result = new Dictionary<string, T?>();
                foreach (var key in keys)
                {
                    result[key] = await GetAsync<T>(key);
                }
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量获取缓存失败");
                return await Task.FromResult(new Dictionary<string, T?>());
            }
        }

        public async Task<long> RemoveMultipleAsync(IEnumerable<string> keys)
        {
            try
            {
                long count = 0;
                foreach (var key in keys)
                {
                    if (await RemoveAsync(key))
                    {
                        count++;
                    }
                }
                return await Task.FromResult(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量删除缓存失败");
                return await Task.FromResult(0L);
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            try
            {
                var value = await GetAsync<T>(key);
                if (value != null)
                {
                    return value;
                }

                value = await factory();
                await SetAsync(key, value, expiry);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取或设置缓存失败: {Key}", key);
                return await factory();
            }
        }

        public async Task<long> IncrementAsync(string key, long value = 1)
        {
            try
            {
                var current = await GetAsync<long?>(key);
                var newValue = (current ?? 0L) + value;
                await SetAsync(key, newValue);
                return await Task.FromResult(newValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "递增缓存失败: {Key}", key);
                return await Task.FromResult(0L);
            }
        }

        public async Task<long> DecrementAsync(string key, long value = 1)
        {
            try
            {
                var current = await GetAsync<long?>(key);
                var newValue = (current ?? 0L) - value;
                await SetAsync(key, newValue);
                return await Task.FromResult(newValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "递减缓存失败: {Key}", key);
                return await Task.FromResult(0L);
            }
        }

        public async Task<bool> SetHashAsync<T>(string key, string field, T value)
        {
            try
            {
                var hashKey = $"{key}:hash";
                var hash = await GetAsync<Dictionary<string, T>>(hashKey) ?? new Dictionary<string, T>();
                hash[field] = value;
                await SetAsync(hashKey, hash);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置哈希表字段失败: {Key}:{Field}", key, field);
                return await Task.FromResult(false);
            }
        }

        public async Task<T?> GetHashAsync<T>(string key, string field)
        {
            try
            {
                var hashKey = $"{key}:hash";
                var hash = await GetAsync<Dictionary<string, T>>(hashKey);
                if (hash != null && hash.TryGetValue(field, out var value))
                {
                    return await Task.FromResult(value);
                }
                return await Task.FromResult<T?>(default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取哈希表字段失败: {Key}:{Field}", key, field);
                return await Task.FromResult<T?>(default);
            }
        }

        public async Task<Dictionary<string, T?>> GetHashAllAsync<T>(string key)
        {
            try
            {
                var hashKey = $"{key}:hash";
                var hash = await GetAsync<Dictionary<string, T>>(hashKey);
                return await Task.FromResult(hash ?? new Dictionary<string, T?>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取哈希表所有字段失败: {Key}", key);
                return await Task.FromResult(new Dictionary<string, T?>());
            }
        }

        public async Task<IEnumerable<string>> GetKeysAsync(string pattern)
        {
            try
            {
                // 简单的模式匹配实现
                var keys = _cache.Keys.Where(k => k.Contains(pattern.Replace("*", ""))).ToList();
                return await Task.FromResult(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据模式获取键失败: {Pattern}", pattern);
                return await Task.FromResult(Enumerable.Empty<string>());
            }
        }

        public async Task<long> RemoveByPatternAsync(string pattern)
        {
            try
            {
                var keys = await GetKeysAsync(pattern);
                return await RemoveMultipleAsync(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据模式删除键失败: {Pattern}", pattern);
                return await Task.FromResult(0L);
            }
        }

        public async Task<CacheStatistics> GetStatisticsAsync()
        {
            try
            {
                var stats = new CacheStatistics
                {
                    TotalKeys = _cache.Count,
                    MemoryUsage = _cache.Count * 100, // 简单估算
                    LastResetTime = DateTime.UtcNow
                };
                return await Task.FromResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓存统计信息失败");
                return await Task.FromResult(new CacheStatistics());
            }
        }

        public async Task<bool> ClearAllAsync()
        {
            try
            {
                _cache.Clear();
                _logger.LogInformation("清空所有缓存");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空所有缓存失败");
                return await Task.FromResult(false);
            }
        }

        private class CacheItem
        {
            public object? Value { get; set; }
            public DateTime? ExpiryTime { get; set; }
        }
    }
} 