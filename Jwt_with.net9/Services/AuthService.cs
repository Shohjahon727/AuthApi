using Jwt_with.net9.Data;
using Jwt_with.net9.Entities;
using Jwt_with.net9.Models;
using Jwt_with.net9.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Jwt_with.net9.Services
{
	public class AuthService : IAuthService
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _configuration;
		public AuthService(AppDbContext context,IConfiguration configuration)
		{
			_configuration = configuration;
			_context = context;
		}
		public async Task<TokenResponseDto?> LoginAsync(UserDto userDto)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
			if (user is null)
			{
				return null;
			}

			if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, userDto.Password)
				== PasswordVerificationResult.Failed)
			{
				return null;
			}
			
			return await CreateTokenResponse(user);
		}

		public async Task<User?> RegisterAsync(UserDto userDto)
		{
			if(await _context.Users.AnyAsync(u => u.Username == userDto.Username))
			{
				return null;
			}
			var user = new User();
			var hashedPassword = new PasswordHasher<User>()
				.HashPassword(user, userDto.Password);

			user.Username = userDto.Username;
			user.PasswordHash = hashedPassword;

			await _context.Users.AddAsync(user);

			await CreateTokenResponse(user);
			await _context.SaveChangesAsync();
			return user;
		}
		private async Task<TokenResponseDto> CreateTokenResponse(User? user)
		{
			return new TokenResponseDto
			{
				AccessToken = CreateToken(user),
				RefreshToken = await GenerateAndSaveRefreshToken(user),
				Role = user.Role,
				
				User = new UserDto
				{
					Username = user.Username,
				}
			};
		}
		public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
		{
			var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
			if (user is null)
			{
				return null;
			}
			//if(new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
			//	== PasswordVerificationResult.Failed)
			//{
			//	return null;
			//}
			
			
			return await CreateTokenResponse(user);
		}
		private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
		{
			var user = await _context.Users.FindAsync(userId);
			if(user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
			{
				return null;
			}
			return user;
		}
		private string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);
			return Convert.ToBase64String(randomNumber);
		}

		private async Task<string> GenerateAndSaveRefreshToken(User user)
		{
			var refreshToken = GenerateRefreshToken();
			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
			await _context.SaveChangesAsync();
			return refreshToken;
		}

		private string CreateToken(User user)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.Username),
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Role, user.Role)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
			var tokenDescriptor = new JwtSecurityToken(

				issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
				audience: _configuration.GetValue<string>("AppSettings:Audience"),
				claims: claims,
				expires: DateTime.Now.AddDays(1),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
		}

		
	}
}
