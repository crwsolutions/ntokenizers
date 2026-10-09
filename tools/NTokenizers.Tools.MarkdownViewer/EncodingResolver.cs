using System.Text;
using System.Text.Encodings.Web;

namespace NTokenizers_Tools_MarkdownViewer;

/// <summary>
/// Resolves the input encoding for Markdown files.
/// Detection order: BOM → strict UTF-8 validation → platform default.
/// </summary>
public static class EncodingResolver
{
    private const int DetectChunkSize = 64 * 1024;

    /// <summary>
    /// The result of resolving an input encoding.
    /// </summary>
    public sealed record ResolvedEncoding(Encoding Encoding, string Source);

    /// <summary>
    /// Resolves the input encoding by checking BOM, then strict UTF-8, then platform default.
    /// </summary>
    /// <param name="input">The input stream. The stream position is restored to its original value.</param>
    /// <returns>The resolved encoding and the source that determined it.</returns>
    public static ResolvedEncoding Resolve(Stream input)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var bomEncoding = DetectBom(input);
        if (bomEncoding is not null)
        {
            return new ResolvedEncoding(bomEncoding, "BOM detected");
        }

        var detected = Detect(input);
        return new ResolvedEncoding(detected, "detected");
    }

    private static Encoding? DetectBom(Stream stream)
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

    private static Encoding Detect(Stream stream)
    {
        if (!stream.CanSeek)
        {
            return OperatingSystem.IsWindows()
                ? Encoding.GetEncoding(1252)
                : new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
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
}
