namespace NTokenizers.Tools.MarkdownToHtml;

/// <summary>
/// Parsed command line options for the Markdown to HTML conversion tool.
/// </summary>
public sealed class CommandLineOptions
{
    /// <summary>
    /// Gets the path of the input Markdown file.
    /// </summary>
    public required string InputPath { get; init; }

    /// <summary>
    /// Gets the optional output HTML file path. When null, the input path with a .html extension is used.
    /// </summary>
    public string? OutputPath { get; init; }

    /// <summary>
    /// Gets the optional explicitly requested input encoding name, for example utf-8 or windows-1252.
    /// </summary>
    public string? EncodingName { get; init; }

    /// <summary>
    /// Gets a value indicating whether the tool should detect the input encoding by probing the stream.
    /// </summary>
    public bool DetectEncoding { get; init; }

    /// <summary>
    /// Gets the effective output path.
    /// </summary>
    public string EffectiveOutputPath => OutputPath ?? Path.ChangeExtension(InputPath, ".html");

    /// <summary>
    /// Parses command line arguments into options.
    /// </summary>
    /// <param name="args">The raw command line arguments.</param>
    /// <returns>The parsed command line options.</returns>
    /// <exception cref="ArgumentException">Thrown when required arguments are missing or an option is unknown.</exception>
    public static CommandLineOptions Parse(string[] args)
    {
        string? inputPath = null;
        string? outputPath = null;
        string? encodingName = null;
        bool detectEncoding = false;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg is "-h" or "--help")
            {
                PrintUsage(Console.Out);
                Environment.Exit(0);
            }

            if (arg is "-e" or "--encoding")
            {
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("Missing value for option -e, --encoding.");
                }

                i++;
                encodingName = args[i];
                continue;
            }

            if (arg.StartsWith("-e=", StringComparison.Ordinal))
            {
                encodingName = arg["-e=".Length..];
                continue;
            }

            if (arg is "-d" or "--detect")
            {
                detectEncoding = true;
                continue;
            }

            if (arg.StartsWith('-') && arg.Length > 1)
            {
                throw new ArgumentException($"Unknown option: {arg}");
            }

            if (inputPath is null)
            {
                inputPath = arg;
            }
            else if (outputPath is null)
            {
                outputPath = arg;
            }
            else
            {
                throw new ArgumentException("Too many positional arguments. Expected <input.md> [output.html].");
            }
        }

        if (inputPath is null)
        {
            throw new ArgumentException("Missing required <input.md> argument.");
        }

        return new CommandLineOptions
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            EncodingName = encodingName,
            DetectEncoding = detectEncoding,
        };
    }

    /// <summary>
    /// Prints usage instructions to the specified writer.
    /// </summary>
    /// <param name="writer">The writer that receives the usage text.</param>
    public static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("Usage: md2html [options] <input.md> [output.html]");
        writer.WriteLine();
        writer.WriteLine("Options:");
        writer.WriteLine("  -e, --encoding <name>   Input file encoding (e.g. windows-1252, utf-8, iso-8859-1).");
        writer.WriteLine("                          Overrides BOM detection and platform default.");
        writer.WriteLine("  -d, --detect            Detect encoding by analyzing the stream (chunked UTF-8 probe).");
        writer.WriteLine("  -h, --help              Show this help message.");
        writer.WriteLine();
        writer.WriteLine("Encoding resolution order:");
        writer.WriteLine("  1. -e <name>              (explicit, highest priority)");
        writer.WriteLine("  2. BOM                    (if file starts with a byte order mark)");
        writer.WriteLine("  3. -d detect              (chunked UTF-8 strict probe, then platform fallback)");
        writer.WriteLine("  4. Platform default       (Windows: windows-1252, other: utf-8)");
        writer.WriteLine();
        writer.WriteLine("Note: On Windows, files without a BOM are read as windows-1252 by default.");
        writer.WriteLine("      For UTF-8 files without BOM, use: -e utf-8");
    }
}
