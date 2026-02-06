using Microsoft.Extensions.DependencyInjection;

namespace Pos.WebApi.Auth;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddPosAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication();
        services.AddAuthorization();
        return services;
    }
}
