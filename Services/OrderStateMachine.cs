using OrderStateMachineOutboxDemo.Domain;

namespace OrderStateMachineOutboxDemo.Services;

public class OrderStateMachine
{
    private static readonly Dictionary<(OrderStatus Status, string Action), OrderStatus> Transitions = new()
    {
        [(OrderStatus.PendingPayment, "pay")] = OrderStatus.Paid,
        [(OrderStatus.PendingPayment, "cancel")] = OrderStatus.Cancelled,
        [(OrderStatus.Paid, "start-fulfillment")] = OrderStatus.Fulfilling,
        [(OrderStatus.Paid, "refund")] = OrderStatus.Refunded,
        [(OrderStatus.Fulfilling, "ship")] = OrderStatus.Shipped,
        [(OrderStatus.Fulfilling, "cancel")] = OrderStatus.Cancelled,
        [(OrderStatus.Shipped, "complete")] = OrderStatus.Completed,
        [(OrderStatus.Shipped, "refund")] = OrderStatus.Refunded
    };

    public bool TryTransition(OrderStatus current, string action, out OrderStatus next)
    {
        return Transitions.TryGetValue((current, action.Trim().ToLowerInvariant()), out next);
    }

    public IReadOnlyList<string> AllowedActions(OrderStatus current)
    {
        return Transitions
            .Where(x => x.Key.Status == current)
            .Select(x => x.Key.Action)
            .OrderBy(x => x)
            .ToList();
    }
}
