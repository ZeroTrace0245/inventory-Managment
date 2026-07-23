namespace inventory_Managment.Inventory;

public sealed class AppSessionStore
{
    private static readonly AppUserProfile[] BuiltInUsers =
    {
        new("manager", "Manager", AppRole.Manager, "Full access to inventory, settings, and reporting."),
        new("suppliers", "Suppliers", AppRole.Suppliers, "Handles vendor coordination and receiving."),
        new("stock-clerks", "Stock Clerks / Inventory Staff", AppRole.StockClerk, "Updates stock counts, movements, and day-to-day inventory tasks.")
    };

    private AppUserProfile? _currentUser;

    public event Action? Changed;

    public IReadOnlyList<AppUserProfile> Users => BuiltInUsers;

    public bool IsAuthenticated => _currentUser is not null;

    public AppUserProfile? CurrentUser => _currentUser;

    public string DisplayName => _currentUser?.DisplayName ?? string.Empty;

    public AppRole? Role => _currentUser?.Role;

    public void SignIn(string userId)
    {
        var user = BuiltInUsers.FirstOrDefault(profile => string.Equals(profile.Id, userId, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            throw new InvalidOperationException("Selected user was not found.");
        }

        _currentUser = user;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        if (_currentUser is null)
        {
            return;
        }

        _currentUser = null;
        Changed?.Invoke();
    }

    public void Restore(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _currentUser = null;
            return;
        }

        _currentUser = BuiltInUsers.FirstOrDefault(profile => string.Equals(profile.Id, userId, StringComparison.OrdinalIgnoreCase));
        Changed?.Invoke();
    }
}
