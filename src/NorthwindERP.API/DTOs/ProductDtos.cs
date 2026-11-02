namespace NorthwindERP.API.DTOs;

// DTO = lo que realmente sale por la API. No exponemos las entidades directamente.

public record ProductDto(
    int ProductId,
    string ProductName,
    int? CategoryId,
    string? CategoryName,
    int? SupplierId,
    string? SupplierName,
    string? Unit,
    decimal? Price);

public record CategoryDto(int CategoryId, string CategoryName, string? Description);
