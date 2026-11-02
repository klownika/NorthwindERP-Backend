namespace NorthwindERP.Core.Entities;

/// <summary>Tabla: customers</summary>
public class Customer
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    // NOTA: la tabla customers de tu diagrama NO tiene columna "phone".
    // Si la agregas en Supabase (ALTER TABLE customers ADD COLUMN phone varchar(24);)
    // descomenta esta línea y la del DTO/servicio de actualización.
    // public string? Phone { get; set; }
}

/// <summary>Tabla: employees</summary>
public class Employee
{
    public int EmployeeId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string? Photo { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Tabla: shippers</summary>
public class Shipper
{
    public int ShipperId { get; set; }
    public string ShipperName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

/// <summary>Tabla: orders</summary>
public class Order
{
    public int OrderId { get; set; }
    public int? CustomerId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime? OrderDate { get; set; }
    public int? ShipperId { get; set; }

    public Customer? Customer { get; set; }
    public Employee? Employee { get; set; }
    public Shipper? Shipper { get; set; }
    public List<OrderDetail> OrderDetails { get; set; } = new();
}

/// <summary>Tabla: orderdetails</summary>
public class OrderDetail
{
    public int OrderDetailId { get; set; }
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
    public int? Quantity { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
