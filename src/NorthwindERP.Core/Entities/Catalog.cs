namespace NorthwindERP.Core.Entities;

// Cada clase representa una tabla de Supabase.
// Las propiedades en PascalCase se convierten a minúsculas automáticamente
// (CategoryId -> categoryid) gracias a UseLowerCaseNamingConvention() en el DbContext.

/// <summary>Tabla: categories</summary>
public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>Tabla: suppliers</summary>
public class Supplier
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
}

/// <summary>Tabla: products</summary>
public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public int? CategoryId { get; set; }
    public string? Unit { get; set; }
    public decimal? Price { get; set; }

    // Propiedades de navegación (JOIN automático con Include)
    public Category? Category { get; set; }
    public Supplier? Supplier { get; set; }
}
