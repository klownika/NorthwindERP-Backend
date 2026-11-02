using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.API.Controllers;

/// <summary>📊 Módulo 5 — Dashboard y Analítica</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    /// <summary>
    /// KPIs para las tarjetas (ventas, unidades, inversión, ganancia estimada, stock bajo)
    /// + ventas por cliente desde la vista vw_customer_sales_summary.
    /// </summary>
    [HttpGet("kpis")]
    public async Task<ActionResult<DashboardResult>> GetKpis(CancellationToken ct)
        => Ok(await _dashboard.GetKpisAsync(ct));

    /// <summary>Serie mensual Ventas vs. Inversión para el gráfico (Recharts / Chart.js).</summary>
    [HttpGet("sales-vs-investment")]
    public async Task<ActionResult<IEnumerable<MonthlyAmount>>> GetSalesVsInvestment(CancellationToken ct)
        => Ok(await _dashboard.GetSalesVsInvestmentAsync(ct));
}
