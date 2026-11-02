using System.Text.Json;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;
using NorthwindERP.Infrastructure.Data;

namespace NorthwindERP.Infrastructure.Repositories;

/// <summary>
/// EF Core para tablas + Dapper para funciones RPC y vistas de Supabase.
/// Ambos usan la MISMA conexión, así que pueden compartir una transacción.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly NorthwindDbContext _db;

    public OrderRepository(NorthwindDbContext db) => _db = db;

    public async Task<OrderCreationResult> CreateOrderAsync(
        Customer? newCustomer, int? existingCustomerId, int employeeId, int shipperId,
        IReadOnlyList<OrderLineItem> items, CancellationToken ct = default)
    {
        // Si algo falla antes del Commit, "await using" hace ROLLBACK automático:
        // no queda un cliente creado sin pedido.
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        int customerId;
        if (newCustomer is not null)
        {
            _db.Customers.Add(newCustomer);
            await _db.SaveChangesAsync(ct);   // INSERT INTO customers ... RETURNING customerid
            customerId = newCustomer.CustomerId;
        }
        else
        {
            customerId = existingCustomerId
                         ?? throw new ArgumentException("Se requiere un cliente.", nameof(existingCustomerId));
        }

        // p_details (jsonb). ⚠️ Las claves deben ser EXACTAMENTE las que lee tu función
        // register_customer_order (ej: elem->>'product_id'). Cámbialas aquí si usas otras.
        var detailsJson = JsonSerializer.Serialize(
            items.Select(i => new { product_id = i.ProductId, quantity = i.Quantity }));

        // Notación con nombre (p_x => valor): no importa el orden de los parámetros.
        const string sql = """
            SELECT * FROM register_customer_order(
                p_customer_id => @CustomerId,
                p_employee_id => @EmployeeId,
                p_details     => @Details::jsonb,
                p_shipper_id  => @ShipperId)
            """;

        var rows = await _db.Database.GetDbConnection().QueryAsync(new CommandDefinition(
            sql,
            new { CustomerId = customerId, EmployeeId = employeeId, Details = detailsJson, ShipperId = shipperId },
            transaction.GetDbTransaction(),
            cancellationToken: ct));

        await transaction.CommitAsync(ct);

        return new OrderCreationResult(customerId, rows.ToQueryRows());
    }

    public async Task<IReadOnlyList<QueryRow>> GetCustomerOrderHistoryAsync(int customerId, CancellationToken ct = default)
    {
        const string sql = "SELECT * FROM get_customer_order_history(p_customer_id => @CustomerId)";

        var rows = await _db.Database.GetDbConnection().QueryAsync(
            new CommandDefinition(sql, new { CustomerId = customerId }, cancellationToken: ct));

        return rows.ToQueryRows();
    }

    public async Task<IReadOnlyList<QueryRow>> GetOrdersExtendedAsync(CancellationToken ct = default)
    {
        // ORDER BY 1 DESC = ordena por la primera columna de la vista (normalmente orderid)
        const string sql = "SELECT * FROM vw_order_details_extended ORDER BY 1 DESC";

        var rows = await _db.Database.GetDbConnection().QueryAsync(
            new CommandDefinition(sql, cancellationToken: ct));

        return rows.ToQueryRows();
    }

    public async Task<bool> DeleteAsync(int orderId, CancellationToken ct = default)
    {
        // DELETE FROM orders WHERE orderid = @id
        // orderdetails se borra solo por la regla ON DELETE CASCADE de la BD.
        var affected = await _db.Orders
            .Where(o => o.OrderId == orderId)
            .ExecuteDeleteAsync(ct);

        return affected > 0;
    }
}
