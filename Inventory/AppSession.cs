namespace inventory_Managment.Inventory;

public enum AppRole
{
    Manager,
    Suppliers,
    StockClerk
}

public sealed record AppUserProfile(
    string Id,
    string DisplayName,
    AppRole Role,
    string Description);

public sealed record AppSessionSnapshot(string UserId);
