using Microsoft.AspNetCore.Mvc;
using OrderService.Domain;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var result = await orderService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) => Ok(new { Message = $"Get order {id} (placeholder)" });
}