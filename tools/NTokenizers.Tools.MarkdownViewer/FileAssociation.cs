using Microsoft.Win32;

namespace NTokenizers_Tools_MarkdownViewer;

/// <summary>
/// Manages the Windows file association for .md files (HKCU, no admin required).
/// </summary>
public static class FileAssociation
{
    private const string ExeName = "MarkdownViewer";
    private const string RegExeName = ExeName + ".Md";

    /// <summary>
    /// Registers .md files to open with MarkdownViewer.
    /// </summary>
    /// <param name="exePath">Full path to the MarkdownViewer executable.</param>
    public static void Register(string exePath)
    {
        if (string.IsNullOrEmpty(exePath))
            return;

        using var mdKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.md");
        mdKey.SetValue(string.Empty, RegExeName);

        using var fileTypeKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{RegExeName}");
        fileTypeKey.SetValue(string.Empty, "MarkdownViewer Markdown File");

        using var commandKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{RegExeName}\shell\open\command");
        commandKey.SetValue(string.Empty, $"\"{exePath}\" \"%1\"");
    }

    /// <summary>
    /// Removes the .md file association for MarkdownViewer.
    /// </summary>
    public static void Unregister()
    {
        try
        {
            using var mdKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.md", writable: true);
            if (mdKey?.GetValue(string.Empty) is string current && current == RegExeName)
            {
                mdKey.DeleteValue(string.Empty, throwOnMissingValue: false);
            }
        }
        catch { }

        try
        {
            Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{RegExeName}", throwOnMissingSubKey: false);
        }
        catch { }
    }
}
