using Domain.Types;

namespace Domain.Models;

public class Order
{
    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public string SenderCity { get; private init; } = string.Empty;
    public string RecipientCity { get; private init; } = string.Empty;
    public string SenderAddress { get; private init; } = string.Empty;
    public string RecipientAddress { get; private init; } = string.Empty;
    public int Weight { get; private init; }
    public Status Status { get; private set; }
    public DateTime CreatedAt { get; private init; }

    /// <summary>Допустимые переходы статусов. В Cancelled можно уйти из любого статуса.</summary>
    private static readonly IReadOnlyDictionary<Status, IReadOnlySet<Status>> AllowedTransitions =
        new Dictionary<Status, IReadOnlySet<Status>>
        {
            [Status.New] = new HashSet<Status> { Status.InProgress, Status.Cancelled },
            [Status.InProgress] = new HashSet<Status> { Status.PickedUp, Status.Cancelled },
            [Status.PickedUp] = new HashSet<Status> { Status.InTransit, Status.Cancelled },
            [Status.InTransit] = new HashSet<Status> { Status.OutForDelivery, Status.Cancelled },
            [Status.OutForDelivery] = new HashSet<Status> { Status.Delivered, Status.Cancelled },
            [Status.Delivered] = new HashSet<Status> { Status.Cancelled },
            [Status.Cancelled] = new HashSet<Status>()
        };

    private Order(Guid userId, string senderCity, string recipientCity,
        string senderAddress, string recipientAddress, int weight)
    {
        Id = Guid.CreateVersion7();
        UserId = userId;
        SenderCity = senderCity;
        RecipientCity = recipientCity;
        SenderAddress = senderAddress;
        RecipientAddress = recipientAddress;
        Weight = weight;
        Status = Status.New;
        CreatedAt = DateTime.UtcNow;
    }

    private Order(Guid id, Guid userId, string senderCity, string recipientCity,
        string senderAddress, string recipientAddress, int weight, Status status, DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        SenderCity = senderCity;
        RecipientCity = recipientCity;
        SenderAddress = senderAddress;
        RecipientAddress = recipientAddress;
        Weight = weight;
        Status = status;
        CreatedAt = createdAt;
    }

    /// <summary>Восстановление полностью материализованной сущности (для чтения из БД).</summary>
    public static Order Restore(Guid id, Guid userId, string senderCity, string recipientCity,
        string senderAddress, string recipientAddress, int weight, Status status, DateTime createdAt)
        => new(id, userId, senderCity, recipientCity, senderAddress, recipientAddress, weight, status, createdAt);

    public static Order Create(Guid userId, string senderCity, string recipientCity,
        string senderAddress, string recipientAddress, int weight)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty");
        if (string.IsNullOrWhiteSpace(senderCity)) throw new ArgumentException("Sender city cannot be empty");
        if (string.IsNullOrWhiteSpace(recipientCity)) throw new ArgumentException("Recipient city cannot be empty");
        if (string.IsNullOrWhiteSpace(senderAddress)) throw new ArgumentException("Sender address cannot be empty");
        if (string.IsNullOrWhiteSpace(recipientAddress)) throw new ArgumentException("Recipient address cannot be empty");
        if (weight <= 0) throw new ArgumentException("Weight must be greater than zero");

        return new Order(userId, senderCity, recipientCity, senderAddress, recipientAddress, weight);
    }

    /// <summary>
    /// Переводит заказ в новый статус согласно стейт-машине. Установка того же
    /// статуса — идемпотентный no-op. Недопустимый переход бросает
    /// <see cref="InvalidOperationException"/> (Application оборачивает её в ValidationException).
    /// </summary>
    public void ChangeStatus(Status newStatus)
    {
        if (newStatus == Status)
            return;

        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException($"Invalid status transition: {Status} -> {newStatus}.");

        Status = newStatus;
    }
}
