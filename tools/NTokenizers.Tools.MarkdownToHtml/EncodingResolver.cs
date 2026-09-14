using System.Text;

namespace NTokenizers.Tools.MarkdownToHtml;

/// <summary>
/// Resolves the input encoding for the Markdown to HTML conversion tool.
/// </summary>
public static class EncodingResolver
{
    private const int DetectChunkSize = 64 * 1024;

    private static void EnsureCodePagesRegistered()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// The result of resolving an input encoding.
    /// </summary>
    public sealed record ResolvedEncoding(Encoding Encoding, string Source);

    /// <summary>
    /// Resolves the input encoding based on explicit options, BOM, detection, and platform default.
    /// </summary>
    /// <param name="options">The parsed command line options.</param>
    /// <param name="input">The input stream. The stream position is restored to its original value.</param>
    /// <returns>The resolved encoding and the source that determined it.</returns>
    public static ResolvedEncoding Resolve(CommandLineOptions options, Stream input)
    {
        EnsureCodePagesRegistered();

        if (options.EncodingName is not null)
        {
            return new ResolvedEncoding(GetEncodingOrThrow(options.EncodingName), $"explicit (-e {options.EncodingName})");
        }

        var bomEncoding = DetectBom(input);
        if (bomEncoding is not null)
        {
            return new ResolvedEncoding(bomEncoding, "BOM detected");
        }

        if (options.DetectEncoding)
        {
            var detected = Detect(input);
            return new ResolvedEncoding(detected, "detected (-d)");
        }

        var platformDefault = OperatingSystem.IsWindows()
            ? Encoding.GetEncoding(1252)
            : new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        return new ResolvedEncoding(platformDefault, "platform default");
    }

    /// <summary>
    /// Detects an encoding from a byte order mark, if present.
    /// </summary>
    /// <param name="stream">The input stream. The stream position is restored to its original value.</param>
    /// <returns>The BOM encoding, or null when no supported BOM is present.</returns>
    public static Encoding? DetectBom(Stream stream)
    {
        if (!stream.CanSeek)
        {
            return null;
        }

        long originalPosition = stream.Position;
        byte[] header = new byte[3];
        int bytesRead = stream.Read(header, 0, header.Length);
        stream.Position = originalPosition;

        if (bytesRead == 3 && header[0] == 0xEF && header[1] == 0xBB && header[2] == 0xBF)
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
        }

        if (bytesRead == 2 && header[0] == 0xFF && header[1] == 0xFE)
        {
            return Encoding.Unicode;
        }

        if (bytesRead == 2 && header[0] == 0xFE && header[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode;
        }

        return null;
    }

    /// <summary>
    /// Detects whether the stream is valid UTF-8 by performing a chunked strict UTF-8 decode.
    /// When the stream is not valid UTF-8, the platform fallback encoding is returned.
    /// </summary>
    /// <param name="stream">The input stream. The stream position is restored to its original value.</param>
    /// <returns>The detected encoding.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the stream is not seekable.</exception>
    public static Encoding Detect(Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new InvalidOperationException("Stream must be seekable for encoding detection.");
        }

        long originalPosition = stream.Position;
        var strictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        Decoder decoder = strictUtf8.GetDecoder();
        byte[] buffer = new byte[DetectChunkSize];
        char[] chars = new char[DetectChunkSize];

        try
        {
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                decoder.Convert(buffer, 0, bytesRead, chars, 0, chars.Length, flush: false, out _, out _, out _);
            }

            decoder.Convert(buffer, 0, 0, chars, 0, chars.Length, flush: true, out _, out _, out _);

            stream.Position = originalPosition;
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
        }
        catch (DecoderFallbackException)
        {
            stream.Position = originalPosition;
            return OperatingSystem.IsWindows()
                ? Encoding.GetEncoding(1252)
                : new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        }
    }

    /// <summary>
    /// Gets an encoding by name, or throws a helpful error when the name is not supported.
    /// </summary>
    /// <param name="name">The encoding name, for example utf-8, windows-1252, or iso-8859-1.</param>
    /// <returns>The resolved encoding.</returns>
    /// <exception cref="ArgumentException">Thrown when the encoding name is not supported.</exception>
    public static Encoding GetEncodingOrThrow(string name)
    {
        try
        {
            return Encoding.GetEncoding(name);
        }
        catch (ArgumentException)
        {
            var commonEncodings = new[]
            {
                "utf-8",
                "utf-16",
                "utf-32",
                "windows-1252",
                "iso-8859-1",
                "iso-8859-15",
                "ascii",
            };

            throw new ArgumentException(
                $"Unsupported encoding: {name}. Common supported encoding names include: {string.Join(", ", commonEncodings)}.");
        }
    }
}
