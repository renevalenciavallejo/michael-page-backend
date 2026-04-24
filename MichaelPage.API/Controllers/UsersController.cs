using MichaelPage.Application.Users;
using MichaelPage.Application.Users.Dto;
using Microsoft.AspNetCore.Mvc;

namespace MichaelPage.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class UsersController(IUserService userService, ILogger<UsersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await userService.GetAllUsers();
        if (result.Success)
            return Ok(result);

        logger.LogError("UsersController - GetAll: {ErrorMessage}", result.Message);
        return BadRequest(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await userService.GetUserById(id);
        if (result.Success)
            return Ok(result);

        logger.LogWarning("UsersController - GetById: {ErrorMessage}", result.Message);
        return NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto input)
    {
        var result = await userService.CreateUser(input);
        if (result.Success)
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);

        logger.LogError("UsersController - Create: {ErrorMessage}", result.Message);
        return BadRequest(result);
    }
}
