using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;

namespace TheLibraryOfAlexandria.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users
        [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var result = await _userService.GetAllUsersAsync();
            if (result.Success)
                return Ok(result.Data);
            return NotFound(result.Message);
        }

        // GET: api/Users/5
        [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }

        // POST: api/Users
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            var result = await _userService.CreateUserAsync(user);
            if (!result.Success)
                return BadRequest(result.Message);
            return CreatedAtAction("GetUser", new { id = result?.Data?.Id }, result?.Data);
        }

        // PUT: api/Users/5
        [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            var result = await _userService.UpdateUserAsync(id, user);
            if (!result.Success)
                return BadRequest(result.Message);
            return NoContent();
        }

        // DELETE: api/Users/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            return NoContent();
        }
    }
}
