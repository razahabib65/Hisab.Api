using Hisab.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// Controllers
// ======================================================

builder.Services.AddControllers();


// ======================================================
// Database
// ======================================================

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("DefaultConnection is missing.");
}

if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(connectionString);

    var userInfo = uri.UserInfo.Split(':', 2);

    var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1
            ? Uri.UnescapeDataString(userInfo[1])
            : ""
    };

    connectionString = npgsqlBuilder.ConnectionString;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// ======================================================
// JWT Authentication
// ======================================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                )
            )
        };
    });


// ======================================================
// Authorization
// ======================================================

builder.Services.AddAuthorization();


// ======================================================
// CORS - Angular
// ======================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ======================================================
// Swagger
// ======================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hisab API",
        Version = "v1"
    });

    // JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// ======================================================
// Build Application
// ======================================================

var app = builder.Build();


// ======================================================
// Swagger UI
// ======================================================

app.UseSwagger();
app.UseSwaggerUI();

// ======================================================
// Middleware
// ======================================================

app.UseHttpsRedirection();


// Business logo/images
app.UseStaticFiles();


// CORS
app.UseCors("AngularPolicy");


// Authentication MUST come before Authorization
app.UseAuthentication();

app.UseAuthorization();


// Controllers
app.MapControllers();


// Run
app.Run();