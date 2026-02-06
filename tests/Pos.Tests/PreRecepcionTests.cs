using System.Net;
using System.Net.Http.Json;
using Pos.Application.DTOs.DocumentosCompra;
using Pos.Application.DTOs.PreRecepciones;
using Pos.Application.DTOs.Products;
using Pos.Domain.Enums;
using Xunit;

namespace Pos.Tests;

public sealed class PreRecepcionTests : IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;

    public PreRecepcionTests(WebApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PreRecepcion_crea_items_con_match_ok()
    {
        await _factory.EnsureDatabaseMigratedAsync();

        var client = _factory.CreateClient();
        var token = _factory.CreateTokenWithPermissions(
            PermissionCodes.ComprasRegistrar,
            PermissionCodes.ProductoEditar,
            PermissionCodes.ProductoVer);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var sku = $"SKU-{Guid.NewGuid():N}";
        var productResponse = await client.PostAsJsonAsync("/api/v1/productos", new ProductCreateDto(
            $"Producto {Guid.NewGuid():N}",
            sku,
            null,
            null,
            null,
            true));

        Assert.Equal(HttpStatusCode.Created, productResponse.StatusCode);
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDetailDto>();
        Assert.NotNull(product);

        var codigo = $"COD-{Guid.NewGuid():N}";
        var addCode = await client.PostAsJsonAsync(
            $"/api/v1/productos/{product!.Id}/codigos",
            new ProductCodeCreateDto(codigo));
        Assert.Equal(HttpStatusCode.Created, addCode.StatusCode);

        var parsePayload = new
        {
            proveedorId = (string?)null,
            numero = $"REM-{Guid.NewGuid():N}",
            fecha = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            items = new[]
            {
                new { codigo, descripcion = "Item", cantidad = 1m }
            }
        };

        var parseResponse = await client.PostAsJsonAsync("/api/v1/documentos-compra/parse", parsePayload);
        Assert.Equal(HttpStatusCode.Created, parseResponse.StatusCode);
        var parseResult = await parseResponse.Content.ReadFromJsonAsync<DocumentoCompraParseResultDto>();
        Assert.NotNull(parseResult);

        var preResponse = await client.PostAsJsonAsync("/api/v1/pre-recepciones", new PreRecepcionCreateDto(
            parseResult!.DocumentoCompraId));

        Assert.Equal(HttpStatusCode.Created, preResponse.StatusCode);
        var preRecepcion = await preResponse.Content.ReadFromJsonAsync<PreRecepcionDto>();
        Assert.NotNull(preRecepcion);
        Assert.Single(preRecepcion!.Items);

        var item = preRecepcion.Items[0];
        Assert.Equal("OK", item.Estado);
        Assert.Equal(product.Id, item.ProductoId);
    }
}
