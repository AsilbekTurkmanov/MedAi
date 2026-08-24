using System;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using MedAI.API.Middlewares;
using MedAI.Application.Interfaces;
using MedAI.Application.Validators;
using MedAI.Infrastructure.Authentication;
using MedAI.Infrastructure.Data;
using MedAI.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Setup — Structured logging with enrichment
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MedAI")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();
builder.Host.UseSerilog();

// 2. Database Context Setup with Dynamic Provider Selection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Host=localhost;Database=medaidb;Username=postgres;Password=postgres";

bool usePostgres = false;
try
{
    using var testConn = new Npgsql.NpgsqlConnection(connectionString);
    testConn.Open();
    usePostgres = true;
    Log.Information("Successfully connected to PostgreSQL database.");
}
catch
{
    Log.Warning("PostgreSQL connection unavailable. Falling back to InMemory Database for seamless development execution.");
}

if (usePostgres)
{
    builder.Services.AddDbContext<MedAiDbContext>(options =>
        options.UseNpgsql(connectionString, b => b.MigrationsAssembly("MedAI.Infrastructure")));
}
else
{
    builder.Services.AddDbContext<MedAiDbContext>(options =>
        options.UseInMemoryDatabase("MedAiDevDb"));
}

builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<MedAiDbContext>());

// 3. Application & Infrastructure Services DI
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 4. FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// 5. Authentication & JWT Setup
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? (builder.Environment.IsDevelopment()
        ? "MedAI_Dev_Secret_Key_2026_Not_For_Production_Use_MinLength64Chars!!"
        : throw new InvalidOperationException("JWT_SECRET environment variable is required in production."));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 6. Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// 7. CORS Configuration
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:3001", "http://localhost:3002", "http://localhost:3003", "http://localhost:3010" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// 8. Swagger / OpenAPI Setup with JWT Security Definition & Tag Grouping
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MEDAI — Intelligent AI Healthcare Ecosystem API",
        Version = "v1",
        Description = "Production ASP.NET Core Web API for MedAI Platform supporting Patient, Doctor, Clinic, and AI Assistant Operations."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and your valid JWT token.\n\nExample: `Bearer eyJhbGciOiJIUzI1Ni...`"
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 9. Middleware Pipeline — order matters
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger available in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MedAI API v1");
    c.DocumentTitle = "MedAI API Documentation";
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Static files for document uploads
app.UseStaticFiles();

app.MapControllers();

// 10. Auto Migration & Seed Data Execution
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MedAiDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher>();

        if (usePostgres)
        {
            await context.Database.MigrateAsync();
            Log.Information("Database migration applied successfully.");
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
            Log.Information("InMemory database created successfully.");
        }

        await DbInitializer.SeedAsync(context, hasher);
        Log.Information("Database initialization and seed data populated successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while initializing/seeding the database.");
    }
}

app.Run();
