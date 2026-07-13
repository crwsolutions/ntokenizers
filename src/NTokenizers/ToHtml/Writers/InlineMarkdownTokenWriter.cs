using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Shared writer for inline Markdown tokens (Bold, Italic, Link, Image, Emoji, etc.).
/// Used by Heading, Blockquote, ListItem, OrderedListItem, and Markdown writers.
/// </summary>
internal class InlineMarkdownTokenWriter : BaseHtmlWriter
{
    internal void WriteToken(MarkdownToken token, TextWriter writer)
    {
        var value = token.Value;

        switch (token.TokenType)
        {
            case MarkdownTokenType.Text:
                WriteValue(writer, value, null);
                break;

            case MarkdownTokenType.Bold:
                writer.Write("<strong>");
                WriteValue(writer, value, "tok-bold");
                writer.Write("</strong>");
                break;

            case MarkdownTokenType.Italic:
                writer.Write("<em>");
                WriteValue(writer, value, "tok-italic");
                writer.Write("</em>");
                break;

            case MarkdownTokenType.Emphasis:
                writer.Write("<em>");
                WriteValue(writer, value, "tok-italic");
                writer.Write("</em>");
                break;

            case MarkdownTokenType.CodeInline:
                writer.Write("<code>");
                writer.Write(EscapeHtml(value));
                writer.Write("</code>");
                break;

            case MarkdownTokenType.Link:
                if (token.Metadata is LinkMetadata linkMeta)
                {
                    writer.Write($"<a class=\"tok-link\" href=\"{EscapeHtml(linkMeta.Url)}\"");
                    if (!string.IsNullOrEmpty(linkMeta.Title))
                        writer.Write($" title=\"{EscapeHtml(linkMeta.Title)}\"");
                    writer.Write(">");
                    if (!string.IsNullOrEmpty(linkMeta.Text))
                    {
                        WriteValue(writer, linkMeta.Text, null);
                    }
                    else
                    {
                        WriteValue(writer, linkMeta.Url, null);
                    }
                    writer.Write("</a>");
                }
                else
                {
                    WriteValue(writer, value, null);
                }
                break;

            case MarkdownTokenType.Image:
                if (token.Metadata is LinkMetadata imageMeta)
                {
                    writer.Write($"<img src=\"{EscapeHtml(imageMeta.Url)}\"");
                    if (!string.IsNullOrEmpty(value))
                        writer.Write($" alt=\"{EscapeHtml(value)}\"");
                    if (!string.IsNullOrEmpty(imageMeta.Title))
                        writer.Write($" title=\"{EscapeHtml(imageMeta.Title)}\"");
                    writer.Write(" />");
                }
                else
                {
                    WriteValue(writer, value, null);
                }
                break;

            case MarkdownTokenType.Emoji:
                writer.Write(value);
                break;

            case MarkdownTokenType.Subscript:
                writer.Write("<sub>");
                WriteValue(writer, value, "tok-subscript");
                writer.Write("</sub>");
                break;

            case MarkdownTokenType.Superscript:
                writer.Write("<sup>");
                WriteValue(writer, value, "tok-superscript");
                writer.Write("</sup>");
                break;

            case MarkdownTokenType.InsertedText:
                writer.Write("<ins>");
                WriteValue(writer, value, "tok-inserted");
                writer.Write("</ins>");
                break;

            case MarkdownTokenType.MarkedText:
                writer.Write("<mark>");
                WriteValue(writer, value, "tok-marked");
                writer.Write("</mark>");
                break;

            case MarkdownTokenType.FootnoteReference:
                if (token.Metadata is FootnoteMetadata fnMeta)
                {
                    writer.Write($"<sup class=\"tok-footnote\">[{fnMeta.Id}]</sup>");
                }
                else
                {
                    WriteValue(writer, value, null);
                }
                break;

            case MarkdownTokenType.TypographicReplacement:
                writer.Write(value);
                break;

            // Fallback for any inline token not explicitly handled
            default:
                WriteValue(writer, value, null);
                break;
        }
    }
}
