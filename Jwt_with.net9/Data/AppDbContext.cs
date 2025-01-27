using Jwt_with.net9.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jwt_with.net9.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}
	public DbSet<User> Users { get; set; }
}
