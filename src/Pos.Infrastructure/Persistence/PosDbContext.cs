using Microsoft.EntityFrameworkCore;
using Pos.Domain.Entities;

namespace Pos.Infrastructure.Persistence;

public sealed class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<User> Usuarios => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permisos => Set<Permission>();
    public DbSet<UserRole> UsuarioRoles => Set<UserRole>();
    public DbSet<RolePermission> RolPermisos => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PosDbContext).Assembly);

        modelBuilder.Entity<Tenant>().HasData(SeedData.Tenant);
        modelBuilder.Entity<Sucursal>().HasData(SeedData.Sucursal);
        modelBuilder.Entity<User>().HasData(SeedData.AdminUser);
        modelBuilder.Entity<Role>().HasData(SeedData.Roles);
        modelBuilder.Entity<Permission>().HasData(SeedData.Permissions);
        modelBuilder.Entity<UserRole>().HasData(SeedData.UserRoles);
        modelBuilder.Entity<RolePermission>().HasData(SeedData.RolePermissions);
    }
}
