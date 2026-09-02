namespace Tests.Fakes;

/// <summary>
/// TimeProvider с управляемым временем: <see cref="Now"/> можно сдвигать,
/// чтобы детерминированно проверять истечение кеша/токенов без реальных задержек.
/// </summary>
internal sealed class FakeTimeProvider : TimeProvider
{
    public FakeTimeProvider(DateTimeOffset? now = null)
    {
        Now = now ?? DateTimeOffset.UtcNow;
    }

    public DateTimeOffset Now { get; set; }

    public override DateTimeOffset GetUtcNow() => Now;

    public override long GetTimestamp() => Now.UtcTicks;
}