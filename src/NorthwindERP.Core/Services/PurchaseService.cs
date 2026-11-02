using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Exceptions;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.Core.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchases;

    public PurchaseService(IPurchaseRepository purchases) => _purchases = purchases;

    public Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(CancellationToken ct = default)
        => _purchases.GetAllAsync(ct);

    public async Task<PurchaseOrder> GetByIdAsync(int id, CancellationToken ct = default)
        => await _purchases.GetByIdAsync(id, ct)
           ?? throw new NotFoundException($"No existe la orden de compra con id {id}.");

    public async Task<PurchaseOrder> CreateAsync(CreatePurchaseOrderCommand command, CancellationToken ct = default)
    {
        if (command.Lines.Count == 0)
            throw new BusinessRuleException("La orden de compra debe tener al menos un producto.");

        if (command.Lines.Any(l => l.Quantity <= 0))
            throw new BusinessRuleException("Todas las cantidades deben ser mayores a 0.");

        if (command.Lines.Any(l => l.UnitPrice < 0))
            throw new BusinessRuleException("El precio unitario no puede ser negativo.");

        if (command.RequiredDate.HasValue && command.RequiredDate.Value.Date < DateTime.Today)
            throw new BusinessRuleException("La fecha requerida no puede estar en el pasado.");

        var purchaseOrder = new PurchaseOrder
        {
            SupplierId = command.SupplierId,
            EmployeeId = command.EmployeeId,
            OrderDate = DateTime.Now,
            RequiredDate = command.RequiredDate,
            Status = PurchaseOrderStatus.Pending,
            // La PK de purchaseorderdetails es (purchaseorderid, productid):
            // un producto no puede repetirse, así que se agrupan las líneas.
            Details = command.Lines
                .GroupBy(l => l.ProductId)
                .Select(g => new PurchaseOrderDetail
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    UnitPrice = g.First().UnitPrice
                })
                .ToList()
        };

        var id = await _purchases.AddAsync(purchaseOrder, ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<PurchaseOrder> ReceiveAsync(int purchaseOrderId, int employeeId, CancellationToken ct = default)
    {
        var purchaseOrder = await GetByIdAsync(purchaseOrderId, ct);

        if (string.Equals(purchaseOrder.Status, PurchaseOrderStatus.Received, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException($"La orden de compra {purchaseOrderId} ya fue recibida.");

        // La RPC cambia el estado, suma el stock y crea el movimiento INBOUND en el Kardex.
        await _purchases.ReceiveAsync(purchaseOrderId, employeeId, ct);

        return await GetByIdAsync(purchaseOrderId, ct);
    }
}
