using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.Core.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboard;

    public DashboardService(IDashboardRepository dashboard) => _dashboard = dashboard;

    public async Task<DashboardResult> GetKpisAsync(CancellationToken ct = default)
    {
        // Se ejecutan una tras otra: un DbContext no admite consultas en paralelo.
        var kpis = await _dashboard.GetKpisAsync(ct);
        var byCustomer = await _dashboard.GetCustomerSalesSummaryAsync(ct);
        return new DashboardResult(kpis, byCustomer);
    }

    public Task<IReadOnlyList<MonthlyAmount>> GetSalesVsInvestmentAsync(CancellationToken ct = default)
        => _dashboard.GetSalesVsInvestmentAsync(ct);
}
