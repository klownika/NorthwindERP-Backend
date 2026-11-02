namespace NorthwindERP.Core.Entities;

/// <summary>Tabla: warehouses</summary>
public class Warehouse
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int? ManagerEmployeeId { get; set; }
}

/// <summary>Tabla: inventorystock (stock físico por almacén y producto)</summary>
public class InventoryStock
{
    public int StockId { get; set; }
    public int? WarehouseId { get; set; }
    public int? ProductId { get; set; }
    public int? Quantity { get; set; }
    public int? MinStockLevel { get; set; }

    public Warehouse? Warehouse { get; set; }
    public Product? Product { get; set; }
}

/// <summary>Tabla: inventorytransactions (Kardex)</summary>
public class InventoryTransaction
{
    public int TransactionId { get; set; }
    public int? WarehouseId { get; set; }
    public int? ProductId { get; set; }
    public int? EmployeeId { get; set; }

    /// <summary>INBOUND, OUTBOUND, RESERVATION, ...</summary>
    public string? TransactionType { get; set; }
    public int? Quantity { get; set; }
    public DateTime? TransactionDate { get; set; }

    /// <summary>Id del documento origen (pedido u orden de compra).</summary>
    public int? ReferenceId { get; set; }

    public Warehouse? Warehouse { get; set; }
    public Product? Product { get; set; }
    public Employee? Employee { get; set; }
}
