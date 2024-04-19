using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

[Authorize(Roles = "Customer, Admin, SuperAdmin")]
[Route("api/[controller]")]
[ApiController]
public class UserFavoriteController : ControllerBase
{
    private readonly IUserFavoriteService _userFavoriteService;

    public UserFavoriteController(IUserFavoriteService userFavoriteService)
    {
        _userFavoriteService = userFavoriteService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ServiceResponse<List<UserFavorite>>>> GetUserFavorites(int userId)
    {
        var response = await _userFavoriteService.GetUserFavoritesAsync(userId);
        if (!response.Success)
            return NotFound(response.Message);
        return Ok(response);
    }

    [HttpGet("single/{favoriteId}")]
    public async Task<ActionResult<ServiceResponse<UserFavorite>>> GetFavoriteById(int favoriteId)
    {
        var response = await _userFavoriteService.GetFavoriteByIdAsync(favoriteId);
        if (!response.Success)
            return NotFound(response.Message);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse<UserFavorite>>> AddFavorite([FromBody] UserFavorite favorite)
    {
        var response = await _userFavoriteService.AddFavoriteAsync(favorite);
        if (!response.Success)
            return BadRequest(response.Message);
        return CreatedAtAction(nameof(GetFavoriteById), new { favoriteId = response?.Data?.Id }, response?.Data);
    }

    [HttpDelete("{favoriteId}")]
    public async Task<ActionResult<ServiceResponse<bool>>> RemoveFavorite(int favoriteId)
    {
        var response = await _userFavoriteService.RemoveFavoriteAsync(favoriteId);
        if (!response.Success)
            return NotFound(response.Message);
        return NoContent();
    }
}
