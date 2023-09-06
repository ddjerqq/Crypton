using Blazored.LocalStorage;
using Crypton.WebUI.Services.Interfaces;
using Microsoft.JSInterop;

namespace Crypton.WebUI.Services;

public sealed class ThemeService : IThemeService
{
    private readonly IJSRuntime _js;
    private readonly ILocalStorageService _localStorage;

    public ThemeService(ILocalStorageService localStorage, IJSRuntime js)
    {
        _localStorage = localStorage;
        _js = js;
    }

    public event EventHandler<string>? OnThemeChanged;

    public async Task<string> GetThemeAsync()
    {
        string? theme = await _localStorage.GetItemAsStringAsync("theme");

        // if the theme is dark return dark
        if (theme == "dark") return "dark";

        // if its anything else, even light, return light.
        return "light";
    }

    public async Task SetThemeAsync(string theme)
    {
        await _js.InvokeVoidAsync("setTheme", theme);
        await _localStorage.SetItemAsStringAsync("theme", theme);

        OnThemeChanged?.Invoke(this, theme);
    }

    public async Task ToggleThemeAsync()
    {
        var theme = await GetThemeAsync();
        await SetThemeAsync(theme == "light" ? "dark" : "light");
    }
}