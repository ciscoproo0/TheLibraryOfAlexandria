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
