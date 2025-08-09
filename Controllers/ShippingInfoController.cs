using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;
using TheLibraryOfAlexandria.Services;

namespace TheLibraryOfAlexandria.Controllers
{
    [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingInfoController : ControllerBase
    {
        private readonly IShippingInfoService _service;

        public ShippingInfoController(IShippingInfoService service)
        {
            _service = service;
        }

        // GET: api/ShippingInfo
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<ShippingInfo>>>> GetAll()
        {
            var resp = await _service.GetAllShippingInfosAsync();
            if (!resp.Success) return BadRequest(resp);
            return Ok(resp);
        }

        // GET: api/ShippingInfo/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<ShippingInfo>>> GetById(int id)
        {
            var resp = await _service.GetShippingInfoByIdAsync(id);
            if (!resp.Success) return NotFound(resp);
            return Ok(resp);
        }

        // GET: api/ShippingInfo/order/{orderId}
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<ServiceResponse<ShippingInfo>>> GetByOrderId(int orderId)
        {
            var resp = await _service.GetShippingInfoByOrderIdAsync(orderId);
            if (!resp.Success) return NotFound(resp);
            return Ok(resp);
        }

        // POST: api/ShippingInfo
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<ShippingInfo>>> Create([FromBody] ShippingInfo model)
        {
            var resp = await _service.CreateShippingInfoAsync(model);
            if (!resp.Success) return BadRequest(resp);
            return CreatedAtAction(nameof(GetById), new { id = resp.Data.Id }, resp);
        }

        // PUT: api/ShippingInfo/{id}
        // Updates only selected fields (Status, TrackingNumber, ShippingCost)
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<ShippingInfo>>> Put(int id, [FromBody] UpdateShippingRequest request)
        {
            var placeholder = new ShippingInfo
            {
                Status = request.Status ?? ShippingStatus.Preparing,
                TrackingNumber = request.TrackingNumber ?? string.Empty,
                ShippingCost = request.ShippingCost ?? 0
            };
            var resp = await _service.UpdateShippingInfoAsync(id, placeholder);
            if (!resp.Success) return BadRequest(resp);
            return Ok(resp);
        }

        // DELETE: api/ShippingInfo/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var resp = await _service.DeleteShippingInfoAsync(id);
            if (!resp.Success) return NotFound(resp);
            return NoContent();
        }
    }

    public class UpdateShippingRequest
    {
        public ShippingStatus? Status { get; set; }
        public string? TrackingNumber { get; set; }
        public decimal? ShippingCost { get; set; }
    }
}


