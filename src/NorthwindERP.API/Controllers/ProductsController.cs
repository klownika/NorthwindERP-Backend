using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindERP.API.DTOs;
using NorthwindERP.Core.Interfaces;

namespace NorthwindERP.API.Controllers;

/// <summary>📦 Módulo 1 — Catálogo de productos</summary>
[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IInventoryService _inventory;

    public ProductsController(IInventoryService inventory) => _inventory = inventory;

    /// <summary>Lista productos con su categoría, proveedor y precio.</summary>
    /// <param name="search">Texto a buscar en el nombre (opcional).</param>
    /// <param name="categoryId">Filtra por categoría (opcional).</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
        [FromQuery] string? search, [FromQuery] int? categoryId, CancellationToken ct)
    {
        var products = await _inventory.GetProductsAsync(search, categoryId, ct);
        return Ok(products.Select(p => p.ToDto()));
    }

    /// <summary>Lista las categorías (para el filtro del catálogo).</summary>
    [HttpGet("/api/categories")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(CancellationToken ct)
    {
        var categories = await _inventory.GetCategoriesAsync(ct);
        return Ok(categories.Select(c => c.ToDto()));
    }
}
