using NorthwindERP.Core.Exceptions;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.Core.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly ICustomerRepository _customers;

    public OrderService(IOrderRepository orders, ICustomerRepository customers)
    {
        _orders = orders;
        _customers = customers;
    }

    public async Task<OrderCreationResult> CreateWithCustomerAsync(CreateOrderCommand command, CancellationToken ct = default)
    {
        var usesExisting = command.CustomerId.HasValue;
        var usesNew = command.NewCustomer is not null;

        // Regla: o mandas un cliente existente, o uno nuevo. No ambos, no ninguno.
        if (usesExisting == usesNew)
            throw new BusinessRuleException("Envía 'customerId' (cliente existente) o 'newCustomer' (cliente nuevo), pero no ambos.");

        if (usesNew && string.IsNullOrWhiteSpace(command.NewCustomer!.CustomerName))
            throw new BusinessRuleException("El nombre del cliente nuevo es obligatorio.");

        if (command.Items.Count == 0)
            throw new BusinessRuleException("El pedido debe tener al menos un producto.");

        if (command.Items.Any(i => i.Quantity <= 0))
            throw new BusinessRuleException("Todas las cantidades deben ser mayores a 0.");

        if (usesExisting && !await _customers.ExistsAsync(command.CustomerId!.Value, ct))
            throw new NotFoundException($"No existe el cliente con id {command.CustomerId}.");

        // El stock se valida dentro de la RPC (register_customer_order).
        var items = MergeDuplicateLines(command.Items);

        return await _orders.CreateOrderAsync(
            command.NewCustomer, command.CustomerId, command.EmployeeId, command.ShipperId, items, ct);
    }

    public async Task<IReadOnlyList<QueryRow>> GetCustomerHistoryAsync(int customerId, CancellationToken ct = default)
    {
        if (!await _customers.ExistsAsync(customerId, ct))
            throw new NotFoundException($"No existe el cliente con id {customerId}.");

        return await _orders.GetCustomerOrderHistoryAsync(customerId, ct);
    }

    public Task<IReadOnlyList<QueryRow>> GetAllAsync(CancellationToken ct = default)
        => _orders.GetOrdersExtendedAsync(ct);

    public async Task DeleteAsync(int orderId, CancellationToken ct = default)
    {
        var deleted = await _orders.DeleteAsync(orderId, ct);
        if (!deleted)
            throw new NotFoundException($"No existe el pedido con id {orderId}.");
    }

    /// <summary>Si el mismo producto viene dos veces, suma las cantidades en una sola línea.</summary>
    public static IReadOnlyList<OrderLineItem> MergeDuplicateLines(IEnumerable<OrderLineItem> items)
        => items
            .GroupBy(i => i.ProductId)
            .Select(g => new OrderLineItem(g.Key, g.Sum(x => x.Quantity)))
            .ToList();
}
