using NTokenizers.ToHtml;
using System.Diagnostics;
using System.Text;

namespace NTokenizers.Tools.MarkdownToHtml;

/// <summary>
/// CLI entry point. Reads a Markdown file via stream and writes a styled HTML file.
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        CommandLineOptions options;

        try
        {
            options = CommandLineOptions.Parse(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine();
            CommandLineOptions.PrintUsage(Console.Error);
            return 1;
        }

        if (!File.Exists(options.InputPath))
        {
            Console.Error.WriteLine($"Error: Input file not found: {options.InputPath}");
            return 1;
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();

            using var inputStream = new FileStream(options.InputPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            using var outputStream = new FileStream(options.EffectiveOutputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);

            var resolved = EncodingResolver.Resolve(options, inputStream);
            Encoding inputEncoding = resolved.Encoding;
            using var reader = new StreamReader(inputStream, inputEncoding, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
            using var writer = new StreamWriter(outputStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: false);

            await MarkdownConverter.WriteHtmlDocumentAsync(reader, writer);

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;

            Console.WriteLine();
            Console.WriteLine(new string('─', 60));
            Console.WriteLine($"Converted: {options.InputPath} -> {options.EffectiveOutputPath}");
            Console.WriteLine($"Input encoding: {inputEncoding.WebName} ({resolved.Source})");
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
