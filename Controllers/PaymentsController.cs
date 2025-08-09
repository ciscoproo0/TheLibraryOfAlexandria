using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<List<Payment>>>> Get(
        [FromQuery] string? status = null,
        [FromQuery] string? method = null,
        [FromQuery] int? orderId = null,
        [FromQuery] int? userId = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        [FromQuery] DateTime? completedFrom = null,
        [FromQuery] DateTime? completedTo = null,
        [FromQuery] string? transactionId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null
    )
    {
        var resp = await _paymentService.GetPaymentsAsync(status, method, orderId, userId, minAmount, maxAmount, createdFrom, createdTo, completedFrom, completedTo, transactionId, sortBy, sortDir);
        if (!resp.Success) return BadRequest(resp);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse<Payment>>> CreatePayment(Payment payment)
    {
        var response = await _paymentService.CreatePaymentAsync(payment);
        if (!response.Success)
        {
            return BadRequest(response.Message);
        }
        return CreatedAtAction("GetPayment", new { id = response?.Data?.Id }, response?.Data);
    }

    [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResponse<Payment>>> GetPayment(int id)
    {
        var response = await _paymentService.GetPaymentByIdAsync(id);
        if (!response.Success)
        {
            return NotFound(response.Message);
        }
        return Ok(response);
    }

    [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(int id, Payment payment)
    {
        var response = await _paymentService.UpdatePaymentAsync(id, payment);
        if (!response.Success)
        {
            return BadRequest(response.Message);
        }
        return NoContent();
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        var response = await _paymentService.DeletePaymentAsync(id);
        if (!response.Success)
        {
            return NotFound(response.Message);
        }
        return NoContent();
    }
}
