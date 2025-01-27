using Jwt_with.net9.Entities;
using Jwt_with.net9.Models;
using Jwt_with.net9.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Jwt_with.net9.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController(IAuthService authService) : ControllerBase
	{

		[HttpPost("register")]
		public async Task<ActionResult<User>> Register(UserDto userDto)
		{
			var user = await authService.RegisterAsync(userDto);
			if (user is null)
			{
				return BadRequest("UserName already exists");
			}
			return Ok(user);
		}

		[HttpPost("login")]
		public async Task<ActionResult<TokenResponseDto>> Login(UserDto userDto)
		{
			var result = await authService.LoginAsync(userDto);
			if(result is null)
			{
				return BadRequest("Invalid username or password");
			}
			return Ok(result);
		}

		[HttpPost("refresh-token")]
		public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
		{
			var result = await authService.RefreshTokensAsync(request);
			if (result is null || result.AccessToken is null || result.RefreshToken is null)
			{
				return Unauthorized("Invalid refresh token");
			}

			return Ok(result);

		}

		[Authorize]
		[HttpGet]
		public IActionResult AuthenticatedOnlyEndPoint()
		{
			return Ok("You are authenticated");
		}


		[Authorize(Roles = "Admin")]
		[HttpGet("admin")]
		public IActionResult AdminOnlyEndPoint()
		{
			return Ok("You are and admin!");
		}
	}
}
