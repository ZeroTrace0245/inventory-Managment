namespace inventory_Managment.Inventory;

public enum UserRole
{
    Manager,
    Supplier,
    StockClerk
}

public sealed record User(int Id, string Username, UserRole Role);

public sealed record Category(int Id, string Name, string Description);

public sealed record Supplier(int Id, string Name, string ContactName, string Phone, string Email);

public sealed record Product(
    int Id,
    string Name,
    string Sku,
    int CategoryId,
    int SupplierId,
    string Unit,
    decimal CostPrice,
    decimal SalePrice,
    int QuantityOnHand,
    int ReorderLevel,
    DateOnly? ExpiryDate,
    bool IsActive);

public sealed record StockMovement(
    int Id,
    int ProductId,
    int QuantityChange,
    string Reason,
    string Notes,
    DateTimeOffset OccurredAt);

public sealed record StockMovementDetail(
    int Id,
    string ProductName,
    int QuantityChange,
    string Reason,
    string Notes,
    DateTimeOffset OccurredAt);

public sealed record InventoryDashboard(
    int ProductCount,
    int CategoryCount,
    int SupplierCount,
    int LowStockCount,
    int OutOfStockCount,
    decimal StockCostValue,
    decimal StockRetailValue);

public sealed record ProductOverview(
    int Id,
    string Name,
    string Sku,
    string Category,
    string Supplier,
    string Unit,
    decimal CostPrice,
    decimal SalePrice,
    int QuantityOnHand,
    int ReorderLevel,
    DateOnly? ExpiryDate,
    bool IsLowStock,
    bool IsOutOfStock);
