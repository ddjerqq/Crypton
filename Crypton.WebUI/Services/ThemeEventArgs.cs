namespace Crypton.WebUI.Services;

public sealed class ThemeEventArgs : EventArgs
#pragma warning restore SA1402
{
    public ThemeEventArgs(Theme theme)
    {
        this.Theme = theme;
    }

    public Theme Theme { get; set; }
}