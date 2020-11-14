using JWTExample.Models.Dtos;
using JWTExample.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticationRequestDto model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest("Kullanıcı adı veya şifre yanlış");

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RevokeTokenRequestDto model)
        {

            if (string.IsNullOrEmpty(model.RefreshToken))
                return BadRequest("Token değeri boş olamaz");

            var response = _userService.RefreshToken(model.RefreshToken);
        
            if (response == null)
                return BadRequest("Geçersiz token değeri girildi");


            return Ok(response);
        }

        [Authorize]
        [HttpPost("revoke-refresh-token")]
        public IActionResult RevokeToken([FromBody] RevokeTokenRequestDto model)
        {

            if (string.IsNullOrEmpty(model.RefreshToken))
                return BadRequest("Token değeri boş olamaz");

            var response = _userService.RevokeToken(model.RefreshToken);
            if (!response)
                return NotFound("Geçersiz token değeri girildi");

            return Ok("Token revoked");
        }
    }
}
