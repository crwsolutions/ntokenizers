using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NTokenizers.ToHtml;
using Windows.Storage.Pickers;
using WinRT;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NTokenizers_Tools_MarkdownViewer;

/// <summary>
/// The main content page for the MarkdownViewer application.
/// Renders Markdown files as HTML in a WebView2 control with live file watching.
/// </summary>
public sealed partial class MainPage : Page
{
    private readonly FileSystemWatcher _watcher;
    private readonly System.Timers.Timer _debounceTimer;
    private string? _currentFilePath;
    private bool _webViewReady;
    private bool _pageClosed;

    /// <summary>
    /// Gets or sets the initial file path to load before the Loaded event.
    /// This bypasses the open dialog when a file is passed via command line.
    /// </summary>
    public string? InitialFilePath { get; set; }

    public MainPage()
    {
        InitializeComponent();

        _watcher = new FileSystemWatcher
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
        };
        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Renamed += OnFileRenamed;

        _debounceTimer = new System.Timers.Timer(300) { AutoReset = false };
        _debounceTimer.Elapsed += async (_, _) =>
        {
            var path = _currentFilePath;
            if (path is not null && !_pageClosed)
            {
                try
                {
                    App.DispatcherQueue.TryEnqueue(async () =>
                    {
                        await LoadAndRenderAsync(path);
                    });
                }
                catch (ObjectDisposedException) { }
            }
        };

        Loaded += async (_, _) =>
        {
            await InitializeWebView2Async();

            if (_currentFilePath is null)
            {
                if (InitialFilePath is not null)
                {
                    // File path was provided via command line before window activation
                    // Use OpenFile to properly set _currentFilePath and configure the watcher
                    OpenFile(InitialFilePath);
                    InitialFilePath = null;
                }
                else
                {
                    ShowOpenDialog();
                }
            }
        };

        KeyDown += (_, e) =>
        {
            if (e.Key == Windows.System.VirtualKey.F5)
            {
                Refresh();
            }
        };

        Unloaded += (_, _) =>
        {
            _pageClosed = true;
            _debounceTimer.Stop();
            _watcher.Dispose();
            _debounceTimer.Dispose();
        };
    }

    private async Task InitializeWebView2Async()
    {
        await WebView.EnsureCoreWebView2Async();

        _webViewReady = true;

        try
        {
            var exePath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exePath))
            {
                FileAssociation.Register(exePath);
            }
        }
        catch { }
    }

    public void OpenFile(string filePath)
    {
        if (!_webViewReady)
        {
            // WebView2 not ready yet, defer until it is
            Loaded += async (_, _) =>
            {
                await InitializeWebView2Async();
                OpenFile(filePath);
            };
            return;
        }

        _watcher.EnableRaisingEvents = false;

        _currentFilePath = filePath;
        var fileName = Path.GetFileName(filePath);

        UpdateTitleBar(fileName, "—", true);

        _watcher.Path = Path.GetDirectoryName(filePath)!;
        _watcher.Filter = fileName;
        _watcher.EnableRaisingEvents = true;

        _ = LoadAndRenderAsync(filePath);
    }

    public void Refresh()
    {
        var path = _currentFilePath;
        if (path is not null && _webViewReady)
            _ = LoadAndRenderAsync(path);
    }

    public void OpenInExplorer()
    {
        if (_currentFilePath is null) return;
        Process.Start("explorer.exe", $"/select \"{_currentFilePath}\"");
    }

    public void ShowOpenDialog()
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.Desktop
        };
        picker.FileTypeFilter.Add(".md");
        picker.FileTypeFilter.Add(".markdown");

        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle);

        var file = picker.PickSingleFileAsync().AsTask().GetAwaiter().GetResult();
        if (file is not null)
        {
            OpenFile(file.Path);
        }
    }

    public void OpenInEditor()
    {
        if (_currentFilePath is null) return;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = _currentFilePath, UseShellExecute = true });
        }
        catch { }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _watcher.EnableRaisingEvents = false;
    }

    private async Task LoadAndRenderAsync(string filePath)
    {
        try
        {
            if (!Path.IsPathRooted(filePath))
            {
                // Het is een relatief pad, plak de map van de .exe er direct voor
                var exeFolder = AppDomain.CurrentDomain.BaseDirectory;
                filePath = Path.Combine(exeFolder, filePath);
            }

            if (!File.Exists(filePath))
            {
                ShowError("File not found", "The file no longer exists.");
                return;
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            var resolved = EncodingResolver.Resolve(stream);
            stream.Position = 0;

            using var reader = new StreamReader(stream, resolved.Encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
            using var writer = new StringWriter();

            await MarkdownConverter.WriteHtmlDocumentAsync(reader, writer);
            var html = writer.ToString();

            const string overrideCss =
                "<style>" +
                "html,body{background:#fff!important;color:#222!important}" +
                ".markdown-document{max-width:none!important;margin:0!important;padding:20px!important}" +
                "</style>";
            html = html.Replace("</head>", overrideCss + "</head>");

            if (!_pageClosed)
            {
                WebView.NavigateToString(html);

                var fileName = Path.GetFileName(filePath);
                UpdateTitleBar(fileName, $"✓ {resolved.Encoding.WebName} · {resolved.Source}", true);
            }
        }
        catch (Exception ex)
        {
            if (!_pageClosed)
                ShowError("Error reading file", ex.Message);
        }
    }

    private void UpdateTitleBar(string fileName, string encoding, bool hasFile)
    {
        var window = App.Window as MainWindow;
        if (window is null) return;
        App.DispatcherQueue.TryEnqueue(() =>
        {
            window.UpdateTitleBar(fileName, encoding, hasFile);
        });
    }

    private void ShowError(string title, string message)
    {
        var t = WebUtility.HtmlEncode(title);
        var m = WebUtility.HtmlEncode(message);
        WebView.NavigateToString(
            "<!DOCTYPE html><html><head><meta charset=\"UTF-8\" /><style>" +
            "body{font-family:system-ui,sans-serif;display:flex;align-items:center;justify-content:center;height:100vh;margin:0;background:#fafafa}" +
            ".error{text-align:center;color:#c0392b}.error h2{margin-bottom:.5em}.error p{color:#666}" +
            $"</style></head><body><div class=\"error\"><h2>{t}</h2><p>{m}</p></div></body></html>");
    }
}
