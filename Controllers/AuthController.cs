using System.Security.Cryptography;
using System.Text;
using Kovalev.Data;
using Kovalev.Models;
using Kovalev.Models.Auth;
using Kovalev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthRegisterRequest = Kovalev.Models.Auth.RegisterRequest;
using AuthLoginRequest = Kovalev.Models.Auth.LoginRequest;

namespace Kovalev.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, JwtTokenService jwtTokenService, IConfiguration configuration)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(AuthRegisterRequest request)
    {
        try
        {
            // Проверяем, существует ли пользователь с таким именем
            if (await _context.Users.AnyAsync(u => u.Name == request.Username))
            {
                return BadRequest(new { message = "Пользователь с таким именем уже существует" });
            }

            // Проверяем, существует ли пользователь с таким email
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            // Создаем нового пользователя
            var user = new AppUser
            {
                Name = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password ?? ""),
                Address = "",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Генерируем JWT токен
            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Username = user.Name,
                Email = user.Email,
                Expiration = DateTime.UtcNow.AddMinutes(_configuration.GetValue<double>("JwtSettings:ExpirationInMinutes"))
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(AuthLoginRequest request)
    {
        try
        {
            // Ищем пользователя по имени (поля name)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == request.Username);

            if (user == null || !VerifyPassword(request.Password ?? "", user.PasswordHash ?? ""))
            {
                return Unauthorized(new { message = "Неверное имя пользователя или пароль" });
            }

            // Генерируем JWT токен
            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Username = user.Name,
                Email = user.Email,
                Expiration = DateTime.UtcNow.AddMinutes(_configuration.GetValue<double>("JwtSettings:ExpirationInMinutes"))
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}