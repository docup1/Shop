using System.Text;
using System.Text.Json.Serialization;
using Application.Services;
using Domain.Contracts;
using Infrastructure.Cache;
using Infrastructure.DataBase;
using Infrastructure.Hash;
using Infrastructure.JWT;
using Infrastructure.DataBase.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.AddScoped<AdminSeeder>();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICache, InMemoryCache>();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException($"Section '{JwtOptions.SectionName}' is missing or empty.");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddScoped<AuthService>(sp => new AuthService(
    sp.GetRequiredService<IUserRepository>(),
    sp.GetRequiredService<ISessionRepository>(),
    sp.GetRequiredService<IPasswordHasher>(),
    sp.GetRequiredService<ITokenService>(),
    sp.GetRequiredService<IUnitOfWork>(),
    TimeSpan.FromDays(jwtOptions.SessionLifetimeDays)));
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Гарантируем наличие администратора из конфигурации (Seed:Admin).
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider
        .GetRequiredService<AdminSeeder>()
        .SeedAsync();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();