using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Abstractions;
using Pos.Application.UseCases.Auth;
using Pos.Application.UseCases.Health;
using Pos.Application.UseCases.Users;

namespace Pos.Application.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHealthService, HealthService>();
        services.AddScoped<LoginService>();
        services.AddScoped<UpdateUserRolesService>();
        return services;
    }
}
