using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Models;

namespace NorthwindERP.Core.Interfaces;

// Contratos de la lógica de negocio. Los controladores dependen de estas interfaces.

public interface IInventoryService
{
    Task<IReadOnlyList<Product>> GetProductsAsync(string? search, int? categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Warehouse>> GetWarehousesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<InventoryStock>> GetStockAsync(StockFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryTransaction>> GetKardexAsync(KardexFilter filter, CancellationToken ct = default);
}

public interface IOrderService
{
    Task<OrderCreationResult> CreateWithCustomerAsync(CreateOrderCommand command, CancellationToken ct = default);
    Task<IReadOnlyList<QueryRow>> GetCustomerHistoryAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<QueryRow>> GetAllAsync(CancellationToken ct = default);
    Task DeleteAsync(int orderId, CancellationToken ct = default);
}

public interface ICustomerService
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<Customer> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Customer> UpdateContactAsync(int id, CustomerContactUpdate update, CancellationToken ct = default);
}

public interface ILogisticsService
{
    Task<IReadOnlyList<Employee>> GetEmployeesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Shipper>> GetShippersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetSuppliersAsync(CancellationToken ct = default);
}

public interface IPurchaseService
{
    Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(CancellationToken ct = default);
    Task<PurchaseOrder> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PurchaseOrder> CreateAsync(CreatePurchaseOrderCommand command, CancellationToken ct = default);
    Task<PurchaseOrder> ReceiveAsync(int purchaseOrderId, int employeeId, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardResult> GetKpisAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MonthlyAmount>> GetSalesVsInvestmentAsync(CancellationToken ct = default);
}

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string username, string password, CancellationToken ct = default);
}
