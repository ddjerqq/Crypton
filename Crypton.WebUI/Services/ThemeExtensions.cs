namespace Crypton.WebUI.Services;

public static class ThemeExtensions
{
    public static string Value(this Theme theme)
    {
        return theme switch
        {
            Theme.Light => "light",
            Theme.Dark => "dark",
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null),
        };
    }
}