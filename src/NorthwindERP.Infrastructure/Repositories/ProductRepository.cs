using Microsoft.EntityFrameworkCore;
using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Infrastructure.Data;

namespace NorthwindERP.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly NorthwindDbContext _db;

    public ProductRepository(NorthwindDbContext db) => _db = db;

    public async Task<IReadOnlyList<Product>> GetProductsAsync(string? search, int? categoryId, CancellationToken ct = default)
    {
        // AsNoTracking: solo lectura, más rápido.
        // Include: hace el JOIN con categories y suppliers.
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.ProductName, pattern)); // ILIKE = sin distinguir mayúsculas
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        return await query.OrderBy(p => p.ProductName).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default)
        => await _db.Categories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync(ct);
}
