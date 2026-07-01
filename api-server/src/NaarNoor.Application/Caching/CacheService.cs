using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace NaarNoor.Application.Caching;

/// <summary>
/// Generic caching service for distributed cache operations
/// Supports both Redis and memory cache fallback
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DistributedCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(cached))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(cached, _jsonOptions);
        }
        catch
        {
            // If deserialization fails, remove corrupted cache entry
            await _cache.RemoveAsync(key, cancellationToken);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        var options = new DistributedCacheEntryOptions();

        if (expiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        else
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1); // Default: 1 hour

        await _cache.SetStringAsync(key, json, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }
}

/// <summary>
/// Cache key constants to avoid hardcoding and typos
/// </summary>
public static class CacheKeys
{
    // Menu Items (5-minute TTL)
    public const string MenuItems = "menu_items";
    public const string MenuItemsByCategory = "menu_items_category_{0}";
    private const int MenuItemsCacheDuration = 300;

    // Reservations (1-minute TTL)
    public const string ReservationSlots = "reservation_slots_{0}";
    private const int ReservationSlotsCacheDuration = 60;

    // Chefs (10-minute TTL)
    public const string Chefs = "chefs";
    public const string ChefById = "chef_{0}";
    private const int ChefsCacheDuration = 600;

    // Reviews (5-minute TTL)
    public const string ApprovedReviews = "reviews_approved";
    private const int ReviewsCacheDuration = 300;

    /// <summary>
    /// Get cache expiration duration based on cache type
    /// </summary>
    public static TimeSpan GetExpiration(string cacheKeyPrefix) => cacheKeyPrefix switch
    {
        _ when cacheKeyPrefix.StartsWith("menu_items") => TimeSpan.FromSeconds(MenuItemsCacheDuration),
        _ when cacheKeyPrefix.StartsWith("reservation_slots") => TimeSpan.FromSeconds(ReservationSlotsCacheDuration),
        _ when cacheKeyPrefix.StartsWith("chef") => TimeSpan.FromSeconds(ChefsCacheDuration),
        _ when cacheKeyPrefix.StartsWith("reviews") => TimeSpan.FromSeconds(ReviewsCacheDuration),
        _ => TimeSpan.FromHours(1) // Default
    };
}
