namespace OrderStateMachineOutboxDemo.Models;

public class OutboxEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset OccurredAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public bool Published { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }
}
