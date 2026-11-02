namespace NorthwindERP.API.DTOs;

public record WarehouseDto(int WarehouseId, string WarehouseName, string? Location, int? ManagerEmployeeId);

public record StockDto(
    int StockId,
    int? WarehouseId,
    string? WarehouseName,
    int? ProductId,
    string? ProductName,
    int Quantity,
    int MinStockLevel,
    bool IsLowStock); // el frontend lo usa para pintar la alerta roja

public record KardexEntryDto(
    int TransactionId,
    DateTime? TransactionDate,
    string? TransactionType,
    int? WarehouseId,
    string? WarehouseName,
    int? ProductId,
    string? ProductName,
    int? Quantity,
    int? EmployeeId,
    string? EmployeeName,
    int? ReferenceId);
