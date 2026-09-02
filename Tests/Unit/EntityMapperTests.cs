using Domain.Models;
using Infrastructure.DataBase.Mapping;
using Entities = Infrastructure.DataBase.Entities;

namespace Tests.Unit;

public class EntityMapperTests
{
    [Fact]
    public void User_RoundTrips()
    {
        var domain = User.Create("alice", "hash", isAdmin: true);

        var domain2 = EntityMapper.ToDomain(EntityMapper.ToEntity(domain));

        Assert.Equal(domain.Id, domain2.Id);
        Assert.Equal(domain.UserName, domain2.UserName);
        Assert.Equal(domain.PasswordHash, domain2.PasswordHash);
        Assert.Equal(domain.IsAdmin, domain2.IsAdmin);
    }

    [Fact]
    public void Order_RoundTrips()
    {
        var domain = Order.Create(Guid.NewGuid(), "СПб", "Москва", "ул. Ленина, 1", "ул. Гагарина, 2", 12);

        var domain2 = EntityMapper.ToDomain(EntityMapper.ToEntity(domain));

        Assert.Equal(domain.Id, domain2.Id);
        Assert.Equal(domain.UserId, domain2.UserId);
        Assert.Equal(domain.SenderCity, domain2.SenderCity);
        Assert.Equal(domain.RecipientCity, domain2.RecipientCity);
        Assert.Equal(domain.SenderAddress, domain2.SenderAddress);
        Assert.Equal(domain.RecipientAddress, domain2.RecipientAddress);
        Assert.Equal(domain.Weight, domain2.Weight);
        Assert.Equal(domain.Status, domain2.Status);
        Assert.Equal(domain.CreatedAt, domain2.CreatedAt);
    }

    [Fact]
    public void Order_ToDomain_EmptyString_Throws()
    {
        var entity = new Entities.Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SenderCity = "  ",
            RecipientCity = "Москва",
            SenderAddress = "addr",
            RecipientAddress = "addr2",
            Weight = 5,
            Status = Domain.Types.Status.New,
            CreatedAt = DateTime.UtcNow
        };

        Assert.Throws<ArgumentException>(() => EntityMapper.ToDomain(entity));
    }

    [Fact]
    public void Session_RoundTrips()
    {
        var domain = Session.Restore(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(1), DateTime.UtcNow);

        var domain2 = EntityMapper.ToDomain(EntityMapper.ToEntity(domain));

        Assert.Equal(domain.Id, domain2.Id);
        Assert.Equal(domain.UserId, domain2.UserId);
        Assert.Equal(domain.ExpiresAt, domain2.ExpiresAt);
        Assert.Equal(domain.CreatedAt, domain2.CreatedAt);
    }

    [Fact]
    public void ToDomain_NullUser_Throws()
        => Assert.Throws<ArgumentNullException>(() => EntityMapper.ToDomain((Entities.User)null!));

    [Fact]
    public void ToDomain_NullOrder_Throws()
        => Assert.Throws<ArgumentNullException>(() => EntityMapper.ToDomain((Entities.Order)null!));

    [Fact]
    public void ToDomain_NullSession_Throws()
        => Assert.Throws<ArgumentNullException>(() => EntityMapper.ToDomain((Entities.Session)null!));

    [Fact]
    public void ToEntity_NullUser_Throws()
        => Assert.Throws<ArgumentNullException>(() => EntityMapper.ToEntity((User)null!));

    [Fact]
    public void ToEntity_NullOrder_Throws()
        => Assert.Throws<ArgumentNullException>(() => EntityMapper.ToEntity((Order)null!));

    [Fact]
    public void ToEntity_NullSession_Throws()
        => Assert.Throws<ArgumentNullException>(() => EntityMapper.ToEntity((Session)null!));
}