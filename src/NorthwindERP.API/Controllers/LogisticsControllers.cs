using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindERP.API.DTOs;
using NorthwindERP.Core.Interfaces;

namespace NorthwindERP.API.Controllers;

// 👥 Módulo 3 — Empleados, Transportistas y Proveedores (solo lectura)

/// <summary>Empleados (para asociarlos a órdenes y compras).</summary>
[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly ILogisticsService _logistics;
    public EmployeesController(ILogisticsService logistics) => _logistics = logistics;

    /// <summary>Lista los empleados.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll(CancellationToken ct)
        => Ok((await _logistics.GetEmployeesAsync(ct)).Select(e => e.ToDto()));
}

/// <summary>Empresas de transporte (shippers).</summary>
[ApiController]
[Route("api/shippers")]
[Authorize]
public class ShippersController : ControllerBase
{
    private readonly ILogisticsService _logistics;
    public ShippersController(ILogisticsService logistics) => _logistics = logistics;

    /// <summary>Lista los transportistas.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShipperDto>>> GetAll(CancellationToken ct)
        => Ok((await _logistics.GetShippersAsync(ct)).Select(s => s.ToDto()));
}

/// <summary>Proveedores (para el formulario de orden de compra).</summary>
[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ILogisticsService _logistics;
    public SuppliersController(ILogisticsService logistics) => _logistics = logistics;

    /// <summary>Lista los proveedores.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll(CancellationToken ct)
        => Ok((await _logistics.GetSuppliersAsync(ct)).Select(s => s.ToDto()));
}
