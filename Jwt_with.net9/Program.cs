using Jwt_with.net9.Data;
using Jwt_with.net9.Services;
using Jwt_with.net9.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// CORS siyosatini qo‘shing
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAllOrigins", policy =>
	{
		policy.AllowAnyOrigin()   // Barcha domenlarga ruxsat
			  .AllowAnyMethod()   // Har qanday HTTP metodlariga ruxsat
			  .AllowAnyHeader();  // Har qanday headerlarga ruxsat
	});
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = false,
			ValidIssuer = builder.Configuration["AppSettings:Issuer"],
			ValidateAudience = false,
			ValidAudience = builder.Configuration["AppSettings:Audience"],
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
		};
	}
	);

builder.Services.AddScoped<IAuthService, AuthService>();
var app = builder.Build();
// CORS siyosatini qo‘llang
app.UseCors("AllowAllOrigins");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
