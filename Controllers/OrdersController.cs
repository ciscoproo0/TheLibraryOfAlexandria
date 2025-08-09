using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;
using TheLibraryOfAlexandria.Services;


namespace TheLibraryOfAlexandria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/Orders (no pagination; filters preserved)
        [Authorize(Roles = "Customer, Admin, SuperAdmin")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<Order>>>> GetOrders(
            [FromQuery] int? userId,
            [FromQuery] string? status,
            [FromQuery] decimal? minTotalPrice,
            [FromQuery] decimal? maxTotalPrice,
            [FromQuery] DateTime? createdFrom,
            [FromQuery] DateTime? createdTo,
            [FromQuery] DateTime? updatedFrom,
            [FromQuery] DateTime? updatedTo
        )
        {
            var response = await _orderService.GetAllOrdersAsync(userId, status, minTotalPrice, maxTotalPrice, createdFrom, createdTo, updatedFrom, updatedTo);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return Ok(response);
        }

        // GET: api/Orders/5
        [Authorize(Roles = "Customer, Admin, SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOrder(int id)
        {
            var response = await _orderService.GetOrderByIdAsync(id);
            if (response.Success)
            {
                if (response.Data == null)
                {
                    return NotFound("Order not found.");
                }
                // Try to get payment; if not found, return null node
                Payment? payment = null;
                try
                {
                    var payResp = await HttpContext.RequestServices.GetRequiredService<IPaymentService>().GetPaymentByOrderIdAsync(id);
                    if (payResp.Success)
                    {
                        payment = payResp.Data;
                    }
                }
                catch {}

                var order = response.Data;
                // If shipping not present, ensure it goes as null (empty node)
                var shipping = order.ShippingInfo; // may be null

                return Ok(new
                {
                    id = order.Id,
                    userId = order.UserId,
                    status = order.Status,
                    totalPrice = order.TotalPrice,
                    createdAt = order.CreatedAt,
                    updatedAt = order.UpdatedAt,
                    orderItems = order.OrderItems,
                    shippingInfo = shipping,
                    payment = payment
                });
            }
            else
            {
                return BadRequest(response.Message);
            }
        }

        // POST: api/Orders
        [Authorize(Roles = "Customer, Admin, SuperAdmin")]
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            var response = await _orderService.CreateOrderAsync(order);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return CreatedAtAction(nameof(GetOrder), new { id = response.Data.Id }, response.Data);

        }

        // PUT: api/Orders/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> PutOrder(int id, [FromBody][Bind("Status")] Order order)
        {
            if (id != order.Id)
            {
                return BadRequest("Mismatched order ID");
            }

            var response = await _orderService.UpdateOrderAsync(id, order);
            if (!response.Success)
            {
                // Return structured JSON instead of plain text
                if (string.Equals(response.Message, "Order not found.", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }
            return NoContent(); // 204 No Content indicates update succeeded

        }

        // DELETE: api/Orders/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var response = await _orderService.DeleteOrderAsync(id);
            if (!response.Success)
            {
                return NotFound(response.Message);
            }
            return NoContent(); // Returns 204 No Content to indicate successful deletion

        }
    }
}
