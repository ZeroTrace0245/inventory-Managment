namespace inventory_Managment.Inventory;

public enum AppTheme
{
    System = 0,
    Light = 1,
    Dark = 2
}

public sealed record AppPreferences(
    string DisplayName,
    AppTheme Theme,
    string GitHubModelsToken);
