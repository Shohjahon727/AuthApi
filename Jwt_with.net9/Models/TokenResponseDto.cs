namespace Jwt_with.net9.Models
{
	public class TokenResponseDto
	{
		public required string AccessToken { get; set; }
		public required string RefreshToken { get; set; }
		public UserDto User { get; set; }
	}
}
