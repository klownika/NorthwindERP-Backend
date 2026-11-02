using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindERP.API.DTOs;
using NorthwindERP.API.Extensions;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.API.Controllers;

/// <summary>🛒 Módulo 2 — Órdenes y Venta</summary>
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    /// <summary>
    /// Registra un pedido completo (cliente nuevo o existente + detalles) en UNA transacción.
    /// Llama a la RPC register_customer_order: valida stock, descuenta inventario y genera OUTBOUND en el Kardex.
    /// </summary>
    [HttpPost("with-customer")]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateOrderResponse>> CreateWithCustomer(
        [FromBody] CreateOrderWithCustomerRequest request, CancellationToken ct)
    {
        var command = new CreateOrderCommand(
            CustomerId: request.CustomerId,
            NewCustomer: request.NewCustomer?.ToEntity(),
            EmployeeId: User.GetEmployeeId(), // del token, no del body
            ShipperId: request.ShipperId,
            Items: request.Items.Select(i => new OrderLineItem(i.ProductId, i.Quantity)).ToList());

        var result = await _orders.CreateWithCustomerAsync(command, ct);

        return CreatedAtAction(
            nameof(GetCustomerHistory),
            new { customerId = result.CustomerId },
            new CreateOrderResponse(result.CustomerId, result.RpcResult));
    }

    /// <summary>Historial de pedidos de un cliente (RPC get_customer_order_history).</summary>
    [HttpGet("customer/{customerId:int}")]
    public async Task<ActionResult<IEnumerable<QueryRow>>> GetCustomerHistory(int customerId, CancellationToken ct)
        => Ok(await _orders.GetCustomerHistoryAsync(customerId, ct));

    /// <summary>Todos los pedidos con productos, empleado, transportista y total por línea (vista vw_order_details_extended).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QueryRow>>> GetAll(CancellationToken ct)
        => Ok(await _orders.GetAllAsync(ct));

    /// <summary>Elimina un pedido. Sus detalles se borran por ON DELETE CASCADE.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _orders.DeleteAsync(id, ct);
        return NoContent();
    }
}
