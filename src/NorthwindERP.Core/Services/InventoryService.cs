using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Exceptions;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.Core.Services;

public class InventoryService : IInventoryService
{
    private readonly IProductRepository _products;
    private readonly IInventoryRepository _inventory;

    public InventoryService(IProductRepository products, IInventoryRepository inventory)
    {
        _products = products;
        _inventory = inventory;
    }

    public Task<IReadOnlyList<Product>> GetProductsAsync(string? search, int? categoryId, CancellationToken ct = default)
        => _products.GetProductsAsync(search, categoryId, ct);

    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default)
        => _products.GetCategoriesAsync(ct);

    public Task<IReadOnlyList<Warehouse>> GetWarehousesAsync(CancellationToken ct = default)
        => _inventory.GetWarehousesAsync(ct);

    public Task<IReadOnlyList<InventoryStock>> GetStockAsync(StockFilter filter, CancellationToken ct = default)
        => _inventory.GetStockAsync(filter, ct);

    public Task<IReadOnlyList<InventoryTransaction>> GetKardexAsync(KardexFilter filter, CancellationToken ct = default)
    {
        if (filter.From.HasValue && filter.To.HasValue && filter.From > filter.To)
            throw new BusinessRuleException("La fecha 'from' no puede ser mayor que 'to'.");

        if (filter.Take is < 1 or > 2000)
            throw new BusinessRuleException("'take' debe estar entre 1 y 2000.");

        return _inventory.GetKardexAsync(filter, ct);
    }
}
