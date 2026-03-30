using System.Collections.Concurrent;
using OrderStateMachineOutboxDemo.Models;

namespace OrderStateMachineOutboxDemo.Infrastructure;

public class InMemoryOrderStore
{
    public ConcurrentDictionary<Guid, Order> Orders { get; } = new();
    public ConcurrentQueue<OutboxEvent> Outbox { get; } = new();
}
