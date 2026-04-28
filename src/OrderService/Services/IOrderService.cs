using OrderService.Domain;

namespace OrderService.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        Guid userId,
        string userEmail,
        CancellationToken ct = default);
}