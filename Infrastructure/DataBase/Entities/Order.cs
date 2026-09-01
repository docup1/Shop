using Domain.Types;

namespace Infrastructure.DataBase.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SenderCity { get; set; } = string.Empty;
    public string RecipientCity { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
    public string RecipientAddress { get; set; } = string.Empty;
    public int Weight { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
