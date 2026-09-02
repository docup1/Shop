using Domain.Contracts;
using Infrastructure.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace Tests.Unit;

public class InMemoryCacheTests
{
    private static ICache CreateCache()
        => new InMemoryCache(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public void Get_MissingKey_ReturnsDefault()
    {
        var cache = CreateCache();

        Assert.Null(cache.Get<string>("missing"));
    }

    [Fact]
    public void Set_Then_Get_ReturnsValue()
    {
        var cache = CreateCache();

        cache.Set("key", "value");

        Assert.Equal("value", cache.Get<string>("key"));
    }

    [Fact]
    public void Set_WithTtl_Then_Get_ReturnsValue()
    {
        var cache = CreateCache();

        cache.Set("key", 42, TimeSpan.FromMinutes(5));

        Assert.Equal(42, cache.Get<int>("key"));
    }

    [Fact]
    public void Get_WrongType_ReturnsDefault()
    {
        var cache = CreateCache();

        cache.Set("key", "value");

        Assert.Equal(0, cache.Get<int>("key"));
    }

    [Fact]
    public void Remove_ReturnsTrue_AndClearsValue()
    {
        var cache = CreateCache();

        cache.Set("key", "value");

        Assert.True(cache.Remove("key"));
        Assert.Null(cache.Get<string>("key"));
    }

    [Fact]
    public async Task Entry_ExpiresAfterTtl()
    {
        var cache = CreateCache();

        cache.Set("key", "value", TimeSpan.FromMilliseconds(50));
        Assert.Equal("value", cache.Get<string>("key"));

        await Task.Delay(150);

        Assert.Null(cache.Get<string>("key"));
    }

    [Fact]
    public void Entry_WithoutTtl_DoesNotExpire()
    {
        var cache = CreateCache();

        cache.Set("key", "value");

        Assert.Equal("value", cache.Get<string>("key"));
    }
}