using BuildingBlocks.Messaging.Extensions;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WalletPayment.API.Middleware;
using WalletPayment.API.Services;
using WalletPayment.Application;
using WalletPayment.Infrastructure;
using WalletPayment.Infrastructure.Persistence.Context;
using BuildingBlocks.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WalletPayment API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ✅ Add layers in correct order
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Carter for minimal API endpoints
builder.Services.AddCarter();

// Add HTTP Context Accessor for CurrentUserService
builder.Services.AddHttpContextAccessor();

// ✅ Register API-specific CurrentUserService - FIXED
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Messaging (In-Memory for Monolith)
builder.Services.AddMessaging(
    builder.Configuration,
    typeof(Program).Assembly); // Register event handlers from API assembly

// JWT Authentication
var authOptions = builder.Configuration.GetSection("Autentication");
var secretKey = authOptions["SecretKey"]!;
var issuer = authOptions["Issuer"]!;
var audience = authOptions["Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<WalletDbContext>("wallet-db");

var app = builder.Build();
app.UseCors("AllowAllOrigins");
//app.UseExceptionHandler();
app.UseStatusCodePages();
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Custom Middleware
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<CurrentUserMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Carter endpoints
app.MapCarter();

// Health checks
app.MapHealthChecks("/health");

// Controllers (if any)
app.MapControllers();

// Database migration (Development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();