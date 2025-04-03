using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace MobileAPI.Controller
{
    [Route("api/redis")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisController()
        {
            _redis = RedisHelper.Connection;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessage([FromBody] string message)
        {
            var pub = _redis.GetSubscriber();
            await pub.PublishAsync("my_channel", message);
            return Ok(new { Message = "Published: " + message });
        }
    }

}