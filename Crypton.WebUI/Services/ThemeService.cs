using Blazored.LocalStorage;
using Crypton.WebUI.Services.Interfaces;
using Microsoft.JSInterop;

namespace Crypton.WebUI.Services;

public sealed class ThemeService : IThemeService
{
    private readonly IJSRuntime js;
    private readonly ILocalStorageService localStorage;

    public ThemeService(ILocalStorageService localStorage, IJSRuntime js)
    {
        this.localStorage = localStorage;
        this.js = js;
    }

    public event EventHandler<ThemeEventArgs>? OnThemeChanged;

    public async Task<Theme> GetThemeAsync()
    {
        string? theme = await this.localStorage.GetItemAsStringAsync("theme");

        // if the theme is dark return dark
        if (theme == "dark") return Theme.Dark;

        // if its anything else, even light, return light.
        return Theme.Light;
    }

    public async Task SetThemeAsync(Theme theme)
    {
        await this.js.InvokeVoidAsync("setTheme", theme.Value());
        await this.localStorage.SetItemAsStringAsync("theme", theme.Value());

        this.OnThemeChanged?.Invoke(this, new ThemeEventArgs(theme));
    }

    public async Task ToggleThemeAsync()
    {
        var theme = await this.GetThemeAsync();
        await this.SetThemeAsync(theme == Theme.Light ? Theme.Dark : Theme.Light);
    }
}