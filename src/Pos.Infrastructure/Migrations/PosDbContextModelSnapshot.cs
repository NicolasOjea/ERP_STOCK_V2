using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence;

#nullable disable

namespace Pos.Infrastructure.Migrations;

[DbContext(typeof(PosDbContext))]
partial class PosDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

        modelBuilder.Entity("Pos.Domain.Entities.AuditLog", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<int>("Action")
                .HasColumnType("integer");

            b.Property<string>("AfterJson")
                .HasColumnType("text");

            b.Property<string>("BeforeJson")
                .HasColumnType("text");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("EntityId")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<string>("EntityName")
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("character varying(150)");

            b.Property<string>("MetadataJson")
                .HasColumnType("text");

            b.Property<DateTimeOffset>("OccurredAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid?>("UserId")
                .HasColumnType("uuid");

            b.HasKey("Id");

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.ToTable("audit_logs");
        });

        modelBuilder.Entity("Pos.Domain.Entities.Permission", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<string>("Code")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Description")
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnType("character varying(300)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "Code")
                .IsUnique();

            b.ToTable("permisos");
        });

        modelBuilder.Entity("Pos.Domain.Entities.Role", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "Name")
                .IsUnique();

            b.ToTable("roles");
        });

        modelBuilder.Entity("Pos.Domain.Entities.RolePermission", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("PermissionId")
                .HasColumnType("uuid");

            b.Property<Guid>("RoleId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("PermissionId");

            b.HasIndex("RoleId");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "RoleId", "PermissionId")
                .IsUnique();

            b.ToTable("rol_permisos");
        });

        modelBuilder.Entity("Pos.Domain.Entities.Sucursal", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<string>("Code")
                .HasMaxLength(50)
                .HasColumnType("character varying(50)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("TenantId");

            b.ToTable("sucursales");
        });

        modelBuilder.Entity("Pos.Domain.Entities.Tenant", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<bool>("IsActive")
                .HasColumnType("boolean");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("TenantId");

            b.ToTable("tenants");
        });

        modelBuilder.Entity("Pos.Domain.Entities.User", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<bool>("IsActive")
                .HasColumnType("boolean");

            b.Property<string>("PasswordHash")
                .IsRequired()
                .HasMaxLength(512)
                .HasColumnType("character varying(512)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Username")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.HasKey("Id");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "Username")
                .IsUnique();

            b.ToTable("usuarios");
        });

        modelBuilder.Entity("Pos.Domain.Entities.UserRole", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("RoleId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("UserId")
                .HasColumnType("uuid");

            b.HasKey("Id");

            b.HasIndex("RoleId");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "UserId", "RoleId")
                .IsUnique();

            b.HasIndex("UserId");

            b.ToTable("usuario_roles");
        });

        modelBuilder.Entity<Tenant>().HasData(SeedData.Tenant);
        modelBuilder.Entity<Sucursal>().HasData(SeedData.Sucursal);
        modelBuilder.Entity<User>().HasData(SeedData.AdminUser);
        modelBuilder.Entity<Role>().HasData(SeedData.Roles);
        modelBuilder.Entity<Permission>().HasData(SeedData.Permissions);
        modelBuilder.Entity<UserRole>().HasData(SeedData.UserRoles);
        modelBuilder.Entity<RolePermission>().HasData(SeedData.RolePermissions);
    }
}
