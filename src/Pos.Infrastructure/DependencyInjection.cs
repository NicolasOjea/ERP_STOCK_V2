using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Abstractions;
using Pos.Infrastructure.Persistence;
using Pos.Infrastructure.Repositories;
using Pos.Infrastructure.Security;
using Pos.Infrastructure.Services;

namespace Pos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' is not configured.");
        }

        services.AddDbContext<PosDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<ICajaRepository, CajaRepository>();
        services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
        return services;
    }
}
