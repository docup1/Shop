namespace Domain.Models;

public class Order
{
    public Guid Id{ get; }
    public string SenderCity { get; }
    public string RecipientCity { get; }
    public string SenderAddres { get; }
    public string RecipientAddres { get; }
    public int Weight { get; }
    
    public DateTime CreatedAt { get; }
    
    private Order(string senderCity, string recipientCity, string senderAddres, string recipientAddres, int weight)
    {
        Id = Guid.CreateVersion7();
        SenderCity = senderCity;
        RecipientCity = recipientCity;
        SenderAddres = senderAddres;
        RecipientAddres = recipientAddres;
        Weight = weight;
        CreatedAt = DateTime.UtcNow;
    }
    
    public static Order Create(string senderCity, string recipientCity, string senderAddres, string recipientAddres, int weight)
    {
        if (string.IsNullOrWhiteSpace(senderCity)) throw new ArgumentException("Sender city cannot be empty");
        if (string.IsNullOrWhiteSpace(recipientCity)) throw new ArgumentException("Recipient city cannot be empty");
        if (string.IsNullOrWhiteSpace(senderAddres)) throw new ArgumentException("Sender address cannot be empty");
        if (string.IsNullOrWhiteSpace(recipientAddres)) throw new ArgumentException("Recipient address cannot be empty");
        if (weight <= 0) throw new ArgumentException("Weight must be greater than zero");
        
        return new Order(senderCity, recipientCity, senderAddres, recipientAddres, weight);
    }
}