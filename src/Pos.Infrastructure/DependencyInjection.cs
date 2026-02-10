using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Abstractions;
using Pos.Infrastructure.Persistence;
using Pos.Infrastructure.Repositories;
using Pos.Infrastructure.Security;
using Pos.Infrastructure.Services;
using Pos.Infrastructure.Adapters.Pdf;
using Pos.Infrastructure.Adapters.Fiscal;

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
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
        services.AddScoped<IDocumentoCompraRepository, DocumentoCompraRepository>();
        services.AddScoped<IPreRecepcionRepository, PreRecepcionRepository>();
        services.AddScoped<IDocumentParser, Adapters.JsonDocumentParser>();
        services.AddScoped<IRecepcionRepository, RecepcionRepository>();
        services.AddScoped<IListaPrecioRepository, ListaPrecioRepository>();
        services.AddScoped<IDevolucionRepository, DevolucionRepository>();
        services.AddScoped<IEtiquetaPdfGenerator, EtiquetaPdfGenerator>();
        services.AddScoped<IRemitoPdfGenerator, RemitoPdfGenerator>();
        services.AddScoped<IAuditLogQueryRepository, AuditLogQueryRepository>();
        services.AddScoped<IReportesRepository, ReportesRepository>();
        services.AddScoped<IFiscalProvider, DummyFiscalProvider>();
        services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
        services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
        return services;
    }
}
