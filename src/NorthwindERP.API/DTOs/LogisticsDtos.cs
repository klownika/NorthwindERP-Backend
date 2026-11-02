namespace NorthwindERP.API.DTOs;

public record EmployeeDto(
    int EmployeeId,
    string FirstName,
    string LastName,
    string FullName,
    DateTime? BirthDate,
    string? Notes);

public record ShipperDto(int ShipperId, string ShipperName, string? Phone);

public record SupplierDto(
    int SupplierId,
    string SupplierName,
    string? ContactName,
    string? Address,
    string? City,
    string? Country,
    string? Phone);
