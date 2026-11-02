using NorthwindERP.Core.Entities;

namespace NorthwindERP.Core.Models;

// Modelos de dominio que NO son tablas: filtros, comandos y resultados.

/// <summary>
/// Una fila devuelta por una vista o función RPC de Supabase (columna -> valor).
/// Se usa cuando las columnas las define la BD (vistas/RPC) y no una entidad.
/// Se serializa a JSON como un objeto normal: { "orderid": 1, "productname": "..." }.
/// </summary>
public sealed class QueryRow : Dictionary<string, object?>
{
    public QueryRow() : base(StringComparer.OrdinalIgnoreCase) { }
}

// ---------- Inventario ----------
public record StockFilter(int? WarehouseId, int? ProductId, bool LowStockOnly);

public record KardexFilter(
    int? ProductId,
    int? WarehouseId,
    string? TransactionType,
    DateTime? From,
    DateTime? To,
    int Take = 500);

// ---------- Órdenes ----------
public record OrderLineItem(int ProductId, int Quantity);

public record CreateOrderCommand(
    int? CustomerId,          // cliente existente...
    Customer? NewCustomer,    // ...o cliente nuevo (exactamente uno de los dos)
    int EmployeeId,
    int ShipperId,
    IReadOnlyList<OrderLineItem> Items);

public record OrderCreationResult(int CustomerId, IReadOnlyList<QueryRow> RpcResult);

// ---------- Clientes ----------
public record CustomerContactUpdate(string? Address, string? City, string? PostalCode, string? Country);

// ---------- Compras ----------
public record PurchaseOrderLine(int ProductId, int Quantity, decimal UnitPrice);

public record CreatePurchaseOrderCommand(
    int SupplierId,
    int EmployeeId,
    DateTime? RequiredDate,
    IReadOnlyList<PurchaseOrderLine> Lines);

// ---------- Dashboard ----------
public record DashboardKpis(
    decimal TotalSales,
    int UnitsSold,
    int TotalOrders,
    decimal TotalInvested,
    decimal EstimatedProfit,
    int LowStockItems);

public record DashboardResult(DashboardKpis Kpis, IReadOnlyList<QueryRow> SalesByCustomer);

public record MonthlyAmount(int Year, int Month, decimal Sales, decimal Investment);

// ---------- Auth ----------
public record LoginResult(
    string Token,
    DateTime ExpiresAtUtc,
    int UserId,
    string Username,
    string? Role,
    int? EmployeeId,
    bool MustChangePassword);

/// <summary>Nombres de los claims que viajan dentro del JWT.</summary>
public static class CustomClaims
{
    public const string UserId = "sub";
    public const string Username = "unique_name";
    public const string Role = "role";
    public const string EmployeeId = "employeeId";
}
