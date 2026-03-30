using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderStateMachineOutboxDemo.Infrastructure;
using OrderStateMachineOutboxDemo.Models;

namespace OrderStateMachineOutboxDemo.Services;

public class OrderApplicationService
{
    private readonly AppDbContext _db;
    private readonly OrderStateMachine _stateMachine;

    public OrderApplicationService(AppDbContext db, OrderStateMachine stateMachine)
    {
        _db = db;
        _stateMachine = stateMachine;
    }

    public async Task<Order> CreateAsync(CreateOrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            ProductSku = request.ProductSku,
            Quantity = request.Quantity,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.Orders.Add(order);
        AppendOutbox(order, "OrderCreated", new
        {
            order.Id,
            order.CustomerId,
            order.ProductSku,
            order.Quantity,
            Status = order.Status.ToString(),
            order.Version
        });

        await _db.SaveChangesAsync();
        return order;
    }

    public async Task<IReadOnlyCollection<Order>> ListOrdersAsync()
    {
        var orders = await _db.Orders.ToListAsync();
        return orders.OrderBy(x => x.CreatedAtUtc).ToList();
    }

    public Task<Order?> GetAsync(Guid id)
    {
        return _db.Orders.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<string>?> AllowedActionsAsync(Guid id)
    {
        var order = await GetAsync(id);
        return order is null ? null : _stateMachine.AllowedActions(order.Status);
    }

    public async Task<(bool Success, string? Error, Order? Order)> ChangeStatusAsync(Guid id, ChangeStatusRequest request)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (order is null)
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

        await _db.SaveChangesAsync();
        return (true, null, order);
    }

    public async Task<IReadOnlyCollection<OutboxEvent>> ListOutboxAsync(bool publishedOnly = false)
    {
        var query = _db.OutboxEvents.AsQueryable();
        if (publishedOnly)
        {
            query = query.Where(x => x.Published);
        }

        var events = await query.ToListAsync();
        return events.OrderBy(x => x.OccurredAtUtc).ToList();
    }

    public async Task<IReadOnlyCollection<OutboxEvent>> PublishPendingOutboxAsync()
    {
        var events = (await _db.OutboxEvents
            .Where(x => !x.Published)
            .ToListAsync())
            .OrderBy(x => x.OccurredAtUtc)
            .ToList();

        foreach (var evt in events)
        {
            evt.Published = true;
            evt.PublishedAtUtc = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync();
        return events;
    }

    private void AppendOutbox(Order order, string eventType, object payload)
    {
        _db.OutboxEvents.Add(new OutboxEvent
        {
            OrderId = order.Id,
            EventType = eventType,
            PayloadJson = JsonSerializer.Serialize(payload)
        });
    }
}
