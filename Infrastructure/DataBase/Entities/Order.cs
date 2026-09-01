namespace Infrastructure.DataBase.Entities;

public class Order
{
    public Guid Id{ get; set; }
    public Guid UserId{ get; set; }
    public string SenderCity { get; set; }
    public string RecipientCity { get; set; }
    public string SenderAddress { get; set; }
    public string RecipientAddress { get; set; }
    public int Weight { get; set; }
    public DateTime CreatedAt { get; set; }
}