using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindERP.API.DTOs;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.API.Controllers;

/// <summary>👥 Módulo 3 — Clientes</summary>
[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;

    public CustomersController(ICustomerService customers) => _customers = customers;

    /// <summary>Lista todos los clientes.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll(CancellationToken ct)
    {
        var customers = await _customers.GetAllAsync(ct);
        return Ok(customers.Select(c => c.ToDto()));
    }

    /// <summary>Obtiene un cliente por id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id, CancellationToken ct)
        => Ok((await _customers.GetByIdAsync(id, ct)).ToDto());

    /// <summary>Actualiza solo los datos de contacto/dirección de un cliente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> UpdateContact(
        int id, [FromBody] UpdateCustomerContactRequest request, CancellationToken ct)
    {
        var update = new CustomerContactUpdate(request.Address, request.City, request.PostalCode, request.Country);
        var customer = await _customers.UpdateContactAsync(id, update, ct);
        return Ok(customer.ToDto());
    }
}
