namespace OrderStateMachineOutboxDemo.Domain;

public enum OrderStatus
{
    PendingPayment,
    Paid,
    Fulfilling,
    Shipped,
    Completed,
    Cancelled,
    Refunded
}
