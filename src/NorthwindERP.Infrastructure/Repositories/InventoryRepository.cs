using Microsoft.EntityFrameworkCore;
using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;
using NorthwindERP.Infrastructure.Data;

namespace NorthwindERP.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly NorthwindDbContext _db;

    public InventoryRepository(NorthwindDbContext db) => _db = db;

    public async Task<IReadOnlyList<InventoryStock>> GetStockAsync(StockFilter filter, CancellationToken ct = default)
    {
        var query = _db.InventoryStock
            .AsNoTracking()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .AsQueryable();

        if (filter.WarehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == filter.WarehouseId);

        if (filter.ProductId.HasValue)
            query = query.Where(s => s.ProductId == filter.ProductId);

        if (filter.LowStockOnly)
            query = query.Where(s => s.Quantity <= s.MinStockLevel);

        return await query
            .OrderBy(s => s.Warehouse!.WarehouseName)
            .ThenBy(s => s.Product!.ProductName)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryTransaction>> GetKardexAsync(KardexFilter filter, CancellationToken ct = default)
    {
        var query = _db.InventoryTransactions
            .AsNoTracking()
            .Include(t => t.Product)
            .Include(t => t.Warehouse)
            .Include(t => t.Employee)
            .AsQueryable();

        if (filter.ProductId.HasValue)
            query = query.Where(t => t.ProductId == filter.ProductId);

        if (filter.WarehouseId.HasValue)
            query = query.Where(t => t.WarehouseId == filter.WarehouseId);

        if (!string.IsNullOrWhiteSpace(filter.TransactionType))
        {
            var type = filter.TransactionType.Trim().ToUpper();
            query = query.Where(t => t.TransactionType != null && t.TransactionType.ToUpper() == type);
        }

        if (filter.From.HasValue)
        {
            var from = filter.From.Value.Date;
            query = query.Where(t => t.TransactionDate >= from);
        }

        if (filter.To.HasValue)
        {
            var toExclusive = filter.To.Value.Date.AddDays(1); // incluye todo el día "to"
            query = query.Where(t => t.TransactionDate < toExclusive);
        }

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.TransactionId)
            .Take(filter.Take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Warehouse>> GetWarehousesAsync(CancellationToken ct = default)
        => await _db.Warehouses.AsNoTracking().OrderBy(w => w.WarehouseName).ToListAsync(ct);
}
