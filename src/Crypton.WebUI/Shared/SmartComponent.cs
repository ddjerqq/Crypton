using Crypton.WebUI.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Crypton.WebUI.Shared;

public abstract class SmartComponent : ComponentBase, IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;

    protected CancellationToken CancellationToken => (_cancellationTokenSource ??= new()).Token;

    [Inject]
    protected IThemeService ThemeService { get; set; } = default!;

    [CascadingParameter(Name = "theme")]
    protected string Theme { get; private set; } = "light";

    protected bool IsLight => Theme == "light";

    protected bool IsDark => Theme == "dark";

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (_cancellationTokenSource is null)
            return;

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
    }

    protected override async Task OnParametersSetAsync()
    {
        Theme = await ThemeService.GetThemeAsync();

        ThemeService.OnThemeChanged += (_, theme) =>
        {
            Theme = theme;
            StateHasChanged();
        };

        await base.OnParametersSetAsync();
    }
}