using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;


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

        // GET: api/Order
        [Authorize(Roles = "Customer, Admin, SuperAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var response = await _orderService.GetAllOrdersAsync();
            if (response.Success)
            {
                return Ok(response.Data);
            }
            else
            {
                return BadRequest(response.Message);
            }
        }

        // GET: api/Orders/5
        [Authorize(Roles = "Customer, Admin, SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var response = await _orderService.GetOrderByIdAsync(id);
            if (response.Success)
            {
                if (response.Data == null)
                {
                    return NotFound("Order not found.");
                }
                return Ok(response.Data);
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
        public async Task<ActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest("Mismatched order ID");
            }

            var response = await _orderService.UpdateOrderAsync(id, order);
            if (!response.Success)
            {
                return NotFound(response.Message);
            }
            return NoContent(); // Retorna um status 204 No Content como resposta para indicar sucesso na atualização

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
            return NoContent(); // Retorna um status 204 No Content como resposta para indicar sucesso na deleção

        }
    }
}
