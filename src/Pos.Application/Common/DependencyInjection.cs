using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Abstractions;
using Pos.Application.UseCases.Health;

namespace Pos.Application.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHealthService, HealthService>();
        return services;
    }
}
