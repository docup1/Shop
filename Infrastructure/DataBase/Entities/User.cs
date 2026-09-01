namespace Infrastructure.DataBase.Entities;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }

    public ICollection<Session> Sessions { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}
