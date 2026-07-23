namespace inventory_Managment.Inventory;

public static class InventoryRegistration
{
    public static IServiceCollection AddInventory(this IServiceCollection services)
    {
        services.AddSingleton<InventoryStore>();
        services.AddSingleton<AppPreferencesStore>();
        services.AddSingleton<AppSessionStore>();
        return services;
    }
}
