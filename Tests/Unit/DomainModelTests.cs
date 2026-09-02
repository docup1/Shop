using Domain.Models;
using Domain.Types;

namespace Tests.Unit;

public class DomainModelTests
{
    private static Order NewOrder() => Order.Create(Guid.NewGuid(), "СПб", "Москва", "a", "b", 5);

    [Fact]
    public void Order_FollowsForwardTransitionChain()
    {
        var order = NewOrder();

        order.ChangeStatus(Status.InProgress);
        order.ChangeStatus(Status.PickedUp);
        order.ChangeStatus(Status.InTransit);
        order.ChangeStatus(Status.OutForDelivery);
        order.ChangeStatus(Status.Delivered);

        Assert.Equal(Status.Delivered, order.Status);
    }

    [Theory]
    [InlineData(Status.InProgress)]
    [InlineData(Status.PickedUp)]
    [InlineData(Status.InTransit)]
    [InlineData(Status.OutForDelivery)]
    [InlineData(Status.Delivered)]
    public void Order_CanBeCancelled_FromAnyProgressStatus(Status status)
    {
        var order = NewOrder();
        WalkForward(order, status);

        order.ChangeStatus(Status.Cancelled);

        Assert.Equal(Status.Cancelled, order.Status);
    }

    [Fact]
    public void Order_CanBeCancelled_FromNew()
    {
        var order = NewOrder();

        order.ChangeStatus(Status.Cancelled);

        Assert.Equal(Status.Cancelled, order.Status);
    }

    private static void WalkForward(Order order, Status target)
    {
        var chain = new[] { Status.New, Status.InProgress, Status.PickedUp, Status.InTransit, Status.OutForDelivery, Status.Delivered };
        for (var i = 0; i < chain.Length && chain[i] != target; i++)
            order.ChangeStatus(chain[i + 1]);
    }

    [Theory]
    [InlineData(Status.Delivered)]
    [InlineData(Status.PickedUp)]
    public void Order_InvalidJump_Throws(Status jump)
    {
        var order = NewOrder();

        Assert.Throws<InvalidOperationException>(() => order.ChangeStatus(jump));
        Assert.Equal(Status.New, order.Status);
    }

    [Fact]
    public void Order_SettingSameStatus_IsNoop()
    {
        var order = NewOrder();

        order.ChangeStatus(Status.New);

        Assert.Equal(Status.New, order.Status);
    }

    [Fact]
    public void Order_AfterDelivery_OnlyCancellationIsAllowed()
    {
        var delivered = NewOrder();
        WalkForward(delivered, Status.Delivered);

        Assert.Throws<InvalidOperationException>(() => delivered.ChangeStatus(Status.New));

        delivered.ChangeStatus(Status.Cancelled);
        Assert.Equal(Status.Cancelled, delivered.Status);
    }

    [Fact]
    public void Order_Cancelled_IsTerminal()
    {
        var cancelled = NewOrder();
        cancelled.ChangeStatus(Status.Cancelled);

        Assert.Throws<InvalidOperationException>(() => cancelled.ChangeStatus(Status.InProgress));
        Assert.Throws<InvalidOperationException>(() => cancelled.ChangeStatus(Status.New));
    }

    [Fact]
    public void User_SetAdmin_ReturnsNewInstance_PreservingFields()
    {
        var user = User.Create("alice", "hash", isAdmin: false);

        var admin = user.SetAdmin(isAdmin: true);

        Assert.NotSame(user, admin);
        Assert.Equal(user.Id, admin.Id);
        Assert.Equal(user.UserName, admin.UserName);
        Assert.Equal(user.PasswordHash, admin.PasswordHash);
        Assert.True(admin.IsAdmin);
        Assert.False(user.IsAdmin);
    }
}