using System.Text.Json;
using OrderService.Domain;
using OrderService.Repositories;
using Shared.Events;

namespace OrderService.Services;

public class OrderService(IOrderRepository repository) : IOrderService
{
    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        var orderCreatedEvent = new OrderCreatedEvent(order.Id, order.ProductId, order.Quantity, order.CreatedAtUtc);

        var outbox = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(OrderCreatedEvent),
            Payload = JsonSerializer.Serialize(orderCreatedEvent),
            OccurredOnUtc = DateTime.UtcNow
        };

        await repository.AddAsync(order, ct);
        await repository.AddOutboxMessageAsync(outbox, ct);
        await repository.SaveChangesAsync(ct);

        return new OrderResponse(order.Id, order.ProductId, order.Quantity, order.Status, order.CreatedAtUtc);
    }
}