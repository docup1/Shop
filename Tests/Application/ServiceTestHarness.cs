using Application.Services;
using Domain.Models;

namespace Tests.Application;

/// <summary>
/// Собирает in-memory фейки-зависимости и инстанцирует сервисы Application.
/// Тесты могут сидировать данные через открытые коллекции или хелперы.
/// </summary>
internal sealed class ServiceTestHarness
{
    public ServiceTestHarness(DateTimeOffset? now = null)
    {
        Clock = new Fakes.FakeTimeProvider(now);
        Sessions = new Fakes.InMemorySessionRepository(Clock);
    }

    public Fakes.InMemoryUserRepository Users { get; } = new();
    public Fakes.InMemorySessionRepository Sessions { get; }
    public Fakes.InMemoryOrderRepository Orders { get; } = new();
    public Fakes.FakeUnitOfWork Uow { get; } = new();
    public Fakes.FakePasswordHasher PasswordHasher { get; } = new();
    public Fakes.FakeTokenService Tokens { get; } = new();
    public Fakes.FakeTimeProvider Clock { get; }

    public AuthService Auth(TimeSpan? sessionLifetime = null)
        => new(Users, Sessions, PasswordHasher, Tokens, Uow, sessionLifetime ?? TimeSpan.FromDays(30));

    public OrderService OrderService() => new(Users, Orders, Uow);

    public UserService UserService() => new(Users, Uow);

    public User AddUser(string userName = "alice", bool isAdmin = false)
    {
        var user = User.Create(userName, "hash-" + userName, isAdmin);
        Users.Items.Add(user);
        return user;
    }

    public Order AddOrder(Guid userId, Domain.Types.Status status = Domain.Types.Status.New)
    {
        var order = Order.Create(userId, "СПб", "Москва", "ул. Ленина, 1", "ул. Гагарина, 2", 5);

        var path = Chain;
        var targetIndex = Array.IndexOf(path, status);
        for (var i = 1; i <= targetIndex; i++)
            order.ChangeStatus(path[i]);

        Orders.Items.Add(order);
        return order;
    }

    public Session AddSession(Guid userId, DateTime expiresAt)
    {
        var session = Session.Restore(Guid.CreateVersion7(), userId, expiresAt, Clock.Now.UtcDateTime);
        Sessions.Items.Add(session);
        return session;
    }

    private static readonly Domain.Types.Status[] Chain =
    [
        Domain.Types.Status.New,
        Domain.Types.Status.InProgress,
        Domain.Types.Status.PickedUp,
        Domain.Types.Status.InTransit,
        Domain.Types.Status.OutForDelivery,
        Domain.Types.Status.Delivered
    ];
}