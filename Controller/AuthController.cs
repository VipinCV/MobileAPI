using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IConfiguration _config;
      
        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
           // var refreshTokens = new Dictionary<string, string>();

            if (request.Username != "admin" || request.Password != "password")
                return Unauthorized();


            var token = GenerateJwtToken(request.Username);
            var refreshToken = GenerateRefreshToken();
            //refreshTokens[request.Username] = refreshToken; //save Username and refreshtoken in DB

            return Ok(new
            {
                token=    token,
                refreshToken=    refreshToken
            }); 

        }

        [HttpPost("refresh")]
        public IActionResult Refresh(string username, string refreshToken)
        {
           // if (refreshTokens.TryGetValue(username, out var storedToken) && storedToken == refreshToken)

                if(true) //check refresh token in database
            {
                var newAccessToken = GenerateJwtToken(username);
                var newRefreshToken = GenerateRefreshToken();
                //refreshTokens[username] = newRefreshToken;

                return Ok(new
                {
                    accessToken = newAccessToken,
                    refreshToken = newRefreshToken
                });
            }

            return Unauthorized();
        }


        [HttpPost("logout")]
        public IActionResult Logout([FromBody] string username)
        {
          //  userRefreshTokens.Remove(username); // Invalidate refresh token 
            return Ok("Logged out successfully.");
        }
        private string GenerateJwtToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSettings = _config.GetSection("JwtSettings");

            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                }),
                Expires = DateTime.UtcNow.AddMinutes(1), //DateTime.UtcNow.AddHours(1),
                Issuer = "MyApp",
                Audience = "MyAppUsers",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString(); // simple random string
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
