using System.ComponentModel.DataAnnotations;

namespace NorthwindERP.API.DTOs;

public record CustomerDto(
    int CustomerId,
    string CustomerName,
    string? ContactName,
    string? Address,
    string? City,
    string? PostalCode,
    string? Country);

/// <summary>Cuerpo de PUT /api/customers/{id}. Solo se actualizan los campos que envíes.</summary>
public class UpdateCustomerContactRequest
{
    [StringLength(150)] public string? Address { get; set; }
    [StringLength(60)] public string? City { get; set; }
    [StringLength(20)] public string? PostalCode { get; set; }
    [StringLength(60)] public string? Country { get; set; }
}
