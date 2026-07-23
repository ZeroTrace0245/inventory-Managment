using System.Text.Json;
using Microsoft.JSInterop;

namespace inventory_Managment.Inventory;

public sealed class AppSessionStorage
{
    private const string StorageKey = "inventory-managment.session";
    private readonly IJSRuntime _jsRuntime;

    public AppSessionStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<AppSessionSnapshot?> LoadAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("inventoryPreferences.get", StorageKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<AppSessionSnapshot>(json, JsonOptions);
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

    public ValueTask SaveAsync(AppSessionSnapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        return _jsRuntime.InvokeVoidAsync("inventoryPreferences.set", StorageKey, json);
    }

    public ValueTask ClearAsync() => _jsRuntime.InvokeVoidAsync("inventoryPreferences.remove", StorageKey);

    private static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web);
}
