using System.Text.Json;
using Microsoft.JSInterop;

namespace inventory_Managment.Inventory;

public sealed class AppPreferencesStorage
{
    private const string StorageKey = "inventory-managment.preferences";
    private readonly IJSRuntime _jsRuntime;

    public AppPreferencesStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<AppPreferencesSnapshot?> LoadAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("inventoryPreferences.get", StorageKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<AppPreferencesSnapshot>(json, JsonOptions);
        }
        catch (JsonException)
        {
            // Clear corrupted data
            await _jsRuntime.InvokeVoidAsync("inventoryPreferences.remove", StorageKey);
            return null;
        }
        catch (Exception)
        {
            // Handle other deserialization errors
            return null;
        }
    }

    public ValueTask SaveAsync(AppPreferencesSnapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        return _jsRuntime.InvokeVoidAsync("inventoryPreferences.set", StorageKey, json);
    }

    private static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web);
}
