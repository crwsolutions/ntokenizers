using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NTokenizers_Tools_MarkdownViewer;

/// <summary>
/// The application window. Hosts the TitleBar with toolbar and the main page.
/// </summary>
public sealed partial class MainWindow : Window
{
    private MainPage? _mainPage;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        try
        {
            AppWindow.SetIcon("Assets/AppIcon.ico");
        }
        catch
        {
            // Icon not found - not critical
        }

        CenterOnWorkArea();

        RootFrame.Navigate(typeof(MainPage));
        _mainPage = (MainPage)RootFrame.Content;
    }

    private void CenterOnWorkArea()
    {
        const int width = 1024;
        const int height = 1024;
        AppWindow.Resize(new Windows.Graphics.SizeInt32(width, height));

        // Get the actual work area (excluding taskbar) using Win32 API
        var rect = new RECT();
        SystemParametersInfo(48, 0, ref rect, 0); // SPI_GETWORKAREA

        var x = (rect.Right - rect.Left - width) / 2 + rect.Left;
        var y = (rect.Bottom - rect.Top - height) / 2 + rect.Top;

        AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern bool SystemParametersInfo(int uiAction, int uiParam, ref RECT pvParam, int fWinIni);

    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <summary>
    /// Sets the initial file path to load before the window is activated.
    /// This bypasses the open dialog when a file is passed via command line.
    /// </summary>
    public void SetInitialFilePath(string filePath)
    {
        // Resolve relative paths against the app base directory
        if (!string.IsNullOrWhiteSpace(filePath) && !Path.IsPathRooted(filePath))
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
        }

        _mainPage?.InitialFilePath = filePath;
    }

    public void OpenFile(string filePath)
    {
        _mainPage?.OpenFile(filePath);
    }

    private void BtnExplorer_Click(object sender, RoutedEventArgs e)
    {
        _mainPage?.OpenInExplorer();
    }

    private void BtnOpen_Click(object sender, RoutedEventArgs e)
    {
        _mainPage?.ShowOpenDialog();
    }

    private void BtnEditor_Click(object sender, RoutedEventArgs e)
    {
        _mainPage?.OpenInEditor();
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        _mainPage?.Refresh();
    }

    /// <summary>
    /// Updates the title bar state (title, button enabled states, encoding label).
    /// </summary>
    public void UpdateTitleBar(string fileName, string encoding, bool hasFile)
    {
        Title = fileName;
        AppTitleBar.Title = fileName;
        BtnExplorer.IsEnabled = hasFile;
        BtnEditor.IsEnabled = hasFile;
        BtnRefresh.IsEnabled = hasFile;
        LblEncoding.Text = encoding;
    }
}
