using Microsoft.AspNetCore.Mvc;
using MobileAPI;
using MobileAPI.Helpers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly PostgresHelper _postgresHelper;

    public UserController(PostgresHelper postgresHelper)
    {
        _postgresHelper = postgresHelper;
    }

    // ✅ GET Users
    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _postgresHelper.GetUsers();
        return Ok(users);
    }

    // ✅ POST New User
    [HttpPost]
    public IActionResult CreateUser([FromBody] UserDto user)
    {
        _postgresHelper.InsertUser(user.Name, user.Email);
        return Ok("User added successfully!");
    }


[HttpDelete("{userid}")] 
public IActionResult DeleteUser(int userid)
{
    try
        {
        bool isDeleted = _postgresHelper.DeleteUser(userid);

        if (!isDeleted)
        {
            return NotFound($"❌ User with ID {userid} not found.");
        }

        return NoContent(); // HTTP 204 (Success, No Content)
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"❌ Internal Server Error: {ex.Message}");
    }
  }
}

// DTO Class for API request
public class UserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
}
