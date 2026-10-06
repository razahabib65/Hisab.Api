using Hisab.Data;
using Hisab.DTOs;
using System.IO;
using Hisab.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Hisab.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(
      AppDbContext context,
      IConfiguration configuration)
    {
      _context = context;
      _configuration = configuration;
      _passwordHasher = new PasswordHasher<User>();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterRequest request)
    {
      if (request.Password != request.ConfirmPassword)
      {
        return BadRequest(new
        {
          message = "Passwords do not match."
        });
      }

      if (string.IsNullOrWhiteSpace(request.BusinessName))
      {
        return BadRequest(new
        {
          message = "Business name is required."
        });
      }

      if (string.IsNullOrWhiteSpace(request.OwnerName))
      {
        return BadRequest(new
        {
          message = "Owner name is required."
        });
      }

      if (string.IsNullOrWhiteSpace(request.Email))
      {
        return BadRequest(new
        {
          message = "Email is required."
        });
      }

      if (string.IsNullOrWhiteSpace(request.Password))
      {
        return BadRequest(new
        {
          message = "Password is required."
        });
      }

      var existingUser = await _context.Users
          .FirstOrDefaultAsync(u => u.Email == request.Email);

      if (existingUser != null)
      {
        return BadRequest(new
        {
          message = "A user with this email already exists."
        });
      }
      string? logoPath = null;

      if (request.Logo != null)
      {
        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "business"
        );

        Directory.CreateDirectory(uploadsFolder);

        var fileName = Guid.NewGuid().ToString()
                       + Path.GetExtension(request.Logo.FileName);

        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
          await request.Logo.CopyToAsync(stream);
        }

        logoPath = "/uploads/business/" + fileName;
      }
      var business = new Business
      {
        BusinessName = request.BusinessName,
        BusinessType = request.BusinessType,
        Address = request.Address,
        Logo = logoPath
      };

      var user = new User
      {
        OwnerName = request.OwnerName,
        Phone = request.Phone,
        Email = request.Email,
        Business = business
      };

      user.PasswordHash = _passwordHasher.HashPassword(
          user,
          request.Password
      );

      _context.Users.Add(user);

      await _context.SaveChangesAsync();

      return Ok(new
      {
        message = "Registration successful.",
        userId = user.Id,
        businessId = business.Id
      });
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
      var user = await _context.Users
          .FirstOrDefaultAsync(u => u.Email == request.Email);

      if (user == null)
      {
        return BadRequest(new
        {
          message = "Invalid email or password."
        });
      }

      var result = _passwordHasher.VerifyHashedPassword(
          user,
          user.PasswordHash,
          request.Password
      );

      if (result == PasswordVerificationResult.Failed)
      {
        return BadRequest(new
        {
          message = "Invalid email or password."
        });
      }

      // JWT Claims
      var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()
        ),

        new Claim(
            ClaimTypes.Email,
            user.Email
        ),

        new Claim(
            ClaimTypes.Name,
            user.OwnerName
        ),

        new Claim(
            "BusinessId",
            user.BusinessId.ToString()
        )
    };

      // JWT Key
      var key = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(
              _configuration["Jwt:Key"]!
          )
      );

      // Signing Credentials
      var credentials = new SigningCredentials(
          key,
          SecurityAlgorithms.HmacSha256
      );

      // Create JWT Token
      var token = new JwtSecurityToken(
          issuer: _configuration["Jwt:Issuer"],
          audience: _configuration["Jwt:Audience"],
          claims: claims,
          signingCredentials: credentials
      );

      // Convert token to string
      var tokenString = new JwtSecurityTokenHandler()
          .WriteToken(token);

      return Ok(new
      {
        message = "Login successful.",
        token = tokenString,
        userId = user.Id,
        businessId = user.BusinessId
      });
    }
  }

  }
