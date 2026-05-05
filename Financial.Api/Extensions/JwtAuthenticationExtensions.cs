using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Financial.Api.Extensions
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IHostEnvironment environment)
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Configure for Duende IdentityServer
                    // IdentityServer running on http://localhost:5001
                    options.Authority = "http://localhost:5001";
                    options.Audience = "financial-api";
                    options.RequireHttpsMetadata = environment.IsProduction();

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
                                error = environment.IsDevelopment() ? context.Exception.Message : "Invalid token"
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

            return services;
        }
    }
}