using OrderService.Domain;

namespace OrderService.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
}