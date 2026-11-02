using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Models;

namespace NorthwindERP.Core.Interfaces;

// Contratos de acceso a datos. Core solo conoce estas interfaces;
// la implementación real (EF Core + Supabase) vive en Infrastructure/Repositories.

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetProductsAsync(string? search, int? categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default);
}

public interface IInventoryRepository
{
    Task<IReadOnlyList<InventoryStock>> GetStockAsync(StockFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryTransaction>> GetKardexAsync(KardexFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<Warehouse>> GetWarehousesAsync(CancellationToken ct = default);
}

public interface IOrderRepository
{
    /// <summary>Inserta el cliente (si es nuevo) y llama a la RPC register_customer_order en UNA transacción.</summary>
    Task<OrderCreationResult> CreateOrderAsync(
        Customer? newCustomer, int? existingCustomerId, int employeeId, int shipperId,
        IReadOnlyList<OrderLineItem> items, CancellationToken ct = default);

    /// <summary>RPC get_customer_order_history.</summary>
    Task<IReadOnlyList<QueryRow>> GetCustomerOrderHistoryAsync(int customerId, CancellationToken ct = default);

    /// <summary>Vista vw_order_details_extended.</summary>
    Task<IReadOnlyList<QueryRow>> GetOrdersExtendedAsync(CancellationToken ct = default);

    /// <summary>DELETE en orders (orderdetails se borra por ON DELETE CASCADE).</summary>
    Task<bool> DeleteAsync(int orderId, CancellationToken ct = default);
}

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface ILogisticsRepository
{
    Task<IReadOnlyList<Employee>> GetEmployeesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Shipper>> GetShippersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetSuppliersAsync(CancellationToken ct = default);
}

public interface IPurchaseRepository
{
    Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(CancellationToken ct = default);
    Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> AddAsync(PurchaseOrder purchaseOrder, CancellationToken ct = default);

    /// <summary>RPC receive_purchase_order.</summary>
    Task<IReadOnlyList<QueryRow>> ReceiveAsync(int purchaseOrderId, int employeeId, CancellationToken ct = default);
}

public interface IDashboardRepository
{
    Task<DashboardKpis> GetKpisAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MonthlyAmount>> GetSalesVsInvestmentAsync(CancellationToken ct = default);

    /// <summary>Vista vw_customer_sales_summary.</summary>
    Task<IReadOnlyList<QueryRow>> GetCustomerSalesSummaryAsync(CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<UserAccount?> GetByIdAsync(int id, CancellationToken ct = default);
}

// Servicios técnicos (implementados en Infrastructure/Security)

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(UserAccount user);
}
