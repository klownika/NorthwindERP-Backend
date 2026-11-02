using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Exceptions;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.Core.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customers;

    public CustomerService(ICustomerRepository customers) => _customers = customers;

    public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default)
        => _customers.GetAllAsync(ct);

    public async Task<Customer> GetByIdAsync(int id, CancellationToken ct = default)
        => await _customers.GetByIdAsync(id, ct)
           ?? throw new NotFoundException($"No existe el cliente con id {id}.");

    public async Task<Customer> UpdateContactAsync(int id, CustomerContactUpdate update, CancellationToken ct = default)
    {
        var nothingToUpdate =
            string.IsNullOrWhiteSpace(update.Address) &&
            string.IsNullOrWhiteSpace(update.City) &&
            string.IsNullOrWhiteSpace(update.PostalCode) &&
            string.IsNullOrWhiteSpace(update.Country);

        if (nothingToUpdate)
            throw new BusinessRuleException("Envía al menos un campo de contacto para actualizar.");

        var customer = await GetByIdAsync(id, ct);

        // Solo se cambian los campos que llegaron (los null se ignoran).
        if (!string.IsNullOrWhiteSpace(update.Address)) customer.Address = update.Address.Trim();
        if (!string.IsNullOrWhiteSpace(update.City)) customer.City = update.City.Trim();
        if (!string.IsNullOrWhiteSpace(update.PostalCode)) customer.PostalCode = update.PostalCode.Trim();
        if (!string.IsNullOrWhiteSpace(update.Country)) customer.Country = update.Country.Trim();

        await _customers.SaveChangesAsync(ct); // EF Core genera el UPDATE solo con las columnas modificadas
        return customer;
    }
}
