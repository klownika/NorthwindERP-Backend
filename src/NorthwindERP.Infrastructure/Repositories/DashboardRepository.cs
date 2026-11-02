using Dapper;
using Microsoft.EntityFrameworkCore;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;
using NorthwindERP.Infrastructure.Data;

namespace NorthwindERP.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private const string ReceivedStatus = "RECEIVED"; // se compara en mayúsculas

    private readonly NorthwindDbContext _db;

    public DashboardRepository(NorthwindDbContext db) => _db = db;

    public async Task<DashboardKpis> GetKpisAsync(CancellationToken ct = default)
    {
        // Ventas = SUM(orderdetails.quantity * products.price)
        // (orderdetails no guarda precio histórico, se usa el precio actual del producto)
        var totalSales = await _db.OrderDetails
            .SumAsync(d => d.Quantity * d.Product!.Price, ct) ?? 0m;

        var unitsSold = await _db.OrderDetails.SumAsync(d => d.Quantity, ct) ?? 0;

        var totalOrders = await _db.Orders.CountAsync(ct);

        // Inversión = compras ya recibidas
        var totalInvested = await _db.PurchaseOrderDetails
            .Where(d => d.PurchaseOrder!.Status != null && d.PurchaseOrder.Status.ToUpper() == ReceivedStatus)
            .SumAsync(d => d.Quantity * d.UnitPrice, ct) ?? 0m;

        var lowStockItems = await _db.InventoryStock
            .CountAsync(s => s.Quantity <= s.MinStockLevel, ct);

        return new DashboardKpis(
            TotalSales: totalSales,
            UnitsSold: unitsSold,
            TotalOrders: totalOrders,
            TotalInvested: totalInvested,
            EstimatedProfit: totalSales - totalInvested,
            LowStockItems: lowStockItems);
    }

    public async Task<IReadOnlyList<MonthlyAmount>> GetSalesVsInvestmentAsync(CancellationToken ct = default)
    {
        // GROUP BY año, mes (se traduce a date_part en PostgreSQL)
        var sales = await _db.OrderDetails
            .Where(d => d.Order!.OrderDate != null)
            .GroupBy(d => new { d.Order!.OrderDate!.Value.Year, d.Order!.OrderDate!.Value.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(d => d.Quantity * d.Product!.Price) ?? 0m
            })
            .ToListAsync(ct);

        var investment = await _db.PurchaseOrderDetails
            .Where(d => d.PurchaseOrder!.OrderDate != null
                        && d.PurchaseOrder.Status != null
                        && d.PurchaseOrder.Status.ToUpper() == ReceivedStatus)
            .GroupBy(d => new { d.PurchaseOrder!.OrderDate!.Value.Year, d.PurchaseOrder!.OrderDate!.Value.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Total = g.Sum(d => d.Quantity * d.UnitPrice) ?? 0m
            })
            .ToListAsync(ct);

        // Une ambas series por (año, mes) para el gráfico "Ventas vs. Inversión"
        var months = sales.Select(s => (s.Year, s.Month))
            .Union(investment.Select(i => (i.Year, i.Month)))
            .OrderBy(m => m.Year).ThenBy(m => m.Month);

        return months
            .Select(m => new MonthlyAmount(
                m.Year,
                m.Month,
                sales.Where(s => s.Year == m.Year && s.Month == m.Month).Sum(s => s.Total),
                investment.Where(i => i.Year == m.Year && i.Month == m.Month).Sum(i => i.Total)))
            .ToList();
    }

    public async Task<IReadOnlyList<QueryRow>> GetCustomerSalesSummaryAsync(CancellationToken ct = default)
    {
        var rows = await _db.Database.GetDbConnection().QueryAsync(
            new CommandDefinition("SELECT * FROM vw_customer_sales_summary", cancellationToken: ct));

        return rows.ToQueryRows();
    }
}
