using Jwt_with.net9.Entities;
using Jwt_with.net9.Models;

namespace Jwt_with.net9.Services.Interfaces
{
	public interface IAuthService
	{
		Task<User?> RegisterAsync(UserDto userDto);
		Task<TokenResponseDto?> LoginAsync(UserDto userDto);
		Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
	}
}
