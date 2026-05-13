using Microsoft.AspNetCore.Mvc;
using OrderService.Domain;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(userIdHeader))
            return Unauthorized("Missing X-User-Id header");

        var userId = Guid.Parse(userIdHeader);
        var email = Request.Headers["X-User-Email"].FirstOrDefault()!;

        var result = await orderService.CreateAsync(request, userId, email, ct);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) => Ok(new { Message = $"Get order {id} (placeholder)" });
}