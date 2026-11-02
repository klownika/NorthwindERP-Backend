using System.ComponentModel.DataAnnotations;
using NorthwindERP.Core.Models;

namespace NorthwindERP.API.DTOs;

/// <summary>
/// Cuerpo de POST /api/orders/with-customer.
/// Envía "customerId" (cliente existente) O "newCustomer" (cliente nuevo), no ambos.
/// El empleado se toma del token JWT del usuario logueado.
/// </summary>
public class CreateOrderWithCustomerRequest
{
    public int? CustomerId { get; set; }

    public NewCustomerRequest? NewCustomer { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "shipperId es obligatorio.")]
    public int ShipperId { get; set; }

    [Required, MinLength(1, ErrorMessage = "El pedido debe tener al menos un producto.")]
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class NewCustomerRequest
{
    [Required, StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [StringLength(100)] public string? ContactName { get; set; }
    [StringLength(150)] public string? Address { get; set; }
    [StringLength(60)] public string? City { get; set; }
    [StringLength(20)] public string? PostalCode { get; set; }
    [StringLength(60)] public string? Country { get; set; }
}

public class OrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 100_000)]
    public int Quantity { get; set; }
}

/// <summary>Respuesta al crear el pedido: id del cliente + lo que devolvió la RPC.</summary>
public record CreateOrderResponse(int CustomerId, IReadOnlyList<QueryRow> RpcResult);
