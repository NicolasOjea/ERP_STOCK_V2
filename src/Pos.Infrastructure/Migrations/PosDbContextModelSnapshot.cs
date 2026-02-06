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

        modelBuilder.Entity("Pos.Domain.Entities.Caja", b =>
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

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.ToTable("cajas");
        });

        modelBuilder.Entity("Pos.Domain.Entities.CajaMovimiento", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<Guid>("CajaSesionId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset>("Fecha")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("MedioPago")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<decimal>("Monto")
                .HasColumnType("numeric(18,4)");

            b.Property<string>("Motivo")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("character varying(500)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<int>("Tipo")
                .HasColumnType("integer");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("CajaSesionId");

            b.HasIndex("Fecha");

            b.HasIndex("MedioPago");

            b.HasIndex("TenantId");

            b.ToTable("caja_movimientos");
        });

        modelBuilder.Entity("Pos.Domain.Entities.CajaSesion", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("AperturaAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("ArqueoJson")
                .HasColumnType("jsonb");

            b.Property<Guid>("CajaId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset?>("CierreAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<decimal>("DiferenciaTotal")
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0m);

            b.Property<int>("Estado")
                .HasColumnType("integer");

            b.Property<decimal?>("MontoCierre")
                .HasColumnType("numeric(18,4)");

            b.Property<decimal>("MontoInicial")
                .HasColumnType("numeric(18,4)");

            b.Property<string>("MotivoDiferencia")
                .HasMaxLength(500)
                .HasColumnType("character varying(500)");

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("AperturaAt");

            b.HasIndex("CajaId");

            b.HasIndex("CajaId", "Estado")
                .IsUnique()
                .HasFilter("\"Estado\" = 0");

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.ToTable("caja_sesiones");
        });

        modelBuilder.Entity("Pos.Domain.Entities.Categoria", b =>
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

            b.ToTable("categorias");
        });

        modelBuilder.Entity("Pos.Domain.Entities.Marca", b =>
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

            b.ToTable("marcas");
        });

        modelBuilder.Entity("Pos.Domain.Entities.Proveedor", b =>
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

            b.ToTable("proveedores");
        });

        modelBuilder.Entity("Pos.Domain.Entities.ListaPrecio", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<bool>("IsActive")
                .HasColumnType("boolean");

            b.Property<string>("Nombre")
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("character varying(150)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "Nombre")
                .IsUnique();

            b.ToTable("listas_precio");
        });

        modelBuilder.Entity("Pos.Domain.Entities.ListaPrecioItem", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("ListaPrecioId")
                .HasColumnType("uuid");

            b.Property<decimal>("Precio")
                .HasColumnType("numeric(18,4)");

            b.Property<Guid>("ProductoId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("ListaPrecioId");

            b.HasIndex("ProductoId");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "ListaPrecioId", "ProductoId")
                .IsUnique();

            b.ToTable("lista_precio_items");
        });

        modelBuilder.Entity("Pos.Domain.Entities.DocumentoCompra", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTime>("Fecha")
                .HasColumnType("date");

            b.Property<string>("Numero")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<Guid?>("ProveedorId")
                .HasColumnType("uuid");

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("Fecha");

            b.HasIndex("Numero");

            b.HasIndex("ProveedorId");

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.ToTable("documentos_compra");
        });

        modelBuilder.Entity("Pos.Domain.Entities.DocumentoCompraItem", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<decimal>("Cantidad")
                .HasColumnType("numeric(18,4)");

            b.Property<decimal>("PrecioUnitario")
                .HasColumnType("numeric(18,4)");

            b.Property<string>("Codigo")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<decimal?>("CostoUnitario")
                .HasColumnType("numeric(18,4)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Descripcion")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("character varying(500)");

            b.Property<Guid>("DocumentoCompraId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("Codigo");

            b.HasIndex("DocumentoCompraId");

            b.HasIndex("TenantId");

            b.ToTable("documento_compra_items");
        });

        modelBuilder.Entity("Pos.Domain.Entities.PreRecepcion", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("DocumentoCompraId")
                .HasColumnType("uuid");

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("CreatedAt");

            b.HasIndex("DocumentoCompraId");

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.ToTable("pre_recepciones");
        });

        modelBuilder.Entity("Pos.Domain.Entities.PreRecepcionItem", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<decimal>("Cantidad")
                .HasColumnType("numeric(18,4)");

            b.Property<string>("Codigo")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<decimal?>("CostoUnitario")
                .HasColumnType("numeric(18,4)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Descripcion")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("character varying(500)");

            b.Property<Guid>("DocumentoCompraItemId")
                .HasColumnType("uuid");

            b.Property<int>("Estado")
                .HasColumnType("integer");

            b.Property<Guid>("PreRecepcionId")
                .HasColumnType("uuid");

            b.Property<Guid?>("ProductoId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("DocumentoCompraItemId");

            b.HasIndex("PreRecepcionId");

            b.HasIndex("ProductoId");

            b.HasIndex("TenantId");

            b.HasIndex("PreRecepcionId", "DocumentoCompraItemId")
                .IsUnique();

            b.ToTable("pre_recepcion_items");
        });

        modelBuilder.Entity("Pos.Domain.Entities.Recepcion", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("PreRecepcionId")
                .HasColumnType("uuid");

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("CreatedAt");

            b.HasIndex("PreRecepcionId")
                .IsUnique();

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.ToTable("recepciones");
        });

        modelBuilder.Entity("Pos.Domain.Entities.RecepcionItem", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<decimal>("Cantidad")
                .HasColumnType("numeric(18,4)");

            b.Property<string>("Codigo")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<decimal?>("CostoUnitario")
                .HasColumnType("numeric(18,4)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Descripcion")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("character varying(500)");

            b.Property<Guid>("PreRecepcionItemId")
                .HasColumnType("uuid");

            b.Property<Guid>("ProductoId")
                .HasColumnType("uuid");

            b.Property<Guid>("RecepcionId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("PreRecepcionItemId");

            b.HasIndex("ProductoId");

            b.HasIndex("RecepcionId");

            b.HasIndex("TenantId");

            b.HasIndex("RecepcionId", "PreRecepcionItemId")
                .IsUnique();

            b.ToTable("recepcion_items");
        });

        modelBuilder.Entity("Pos.Domain.Entities.OrdenCompra", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<int>("Estado")
                .HasColumnType("integer");

            b.Property<Guid?>("ProveedorId")
                .HasColumnType("uuid");

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("CreatedAt");

            b.HasIndex("Estado");

            b.HasIndex("ProveedorId");

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.ToTable("ordenes_compra");
        });

        modelBuilder.Entity("Pos.Domain.Entities.OrdenCompraItem", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<decimal>("Cantidad")
                .HasColumnType("numeric(18,4)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("OrdenCompraId")
                .HasColumnType("uuid");

            b.Property<Guid>("ProductoId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("OrdenCompraId");

            b.HasIndex("ProductoId");

            b.HasIndex("TenantId");

            b.HasIndex("OrdenCompraId", "ProductoId")
                .IsUnique();

            b.ToTable("orden_compra_items");
        });

        modelBuilder.Entity("Pos.Domain.Entities.Venta", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<int>("Estado")
                .HasColumnType("integer");

            b.Property<string>("ListaPrecio")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<decimal>("TotalNeto")
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0m);

            b.Property<decimal>("TotalPagos")
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0m);

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid?>("UserId")
                .HasColumnType("uuid");

            b.HasKey("Id");

            b.HasIndex("CreatedAt");

            b.HasIndex("Estado");

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.HasIndex("UserId");

            b.ToTable("ventas");
        });

        modelBuilder.Entity("Pos.Domain.Entities.VentaItem", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<decimal>("Cantidad")
                .HasColumnType("numeric(18,4)");

            b.Property<string>("Codigo")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("ProductoId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("VentaId")
                .HasColumnType("uuid");

            b.HasKey("Id");

            b.HasIndex("ProductoId");

            b.HasIndex("TenantId");

            b.HasIndex("VentaId");

            b.HasIndex("VentaId", "ProductoId")
                .IsUnique();

            b.ToTable("venta_items");
        });

        modelBuilder.Entity("Pos.Domain.Entities.VentaPago", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("MedioPago")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<decimal>("Monto")
                .HasColumnType("numeric(18,4)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("VentaId")
                .HasColumnType("uuid");

            b.HasKey("Id");

            b.HasIndex("MedioPago");

            b.HasIndex("TenantId");

            b.HasIndex("VentaId");

            b.ToTable("venta_pagos");
        });

        modelBuilder.Entity("Pos.Domain.Entities.Producto", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<Guid?>("CategoriaId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<bool>("IsActive")
                .HasColumnType("boolean");

            b.Property<Guid?>("MarcaId")
                .HasColumnType("uuid");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(250)
                .HasColumnType("character varying(250)");

            b.Property<Guid?>("ProveedorId")
                .HasColumnType("uuid");

            b.Property<decimal>("PrecioBase")
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(1m);

            b.Property<string>("Sku")
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnType("character varying(80)");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("CategoriaId");

            b.HasIndex("MarcaId");

            b.HasIndex("ProveedorId");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "Sku")
                .IsUnique();

            b.ToTable("productos");
        });

        modelBuilder.Entity("Pos.Domain.Entities.ProductoProveedor", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<bool>("EsPrincipal")
                .HasColumnType("boolean");

            b.Property<Guid>("ProductoId")
                .HasColumnType("uuid");

            b.Property<Guid>("ProveedorId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("ProductoId");

            b.HasIndex("ProveedorId");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "ProductoId")
                .IsUnique()
                .HasFilter("\"EsPrincipal\" = true");

            b.HasIndex("TenantId", "ProductoId", "ProveedorId")
                .IsUnique();

            b.ToTable("producto_proveedor");
        });

        modelBuilder.Entity("Pos.Domain.Entities.ProductoCodigo", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<string>("Codigo")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("ProductoId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("ProductoId");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "Codigo")
                .IsUnique();

            b.ToTable("producto_codigos");
        });

        modelBuilder.Entity("Pos.Domain.Entities.ProductoStockConfig", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("ProductoId")
                .HasColumnType("uuid");

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<decimal>("StockMinimo")
                .HasColumnType("numeric(18,4)");

            b.Property<decimal>("ToleranciaPct")
                .HasColumnType("numeric(6,2)")
                .HasDefaultValue(1.25m);

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "ProductoId", "SucursalId")
                .IsUnique();

            b.ToTable("producto_stock_config");
        });

        modelBuilder.Entity("Pos.Domain.Entities.StockSaldo", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<decimal>("CantidadActual")
                .HasColumnType("numeric(18,4)")
                .HasDefaultValue(0m);

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("ProductoId")
                .HasColumnType("uuid");

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.HasIndex("TenantId", "ProductoId", "SucursalId")
                .IsUnique();

            b.ToTable("stock_saldos");
        });

        modelBuilder.Entity("Pos.Domain.Entities.StockMovimiento", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset>("Fecha")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Motivo")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("character varying(500)");

            b.Property<Guid>("SucursalId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<int>("Tipo")
                .HasColumnType("integer");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("Fecha");

            b.HasIndex("SucursalId");

            b.HasIndex("TenantId");

            b.ToTable("stock_movimientos");
        });

        modelBuilder.Entity("Pos.Domain.Entities.StockMovimientoItem", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("uuid");

            b.Property<decimal>("Cantidad")
                .HasColumnType("numeric(18,4)");

            b.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<bool>("EsIngreso")
                .HasColumnType("boolean");

            b.Property<Guid>("MovimientoId")
                .HasColumnType("uuid");

            b.Property<Guid>("ProductoId")
                .HasColumnType("uuid");

            b.Property<Guid>("TenantId")
                .HasColumnType("uuid");

            b.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("MovimientoId");

            b.HasIndex("ProductoId");

            b.HasIndex("TenantId");

            b.ToTable("stock_movimiento_items");
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
