namespace Domain.Contracts;

/// <summary>
/// In-memory кеш с generic API. Идеален для быстрых повторных доступов
/// (валидированные сессии, горячие объекты). Не является источником истины —
/// при промахе данные берутся из БД.
/// </summary>
public interface ICache
{
    T? Get<T>(string key);

    /// <summary>Пустой ttl = без срока действия (действует до Remove).</summary>
    void Set<T>(string key, T value, TimeSpan? ttl = null);

    bool Remove(string key);
}