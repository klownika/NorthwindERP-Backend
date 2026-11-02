using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Infrastructure.Data;
using NorthwindERP.Infrastructure.Repositories;
using NorthwindERP.Infrastructure.Security;

namespace NorthwindERP.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra la BD, los repositorios y los servicios de seguridad.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Supabase");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("La cadena de conexión de Supabase está vacía o no configurada.");

        services.AddDbContext<NorthwindDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseLowerCaseNamingConvention()); // CustomerId -> customerid

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Scoped = una instancia por petición HTTP
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ILogisticsRepository, LogisticsRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}