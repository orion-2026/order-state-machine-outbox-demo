using OrderStateMachineOutboxDemo.Infrastructure;
using OrderStateMachineOutboxDemo.Models;

namespace OrderStateMachineOutboxDemo.Services;

public class OrderApplicationService
{
    private readonly InMemoryOrderStore _store;
    private readonly OrderStateMachine _stateMachine;

    public OrderApplicationService(InMemoryOrderStore store, OrderStateMachine stateMachine)
    {
        _store = store;
        _stateMachine = stateMachine;
    }

    public Order Create(CreateOrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            ProductSku = request.ProductSku,
            Quantity = request.Quantity,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        _store.Orders[order.Id] = order;
        AppendOutbox(order, "OrderCreated", new
        {
            order.Id,
            order.CustomerId,
            order.ProductSku,
            order.Quantity,
            Status = order.Status.ToString()
        });

        return order;
    }

    public IReadOnlyCollection<Order> ListOrders() => _store.Orders.Values.OrderBy(x => x.CreatedAtUtc).ToList();

    public Order? Get(Guid id)
    {
        return _store.Orders.TryGetValue(id, out var order) ? order : null;
    }

    public IReadOnlyList<string>? AllowedActions(Guid id)
    {
        var order = Get(id);
        return order is null ? null : _stateMachine.AllowedActions(order.Status);
    }

    public (bool Success, string? Error, Order? Order) ChangeStatus(Guid id, ChangeStatusRequest request)
    {
        if (!_store.Orders.TryGetValue(id, out var order))
        {
            return (false, "Order not found.", null);
        }

        if (!_stateMachine.TryTransition(order.Status, request.Action, out var nextStatus))
        {
            var allowed = string.Join(", ", _stateMachine.AllowedActions(order.Status));
            return (false, $"Invalid transition from {order.Status} using action '{request.Action}'. Allowed actions: {allowed}", null);
        }

        var previousStatus = order.Status;
        order.Status = nextStatus;
        order.Version += 1;
        order.UpdatedAtUtc = DateTimeOffset.UtcNow;

        AppendOutbox(order, "OrderStatusChanged", new
        {
            order.Id,
            PreviousStatus = previousStatus.ToString(),
            NewStatus = nextStatus.ToString(),
            Action = request.Action,
            request.Reason,
            order.Version
        });

        return (true, null, order);
    }

    public IReadOnlyCollection<OutboxEvent> ListOutbox(bool publishedOnly = false)
    {
        var events = _store.Outbox.ToArray().OrderBy(x => x.OccurredAtUtc);
        return publishedOnly ? events.Where(x => x.Published).ToList() : events.ToList();
    }

    public IReadOnlyCollection<OutboxEvent> PublishPendingOutbox()
    {
        var events = _store.Outbox.ToArray()
            .Where(x => !x.Published)
            .OrderBy(x => x.OccurredAtUtc)
            .ToList();

        foreach (var evt in events)
        {
            evt.Published = true;
            evt.PublishedAtUtc = DateTimeOffset.UtcNow;
        }

        return events;
    }

    private void AppendOutbox(Order order, string eventType, object payload)
    {
        _store.Outbox.Enqueue(new OutboxEvent
        {
            OrderId = order.Id,
            EventType = eventType,
            Payload = payload
        });
    }
}
