using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.API.Data;
using PaymentGateway.API.Gateways;
using PaymentGateway.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<ZarinPalGateway>();
builder.Services.AddScoped<ZarinPalGateway>();
builder.Services.AddScoped<SandboxGateway>();
builder.Services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddCarter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapCarter();

app.Run();