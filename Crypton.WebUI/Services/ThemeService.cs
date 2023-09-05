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
        localStorage = localStorage;
        js = js;
    }

    public event EventHandler<ThemeEventArgs>? OnThemeChanged;

    public async Task<Theme> GetThemeAsync()
    {
        string? theme = await localStorage.GetItemAsStringAsync("theme");

        // if the theme is dark return dark
        if (theme == "dark") return Theme.Dark;

        // if its anything else, even light, return light.
        return Theme.Light;
    }

    public async Task SetThemeAsync(Theme theme)
    {
        await js.InvokeVoidAsync("setTheme", theme.Value());
        await localStorage.SetItemAsStringAsync("theme", theme.Value());

        OnThemeChanged?.Invoke(this, new ThemeEventArgs(theme));
    }

    public async Task ToggleThemeAsync()
    {
        var theme = await GetThemeAsync();
        await SetThemeAsync(theme == Theme.Light ? Theme.Dark : Theme.Light);
    }
}