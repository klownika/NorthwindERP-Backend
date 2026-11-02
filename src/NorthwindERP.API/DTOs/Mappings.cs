using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Models;

namespace NorthwindERP.API.DTOs;

/// <summary>Convierte Entidad -> DTO (y DTO -> Entidad). Mapeo manual: simple y sin librerías.</summary>
public static class Mappings
{
    public static ProductDto ToDto(this Product p) => new(
        p.ProductId, p.ProductName,
        p.CategoryId, p.Category?.CategoryName,
        p.SupplierId, p.Supplier?.SupplierName,
        p.Unit, p.Price);

    public static CategoryDto ToDto(this Category c) => new(c.CategoryId, c.CategoryName, c.Description);

    public static WarehouseDto ToDto(this Warehouse w) => new(w.WarehouseId, w.WarehouseName, w.Location, w.ManagerEmployeeId);

    public static StockDto ToDto(this InventoryStock s)
    {
        var quantity = s.Quantity ?? 0;
        var min = s.MinStockLevel ?? 0;
        return new StockDto(
            s.StockId, s.WarehouseId, s.Warehouse?.WarehouseName,
            s.ProductId, s.Product?.ProductName,
            quantity, min, IsLowStock: quantity <= min);
    }

    public static KardexEntryDto ToDto(this InventoryTransaction t) => new(
        t.TransactionId, t.TransactionDate, t.TransactionType,
        t.WarehouseId, t.Warehouse?.WarehouseName,
        t.ProductId, t.Product?.ProductName,
        t.Quantity,
        t.EmployeeId, t.Employee is null ? null : $"{t.Employee.FirstName} {t.Employee.LastName}",
        t.ReferenceId);

    public static CustomerDto ToDto(this Customer c) => new(
        c.CustomerId, c.CustomerName, c.ContactName, c.Address, c.City, c.PostalCode, c.Country);

    public static EmployeeDto ToDto(this Employee e) => new(
        e.EmployeeId, e.FirstName, e.LastName, $"{e.FirstName} {e.LastName}", e.BirthDate, e.Notes);

    public static ShipperDto ToDto(this Shipper s) => new(s.ShipperId, s.ShipperName, s.Phone);

    public static SupplierDto ToDto(this Supplier s) => new(
        s.SupplierId, s.SupplierName, s.ContactName, s.Address, s.City, s.Country, s.Phone);

    public static PurchaseOrderDto ToDto(this PurchaseOrder po)
    {
        var items = po.Details
            .Select(d => new PurchaseOrderItemDto(
                d.ProductId, d.Product?.ProductName, d.Quantity, d.UnitPrice,
                Subtotal: (d.Quantity ?? 0) * (d.UnitPrice ?? 0m)))
            .ToList();

        return new PurchaseOrderDto(
            po.PurchaseOrderId, po.SupplierId, po.Supplier?.SupplierName, po.EmployeeId,
            po.OrderDate, po.RequiredDate, po.Status,
            Total: items.Sum(i => i.Subtotal),
            Items: items);
    }

    public static LoginResponse ToDto(this LoginResult r) => new(
        r.Token, r.ExpiresAtUtc, r.UserId, r.Username, r.Role, r.EmployeeId, r.MustChangePassword);

    // DTO -> Entidad
    public static Customer ToEntity(this NewCustomerRequest r) => new()
    {
        CustomerName = r.CustomerName.Trim(),
        ContactName = r.ContactName?.Trim(),
        Address = r.Address?.Trim(),
        City = r.City?.Trim(),
        PostalCode = r.PostalCode?.Trim(),
        Country = r.Country?.Trim()
    };
}
