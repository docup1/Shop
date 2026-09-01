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
    public Status Status { get; private init; }
    public DateTime CreatedAt { get; private init; }

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
}
