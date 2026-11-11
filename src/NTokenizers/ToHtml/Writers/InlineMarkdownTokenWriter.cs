using System.Text;
using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Shared writer for inline Markdown tokens (Bold, Italic, Link, Image, Emoji, etc.).
/// Used by Heading, Blockquote, ListItem, OrderedListItem, and Markdown writers.
/// </summary>
internal class InlineMarkdownTokenWriter : BaseHtmlWriter
{
    internal async Task WriteTokenAsync(MarkdownToken token, TextWriter writer, CancellationToken ct = default)
    {
        var value = token.Value;

        switch (token.TokenType)
        {
            case MarkdownTokenType.Text:
                await WriteValueAsync(writer, value);
                break;

            case MarkdownTokenType.Bold:
                await writer.WriteAsync("<strong>");
                await WriteValueAsync(writer, value);
                await writer.WriteAsync("</strong>");
                break;

            case MarkdownTokenType.Italic:
                await writer.WriteAsync("<em>");
                await WriteValueAsync(writer, value);
                await writer.WriteAsync("</em>");
                break;

            case MarkdownTokenType.BoldItalic:
                await writer.WriteAsync("<strong><em>");
                await WriteValueAsync(writer, value);
                await writer.WriteAsync("</em></strong>");
                break;

            case MarkdownTokenType.Emphasis:
                await writer.WriteAsync("<em>");
                await WriteValueAsync(writer, value);
                await writer.WriteAsync("</em>");
                break;

            case MarkdownTokenType.CodeInline:
                await writer.WriteAsync("<code>");
                await writer.WriteAsync(EscapeHtml(value));
                await writer.WriteAsync("</code>");
                break;

            case MarkdownTokenType.Link:
                if (token.Metadata is LinkMetadata linkMeta)
                {
                    await writer.WriteAsync($"<a href=\"{FormatLinkHref(linkMeta)}\"");
                    if (!string.IsNullOrEmpty(linkMeta.Title))
                    {
                        await writer.WriteAsync($" title=\"{FormatTitle(linkMeta.Title!)}\"");
                    }
                    await writer.WriteAsync(">");
                    if (!string.IsNullOrEmpty(linkMeta.Text))
                    {
                        await WriteValueAsync(writer, linkMeta.Text!, null);
                    }
                    else
                    {
                        await WriteValueAsync(writer, linkMeta.Url!, null);
                    }
                    await writer.WriteAsync("</a>");
                }
                else
                {
                    await WriteValueAsync(writer, value);
                }
                break;

            case MarkdownTokenType.Image:
                if (token.Metadata is LinkMetadata imageMeta)
                {
                    await writer.WriteAsync($"<img src=\"{FormatLinkHref(imageMeta)}\"");
                    if (!string.IsNullOrEmpty(imageMeta.Text))
                        await writer.WriteAsync($" alt=\"{EscapeHtml(imageMeta.Text!)}\"");
                    if (!string.IsNullOrEmpty(imageMeta.Title))
                        await writer.WriteAsync($" title=\"{FormatTitle(imageMeta.Title!)}\"");
                    await writer.WriteAsync(" />");
                }
                else
                {
                    await WriteValueAsync(writer, value);
                }
                break;

            case MarkdownTokenType.Emoji:
                await writer.WriteAsync(value);
                break;

            case MarkdownTokenType.Subscript:
                await writer.WriteAsync("<sub>");
                await WriteValueAsync(writer, value);
                await writer.WriteAsync("</sub>");
                break;

            case MarkdownTokenType.Superscript:
                await writer.WriteAsync("<sup>");
                await WriteValueAsync(writer, value);
                await writer.WriteAsync("</sup>");
                break;

            case MarkdownTokenType.InsertedText:
                await writer.WriteAsync("<ins>");
                await WriteValueAsync(writer, value);
                await writer.WriteAsync("</ins>");
                break;

            case MarkdownTokenType.MarkedText:
                await writer.WriteAsync("<mark>");
                await WriteValueAsync(writer, value);
                await writer.WriteAsync("</mark>");
                break;

            case MarkdownTokenType.Strikethrough:
                await writer.WriteAsync("<del>");
                await WriteValueAsync(writer, value);
                await writer.WriteAsync("</del>");
                break;

            case MarkdownTokenType.FootnoteReference:
                if (token.Metadata is FootnoteMetadata fnMeta)
                {
                    await writer.WriteAsync($"<sup class=\"tok-footnote\">[{fnMeta.Id}]</sup>");
                }
                else
                {
                    await WriteValueAsync(writer, value);
                }
                break;

            case MarkdownTokenType.TypographicReplacement:
                await writer.WriteAsync(value);
                break;

            // Fallback for any inline token not explicitly handled
            default:
                await WriteValueAsync(writer, value);
                break;
        }
    }

    private string FormatLinkHref(LinkMetadata metadata)
    {
        var url = metadata.Url;
        if (metadata.IsAutolink)
        {
            // Autolink hrefs percent-encode backslashes and bracket characters, then escape.
            return FormatAutolinkHref(url);
        }
        if (metadata.IsBracketed)
        {
            url = url.Replace(" ", "%20");
        }
        else
        {
            url = UnescapeBackslashes(url);
        }
        return EscapeHtml(url);
    }

    /// <summary>
    /// Formats an autolink href: backslash, '[' and ']' are percent-encoded (per the
    /// autolink rules), the remaining characters are HTML-escaped.
    /// </summary>
    private string FormatAutolinkHref(string url)
    {
        var result = new StringBuilder(url.Length);
        for (int i = 0; i < url.Length; i++)
        {
            char c = url[i];
            if (c == '\\' || c == '[' || c == ']')
            {
                result.Append('%').Append(((int)c).ToString("X2"));
            }
            else
            {
                result.Append(c);
            }
        }
        return EscapeHtml(result.ToString());
    }

    /// <summary>
    /// Removes backslash escapes from a URL, keeping only the escaped character.
    /// Backslashes before non-ASCII-punctuation characters are preserved.
    /// </summary>
    private static string UnescapeBackslashes(string url)
    {
        var result = new StringBuilder(url.Length);
        for (int i = 0; i < url.Length; i++)
        {
            if (url[i] == '\\' && i + 1 < url.Length)
            {
                char next = url[i + 1];
                if (IsAsciiPunctuation(next))
                {
                    result.Append(next);
                    i++;
                }
                else
                {
                    result.Append(url[i]);
                }
            }
            else
            {
                result.Append(url[i]);
            }
        }
        return result.ToString();
    }

    private string FormatTitle(string title)
    {
        // Escape & before other HTML characters, then escape remaining HTML chars
        var escaped = title
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
        return escaped;
    }

    private static bool IsAsciiPunctuation(char c) => c switch
    {
        '!' or '"' or '#' or '$' or '%' or '&' or '\'' or '(' or ')' or '*' or
        '+' or ',' or '-' or '.' or '/' or ':' or ';' or '<' or '=' or '>' or
        '?' or '@' or '[' or '\\' or ']' or '^' or '_' or '`' or '{' or '|' or
        '}' or '~' => true,
        _ => false
    };
}
