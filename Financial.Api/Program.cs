using Financial.Data;
using Financial.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("FinancialDb"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Authentication - supports Duende IdentityServer via JWT Bearer tokens
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configure for Duende IdentityServer
        // IdentityServer running on http://localhost:5001
        options.Authority = "http://localhost:5001";
        options.Audience = "financial-api";
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
        
        // Token validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:5001",
            ValidateAudience = false, // Flexible audience validation for development
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(60) // Allow for clock skew
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var authHeader = context.Request.Headers.Authorization.ToString();
                logger.LogInformation($"Auth header received: {(string.IsNullOrEmpty(authHeader) ? "NONE" : authHeader.Substring(0, Math.Min(50, authHeader.Length)) + "...")}");
                logger.LogInformation($"Request path: {context.Request.Path}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError($"Authentication failed: {context.Exception.Message}");
                logger.LogError($"Exception type: {context.Exception.GetType().Name}");
                
                context.NoResult();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new 
                { 
                    message = "Unauthorized", 
                    error = builder.Environment.IsDevelopment() ? context.Exception.Message : "Invalid token"
                });
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}").ToList() ?? new List<string>();
                logger.LogInformation($"Token validated. Claims: {string.Join(", ", claims)}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDev", builder =>
    {
        builder
            .WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:5001", "https://localhost:5002")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowDev");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
