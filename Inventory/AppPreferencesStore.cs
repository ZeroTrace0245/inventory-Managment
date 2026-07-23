namespace inventory_Managment.Inventory;

public sealed class AppPreferencesStore
{
    private AppPreferences _current = new("Shop Admin", AppTheme.System, string.Empty);

    public event Action? Changed;

    public AppPreferences Current => _current;

    public string DisplayName => _current.DisplayName;

    public AppTheme Theme => _current.Theme;

    public bool HasToken => !string.IsNullOrWhiteSpace(_current.GitHubModelsToken);

    public string GitHubModelsToken => _current.GitHubModelsToken;

    public void SetPreferences(AppPreferences preferences)
    {
        _current = Normalize(preferences);
        Changed?.Invoke();
    }

    public void SetDisplayName(string displayName)
    {
        _current = _current with { DisplayName = NormalizeDisplayName(displayName) };
        Changed?.Invoke();
    }

    public void SetTheme(AppTheme theme)
    {
        _current = _current with { Theme = theme };
        Changed?.Invoke();
    }

    public void SetGitHubModelsToken(string token)
    {
        _current = _current with { GitHubModelsToken = token.Trim() };
        Changed?.Invoke();
    }

    private static AppPreferences Normalize(AppPreferences preferences) => preferences with
    {
        DisplayName = NormalizeDisplayName(preferences.DisplayName),
        GitHubModelsToken = preferences.GitHubModelsToken.Trim()
    };

    private static string NormalizeDisplayName(string displayName)
    {
        var normalized = displayName.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? "Shop Admin" : normalized;
    }
}
