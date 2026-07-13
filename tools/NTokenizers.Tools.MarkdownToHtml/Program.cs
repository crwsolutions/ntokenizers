using NTokenizers.ToHtml;
using System.Diagnostics;

namespace NTokenizers.Tools.MarkdownToHtml;

/// <summary>
/// CLI entry point. Reads a Markdown file via stream and writes a styled HTML file.
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: NTokenizers.Tools.MarkdownToHtml <input.md> [output.html]");
            Console.Error.WriteLine("  If output is not specified, it defaults to the same name with .html extension.");
            return 1;
        }

        string inputPath = args[0];
        string outputPath = args.Length > 1 ? args[1] : Path.ChangeExtension(inputPath, ".html");

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Error: Input file not found: {inputPath}");
            return 1;
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();

            using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            using var writer = new StreamWriter(outputStream, leaveOpen: false);

            await MarkdownConverter.WriteHtmlDocumentAsync(inputStream, writer);

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;

            Console.WriteLine();
            Console.WriteLine(new string('─', 60));
            Console.WriteLine($"Converted: {inputPath} -> {outputPath}");
            Console.Write("Elapsed time: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{elapsed:mm\\:ss\\.fff}");
            Console.ResetColor();
            Console.Write(" (");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{elapsed.TotalMilliseconds:F2}");
            Console.ResetColor();
            Console.WriteLine(" ms)");
            Console.WriteLine(new string('─', 60));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
