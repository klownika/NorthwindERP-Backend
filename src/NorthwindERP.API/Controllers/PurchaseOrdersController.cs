using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindERP.API.DTOs;
using NorthwindERP.API.Extensions;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.API.Controllers;

/// <summary>📥 Módulo 4 — Compras a Proveedores</summary>
[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseService _purchases;

    public PurchaseOrdersController(IPurchaseService purchases) => _purchases = purchases;

    /// <summary>Lista las órdenes de compra con sus detalles.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDto>>> GetAll(CancellationToken ct)
        => Ok((await _purchases.GetAllAsync(ct)).Select(p => p.ToDto()));

    /// <summary>Obtiene una orden de compra por id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PurchaseOrderDto>> GetById(int id, CancellationToken ct)
        => Ok((await _purchases.GetByIdAsync(id, ct)).ToDto());

    /// <summary>Emite una nueva orden de compra a un proveedor (estado inicial: Pending).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseOrderDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PurchaseOrderDto>> Create([FromBody] CreatePurchaseOrderRequest request, CancellationToken ct)
    {
        var command = new CreatePurchaseOrderCommand(
            request.SupplierId,
            User.GetEmployeeId(),
            request.RequiredDate,
            request.Items.Select(i => new PurchaseOrderLine(i.ProductId, i.Quantity, i.UnitPrice)).ToList());

        var created = await _purchases.CreateAsync(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.PurchaseOrderId }, created.ToDto());
    }

    /// <summary>
    /// Marca la orden como recibida (RPC receive_purchase_order):
    /// actualiza el estado, suma el stock e inserta el movimiento INBOUND en el Kardex.
    /// </summary>
    [HttpPut("{id:int}/receive")]
    [ProducesResponseType(typeof(PurchaseOrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PurchaseOrderDto>> Receive(int id, CancellationToken ct)
    {
        var received = await _purchases.ReceiveAsync(id, User.GetEmployeeId(), ct);
        return Ok(received.ToDto());
    }
}
