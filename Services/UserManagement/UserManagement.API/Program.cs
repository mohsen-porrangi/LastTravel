using BuildingBlocks.Contracts.Options;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserManagement.API.Infrastructure.Configuration;
using UserManagement.API.Infrastructure.Data;
using UserManagement.API.Infrastructure.Middleware;

namespace UserManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var assembly = typeof(Program).Assembly;

            // Configure Services
            ConfigureAuthentication(builder);
            ConfigureApplicationServices(builder, assembly);
            ConfigureCors(builder);
            ConfigureLogging(builder);
            ConfigureSwagger(builder);

            var app = builder.Build();

            // Configure Pipeline
            ConfigurePipeline(app);

            // Initialize Database
            InitializeDatabase(app);

            app.Run();
        }

        private static void ConfigureAuthentication(WebApplicationBuilder builder)
        {
            builder.Services.Configure<AutenticationOptions>(
                builder.Configuration.GetSection(AutenticationOptions.Name));

            var jwtSettings = builder.Configuration
                .GetSection(AutenticationOptions.Name)
                .Get<AutenticationOptions>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwtSettings!.Issuer,
                        ValidAudience = jwtSettings!.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings!.SecretKey))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(
                                "{\"error\": \"دسترسی غیرمجاز: توکن معتبر نیست یا وجود ندارد.\"}");
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(
                                "{\"error\": \"شما اجازه دسترسی به این منبع را ندارید.\"}");
                        }
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                    policy.RequireAssertion(_ => true));
            });
        }

        private static void ConfigureSwagger(WebApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "User Management API",
                    Version = "v1",
                    Description = "API برای مدیریت کاربران سیستم",
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });

                c.MapType<Guid?>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "uuid",
                    Nullable = true
                });

                c.UseAllOfForInheritance();
                c.UseOneOfForPolymorphism();
                c.SelectDiscriminatorNameUsing(type => type.Name);
                c.CustomSchemaIds(type => type.FullName);
                c.EnableAnnotations();
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "UserManagement.API.xml"));
            });
        }

        private static void ConfigureApplicationServices(WebApplicationBuilder builder, System.Reflection.Assembly assembly)
        {
            // Health Checks
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<UserDbContext>("Database");

            // Custom Services
            builder.Services.ConfigureSqlServer(builder.Configuration);
            builder.Services.ConfigureMediatR(builder.Configuration);
            builder.Services.ConfigureService(builder.Configuration);

            // Validation and API
            builder.Services.AddValidatorsFromAssembly(assembly);
            builder.Services.AddCarter();

            // Token Service
            builder.Services.AddScoped<ITokenService, JwtTokenService>();
            builder.Services.AddHttpContextAccessor();

            // Exception Handling
            builder.Services.AddExceptionHandler<ErrorHandlerMiddleware>();
            builder.Services.AddProblemDetails();
        }

        private static void ConfigureCors(WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
        }

        private static void ConfigurePipeline(WebApplication app)
        {
            // CORS (باید در اول باشد)
            app.UseCors("AllowAllOrigins");

            // Exception Handling
            app.UseExceptionHandler();
            app.UseStatusCodePages();

            // Swagger
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var basePath = httpReq.PathBase.HasValue ? httpReq.PathBase.Value : string.Empty;
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{basePath}" }
                    };
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "User Management API v1");
                c.RoutePrefix = "swagger";
            });

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Custom Middleware
            app.UseMiddleware<PermissionMiddleware>();

            // API Routes
            app.MapCarter();

            // Health Checks
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }

        private static void InitializeDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            DbInitializer.Seed(db);
        }
    }
}