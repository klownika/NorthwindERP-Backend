using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;
using NorthwindERP.Core.Services;
using NorthwindERP.Infrastructure.Security;

namespace NorthwindERP.API.Extensions;

/// <summary>Mantiene Program.cs corto: cada bloque de configuración vive aquí.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Servicios de negocio (capa Core).</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ILogisticsService, LogisticsService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }

    /// <summary>Valida el token JWT que el frontend manda en el header Authorization: Bearer xxx.</summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException("Falta la sección 'Jwt' en appsettings.json.");

        if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false; // conserva los nombres cortos: sub, role, employeeId
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = CustomClaims.Username,
                    RoleClaimType = CustomClaims.Role // permite usar [Authorize(Roles = "Admin")]
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>Swagger con botón "Authorize" para probar endpoints protegidos.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NorthwindERP API",
                Version = "v1",
                Description = "Backend del ERP de inventario Northwind (ASP.NET Core + Supabase)."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Pega aquí SOLO el token que devuelve /api/auth/login (sin la palabra Bearer)."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
