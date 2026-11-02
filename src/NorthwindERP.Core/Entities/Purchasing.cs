namespace NorthwindERP.Core.Entities;

/// <summary>Valores de purchaseorders.status. Deben coincidir con los que usa tu BD / función RPC.</summary>
public static class PurchaseOrderStatus
{
    public const string Pending = "Pending";
    public const string Received = "Received";
}

/// <summary>Tabla: purchaseorders</summary>
public class PurchaseOrder
{
    public int PurchaseOrderId { get; set; }
    public int? SupplierId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string? Status { get; set; }

    public Supplier? Supplier { get; set; }
    public List<PurchaseOrderDetail> Details { get; set; } = new();
}

/// <summary>Tabla: purchaseorderdetails (PK compuesta: purchaseorderid + productid)</summary>
public class PurchaseOrderDetail
{
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? Quantity { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }
    public Product? Product { get; set; }
}

/// <summary>Tabla: payments</summary>
public class Payment
{
    public int PaymentId { get; set; }
    public int? PurchaseOrderId { get; set; }
    public int? OrderId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentMethod { get; set; }
}
