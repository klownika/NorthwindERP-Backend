using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindERP.API.DTOs;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.API.Controllers;

/// <summary>📦 Módulo 1 — Inventario, Kardex y Almacenes</summary>
[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventory;

    public InventoryController(IInventoryService inventory) => _inventory = inventory;

    /// <summary>Stock físico actual por almacén (tabla inventorystock).</summary>
    /// <param name="warehouseId">Filtra por almacén.</param>
    /// <param name="productId">Filtra por producto.</param>
    /// <param name="lowStockOnly">true = solo productos con stock &lt;= min_stock_level.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpGet("stock")]
    public async Task<ActionResult<IEnumerable<StockDto>>> GetStock(
        [FromQuery] int? warehouseId,
        [FromQuery] int? productId,
        [FromQuery] bool lowStockOnly,
        CancellationToken ct)
    {
        var stock = await _inventory.GetStockAsync(new StockFilter(warehouseId, productId, lowStockOnly), ct);
        return Ok(stock.Select(s => s.ToDto()));
    }

    /// <summary>Historial de movimientos del Kardex (tabla inventorytransactions).</summary>
    /// <param name="productId">Filtra por producto.</param>
    /// <param name="warehouseId">Filtra por almacén.</param>
    /// <param name="type">INBOUND, OUTBOUND o RESERVATION.</param>
    /// <param name="from">Fecha inicial (yyyy-MM-dd).</param>
    /// <param name="to">Fecha final (yyyy-MM-dd, inclusive).</param>
    /// <param name="take">Máximo de registros (por defecto 500).</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpGet("kardex")]
    public async Task<ActionResult<IEnumerable<KardexEntryDto>>> GetKardex(
        [FromQuery] int? productId,
        [FromQuery] int? warehouseId,
        [FromQuery] string? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct,
        [FromQuery] int take = 500)
    {
        var filter = new KardexFilter(productId, warehouseId, type, from, to, take);
        var entries = await _inventory.GetKardexAsync(filter, ct);
        return Ok(entries.Select(e => e.ToDto()));
    }

    /// <summary>Lista los almacenes / bodegas.</summary>
    [HttpGet("warehouses")]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetWarehouses(CancellationToken ct)
    {
        var warehouses = await _inventory.GetWarehousesAsync(ct);
        return Ok(warehouses.Select(w => w.ToDto()));
    }
}
