using Domain.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Cache;

/// <summary>
/// In-memory кеш поверх IMemoryCache приложения. Подходит для данных, которые
/// выгодно держать "под рукой" и можно потерять без последствий (кэш не является
/// источником истины).
/// </summary>
public class InMemoryCache : ICache
{
    private readonly IMemoryCache _cache;

    public InMemoryCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T? Get<T>(string key)
        => _cache.TryGetValue(key, out T? value) ? value : default;

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (ttl is null)
            _cache.Set(key, value);
        else
            _cache.Set(key, value, ttl.Value);
    }

    public bool Remove(string key)
    {
        _cache.Remove(key);
        return true;
    }
}