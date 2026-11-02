using Microsoft.EntityFrameworkCore;
using NorthwindERP.Core.Entities;

namespace NorthwindERP.Infrastructure.Data;

/// <summary>
/// Puente entre C# y la base de datos de Supabase.
/// Cada DbSet = una tabla. Las columnas se mapean en minúsculas automáticamente
/// (ver UseLowerCaseNamingConvention en DependencyInjection.cs).
/// </summary>
public class NorthwindDbContext : DbContext
{
    public NorthwindDbContext(DbContextOptions<NorthwindDbContext> options) : base(options) { }

    // 8 tablas base de Northwind
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Shipper> Shippers => Set<Shipper>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    // 8 tablas agregadas
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();
    public DbSet<InventoryStock> InventoryStock => Set<InventoryStock>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Nombres EXACTOS de las tablas en Supabase. Si alguna se llama distinto, cámbiala aquí.
        modelBuilder.Entity<Category>().ToTable("categories");
        modelBuilder.Entity<Customer>().ToTable("customers");
        modelBuilder.Entity<Employee>().ToTable("employees");
        modelBuilder.Entity<Order>().ToTable("orders");
        modelBuilder.Entity<OrderDetail>().ToTable("orderdetails");
        modelBuilder.Entity<Product>().ToTable("products");
        modelBuilder.Entity<Shipper>().ToTable("shippers");
        modelBuilder.Entity<Supplier>().ToTable("suppliers");

        modelBuilder.Entity<Role>().ToTable("roles");
        modelBuilder.Entity<UserAccount>().ToTable("useraccounts");
        modelBuilder.Entity<PurchaseOrder>().ToTable("purchaseorders");
        modelBuilder.Entity<Warehouse>().ToTable("warehouses");
        modelBuilder.Entity<Payment>().ToTable("payments");

        // Claves primarias que no siguen la convención <Clase>Id
        modelBuilder.Entity<InventoryStock>(e =>
        {
            e.ToTable("inventorystock");
            e.HasKey(x => x.StockId);
        });

        modelBuilder.Entity<InventoryTransaction>(e =>
        {
            e.ToTable("inventorytransactions");
            e.HasKey(x => x.TransactionId);
        });

        modelBuilder.Entity<PurchaseOrderDetail>(e =>
        {
            e.ToTable("purchaseorderdetails");
            e.HasKey(x => new { x.PurchaseOrderId, x.ProductId }); // PK compuesta
        });

        // Columnas numeric (dinero)
        modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("numeric");
        modelBuilder.Entity<PurchaseOrderDetail>().Property(p => p.UnitPrice).HasColumnType("numeric");
        modelBuilder.Entity<Payment>().Property(p => p.Amount).HasColumnType("numeric");
    }
}
