namespace OrderService.Domain;

public record CreateOrderRequest(string ProductId, int Quantity);
public record OrderResponse(Guid Id, string ProductId, int Quantity, string Status, DateTime CreatedAtUtc);