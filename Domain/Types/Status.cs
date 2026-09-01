namespace Domain.Types;

public enum Status
{
    New = 0,
    InProgress = 1,
    PickedUp = 2,
    InTransit = 3,
    OutForDelivery = 4,
    Delivered = 5,
    Cancelled = 6
}
