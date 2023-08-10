namespace Crypton.WebUI.Services.Interfaces;

public interface IThemeService
{
    public event EventHandler<ThemeEventArgs> OnThemeChanged;

    public Task<Theme> GetThemeAsync();

    public Task SetThemeAsync(Theme theme);

    public Task ToggleThemeAsync();
}