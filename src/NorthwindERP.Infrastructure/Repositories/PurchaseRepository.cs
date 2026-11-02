using Dapper;
using Microsoft.EntityFrameworkCore;
using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;
using NorthwindERP.Infrastructure.Data;

namespace NorthwindERP.Infrastructure.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly NorthwindDbContext _db;

    public PurchaseRepository(NorthwindDbContext db) => _db = db;

    public async Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(CancellationToken ct = default)
        => await BaseQuery()
            .OrderByDescending(p => p.OrderDate)
            .ThenByDescending(p => p.PurchaseOrderId)
            .ToListAsync(ct);

    public Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken ct = default)
        => BaseQuery().FirstOrDefaultAsync(p => p.PurchaseOrderId == id, ct);

    public async Task<int> AddAsync(PurchaseOrder purchaseOrder, CancellationToken ct = default)
    {
        // EF inserta la cabecera y sus detalles en una sola transacción
        // y rellena purchaseorderid en los detalles automáticamente.
        _db.PurchaseOrders.Add(purchaseOrder);
        await _db.SaveChangesAsync(ct);
        _db.ChangeTracker.Clear(); // para que la siguiente lectura venga fresca de la BD
        return purchaseOrder.PurchaseOrderId;
    }

    public async Task<IReadOnlyList<QueryRow>> ReceiveAsync(int purchaseOrderId, int employeeId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT * FROM receive_purchase_order(
                p_purchase_order_id => @PurchaseOrderId,
                p_employee_id       => @EmployeeId)
            """;

        var rows = await _db.Database.GetDbConnection().QueryAsync(new CommandDefinition(
            sql,
            new { PurchaseOrderId = purchaseOrderId, EmployeeId = employeeId },
            cancellationToken: ct));

        return rows.ToQueryRows();
    }

    private IQueryable<PurchaseOrder> BaseQuery()
        => _db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Details)
                .ThenInclude(d => d.Product);
}
