using Microsoft.UI.Xaml;

namespace NTokenizers_Tools_MarkdownViewer;

/// <summary>
/// Entry point for the MarkdownViewer application.
/// </summary>
public partial class App : Application
{
    public static Window Window { get; private set; } = null!;
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;
    public static nint WindowHandle =>
        WinRT.Interop.WindowNative.GetWindowHandle(Window);

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Handle single-instance: if another instance is already running, exit
        // (optional, but helps with file association issues)
        
        Window = new MainWindow();
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        // Set initial file path BEFORE activating the window
        // This ensures the file is loaded when the Loaded event fires
        var argsArray = Environment.GetCommandLineArgs();
        if (argsArray.Length > 1)
        {
            ((MainWindow)Window).SetInitialFilePath(argsArray[1]);
        }

        Window.Activate();
    }
}
