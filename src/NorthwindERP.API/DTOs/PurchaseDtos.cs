using System.ComponentModel.DataAnnotations;

namespace NorthwindERP.API.DTOs;

/// <summary>Cuerpo de POST /api/purchase-orders. El empleado se toma del token.</summary>
public class CreatePurchaseOrderRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "supplierId es obligatorio.")]
    public int SupplierId { get; set; }

    public DateTime? RequiredDate { get; set; }

    [Required, MinLength(1, ErrorMessage = "La orden debe tener al menos un producto.")]
    public List<PurchaseOrderItemRequest> Items { get; set; } = new();
}

public class PurchaseOrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1_000_000)]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal UnitPrice { get; set; }
}

public record PurchaseOrderItemDto(int ProductId, string? ProductName, int? Quantity, decimal? UnitPrice, decimal Subtotal);

public record PurchaseOrderDto(
    int PurchaseOrderId,
    int? SupplierId,
    string? SupplierName,
    int? EmployeeId,
    DateTime? OrderDate,
    DateTime? RequiredDate,
    string? Status,
    decimal Total,
    IReadOnlyList<PurchaseOrderItemDto> Items);
