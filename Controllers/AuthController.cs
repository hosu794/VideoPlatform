using Microsoft.AspNetCore.Mvc;
using VideoPlatform.Core.Interfaces.Auth;
using VideoPlatform.Core.Models.Auth;

namespace VideoPlatform.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
        {
            var result = await _authService.Register(request);
            if (!result.IsSuccess)
                return BadRequest(new { Message = result.Error });

            return Ok(result.Value);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.Login(request);
            if (!result.IsSuccess)
                return BadRequest(new { Message = result.Error });

            return Ok(result.Value);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshToken(request);
            if (!result.IsSuccess)
                return BadRequest(new { Message = result.Error });

            return Ok(result.Value);
        }
    }
}
