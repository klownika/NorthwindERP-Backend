using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Exceptions;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;
using NorthwindERP.Core.Services;

namespace NorthwindERP.Tests;

/// <summary>
/// Pruebas unitarias de la lógica de negocio de pedidos.
/// No tocan Supabase: usan repositorios "falsos" en memoria.
/// Ejecutar con: dotnet test
/// </summary>
public class OrderServiceTests
{
    private static readonly List<OrderLineItem> OneItem = new() { new OrderLineItem(1, 2) };

    [Fact]
    public async Task Create_SinClienteNiClienteNuevo_LanzaBusinessRule()
    {
        var service = new OrderService(new FakeOrderRepository(), new FakeCustomerRepository());
        var command = new CreateOrderCommand(null, null, EmployeeId: 1, ShipperId: 1, OneItem);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateWithCustomerAsync(command));
    }

    [Fact]
    public async Task Create_ConClienteYClienteNuevo_LanzaBusinessRule()
    {
        var service = new OrderService(new FakeOrderRepository(), new FakeCustomerRepository(1));
        var command = new CreateOrderCommand(1, new Customer { CustomerName = "X" }, 1, 1, OneItem);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateWithCustomerAsync(command));
    }

    [Fact]
    public async Task Create_ClienteInexistente_LanzaNotFound()
    {
        var service = new OrderService(new FakeOrderRepository(), new FakeCustomerRepository());
        var command = new CreateOrderCommand(999, null, 1, 1, OneItem);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateWithCustomerAsync(command));
    }

    [Fact]
    public async Task Create_CantidadCero_LanzaBusinessRule()
    {
        var service = new OrderService(new FakeOrderRepository(), new FakeCustomerRepository(1));
        var command = new CreateOrderCommand(1, null, 1, 1, new List<OrderLineItem> { new(1, 0) });

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateWithCustomerAsync(command));
    }

    [Fact]
    public async Task Create_ClienteNuevoValido_LlamaAlRepositorioConLineasAgrupadas()
    {
        var orders = new FakeOrderRepository();
        var service = new OrderService(orders, new FakeCustomerRepository());
        var items = new List<OrderLineItem> { new(1, 2), new(1, 3), new(2, 1) };
        var command = new CreateOrderCommand(null, new Customer { CustomerName = "Bodega" }, 1, 1, items);

        await service.CreateWithCustomerAsync(command);

        Assert.NotNull(orders.LastItems);
        Assert.Equal(2, orders.LastItems!.Count);
        Assert.Equal(5, orders.LastItems.Single(i => i.ProductId == 1).Quantity);
    }

    [Fact]
    public void MergeDuplicateLines_SumaCantidadesDelMismoProducto()
    {
        var merged = OrderService.MergeDuplicateLines(new[] { new OrderLineItem(7, 1), new OrderLineItem(7, 4) });

        Assert.Single(merged);
        Assert.Equal(5, merged[0].Quantity);
    }

    // ----------------- Fakes -----------------

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public IReadOnlyList<OrderLineItem>? LastItems { get; private set; }

        public Task<OrderCreationResult> CreateOrderAsync(Customer? newCustomer, int? existingCustomerId, int employeeId,
            int shipperId, IReadOnlyList<OrderLineItem> items, CancellationToken ct = default)
        {
            LastItems = items;
            return Task.FromResult(new OrderCreationResult(existingCustomerId ?? 100, new List<QueryRow>()));
        }

        public Task<IReadOnlyList<QueryRow>> GetCustomerOrderHistoryAsync(int customerId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<QueryRow>>(new List<QueryRow>());

        public Task<IReadOnlyList<QueryRow>> GetOrdersExtendedAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<QueryRow>>(new List<QueryRow>());

        public Task<bool> DeleteAsync(int orderId, CancellationToken ct = default) => Task.FromResult(true);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        private readonly HashSet<int> _existingIds;

        public FakeCustomerRepository(params int[] existingIds) => _existingIds = existingIds.ToHashSet();

        public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Customer>>(new List<Customer>());

        public Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult<Customer?>(_existingIds.Contains(id) ? new Customer { CustomerId = id } : null);

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_existingIds.Contains(id));

        public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    }
}
