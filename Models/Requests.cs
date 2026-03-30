namespace OrderStateMachineOutboxDemo.Models;

public record CreateOrderRequest(string CustomerId, string ProductSku, int Quantity);
public record ChangeStatusRequest(string Action, string? Reason);
