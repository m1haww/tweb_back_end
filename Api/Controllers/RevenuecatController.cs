using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RevenuecatController : ControllerBase
{
    [HttpPost("set-user")]
    public async Task<IActionResult> SetUser([FromBody] AddRevenuecatUserDto user)
    {
        return Ok("Received data about user.");
    }
}