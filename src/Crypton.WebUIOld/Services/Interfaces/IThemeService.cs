namespace Crypton.WebUIOld.Services.Interfaces;

public interface IThemeService
{
    public event EventHandler<string> OnThemeChanged;

    public Task<string> GetThemeAsync();

    public Task SetThemeAsync(string theme);

    public Task ToggleThemeAsync();
}