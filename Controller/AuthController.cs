using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MobileAPI.Data;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.Mime.MediaTypeNames;

namespace JwtAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IConfiguration _config;

        private readonly DbDeepStudyServices _deepdb;

    

        public AuthController(DbDeepStudyServices _deepdbobj, IConfiguration _configobj)
        {
            _deepdb = _deepdbobj;
            _config = _configobj;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
         
            List<LoginResponse> result = await _deepdb.ReadAsyncWithParameter("SELECT * FROM user_authenticate(@p_username,@p_password_hash)", new()
            {
                { "p_username", request.Username },
                { "p_password_hash", request.Password } 
            }, reader =>  new  LoginResponse
            {
                    Userid = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(1), 
            });

          
            if (result.Count>0)
            {
                var token = GenerateJwtToken(request.Username);
                var refreshToken = GenerateRefreshToken(); 

                await _deepdb.ExecuteFunctionAsync("SELECT user_token_create(@p_user_id, @p_token,@p_expires_at)", new()
                {
                        { "p_user_id", result.First().Userid },
                        { "p_token", refreshToken },
                        { "p_expires_at",  CurrentTime().AddHours(12)  }
                }); 

                return Ok(new
                {
                    token = token,
                    refreshToken = refreshToken
                }); 
               
            }
            else
            {
                return Unauthorized();
            }
             
        }

        [HttpPost("refresh")]
        public async  Task<IActionResult> Refresh(string username, string refreshToken)
        {
            // if (refreshTokens.TryGetValue(username, out var storedToken) && storedToken == refreshToken)

            List<LoginRefreshToken> result = await _deepdb.ReadAsyncWithParameter("SELECT * FROM user_token_validate(@p_token)", new()
            { 
                { "p_token", refreshToken }
            }, reader => new LoginRefreshToken
            {
               Userid= reader.GetInt32(0),
                Username = username,
                RefreshToken = reader.GetString(1),
            });

            string storedToken = "";

            if (result.Count > 0)
                storedToken= result.First().RefreshToken;


                if (storedToken == refreshToken)  
            {
                var newAccessToken = GenerateJwtToken(username);
                var newRefreshToken = GenerateRefreshToken();
                //refreshTokens[username] = newRefreshToken;

                await _deepdb.ExecuteFunctionAsync("SELECT user_token_create(@p_user_id, @p_token,@p_expires_at)", new()
                {
                        { "p_user_id", result.First().Userid },
                        { "p_token", newRefreshToken },
                        { "p_expires_at", CurrentTime().AddHours(12)  }
                });

                return Ok(new
                {
                    accessToken = newAccessToken,
                    refreshToken = newRefreshToken
                });
            }

            return Unauthorized();
        } 

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string token)
        {
          
            await _deepdb.ExecuteFunctionAsync("SELECT user_token_revoke(@p_token,@p_revoked_at)", new()
                { 
                        { "p_token", token },
                        { "p_revoked_at", DateTime.UtcNow  }
                }); 
         
            return Ok("Logged out successfully.");
        }


        [HttpPost("user")]
        public async Task<IActionResult> AppUser([FromBody] User user)
        { 

            await _deepdb.ExecuteFunctionAsync("SELECT user_create(@p_username, @p_email,@p_password_hash)", new()
            {
                                    { "p_username", user.Username },
                                    { "p_email", user.Email},
                                    { "p_password_hash", user.Password }
            }); 
            return Ok("User Created.");  
           
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
                Expires = CurrentTime().AddHours(7), //DateTime.UtcNow.AddHours(1),
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

        DateTime CurrentTime() 
        {
            DateTime utcNow = DateTime.UtcNow;
            // Define the Indian Standard Time zone.
            TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            // Convert the UTC time to IST.
            DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, istTimeZone);
            return istTime;
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        } 
        public class LoginResponse
        {
            public int Userid { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }  
       public class LoginRefreshToken
        {
            public int Userid { get; set; }
            public string Username { get; set; }
            public string RefreshToken { get; set; }
        }

        public class User
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }


    }
}
