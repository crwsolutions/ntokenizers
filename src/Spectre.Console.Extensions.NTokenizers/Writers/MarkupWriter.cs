using NTokenizers.Markup;
using NTokenizers.Markup.Metadata;
using Spectre.Console.Extensions.NTokenizers.Styles;
using System.Diagnostics;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal class MarkupWriter
{
    internal static MarkupStyles MarkupStyles { get; set; } = MarkupStyles.Default;

    internal static MarkupWriter Create() => new();

    internal async Task WriteAsync(MarkupToken token) => 
        await WriteAsync(null, token, null);

    internal async Task WriteAsync(Paragraph? liveTarget, MarkupToken token, Style? defaultStyle)
    {
        if (token.Metadata is ICodeBlockMetadata codeBlockMetadata)
        {
            var code = string.IsNullOrWhiteSpace(codeBlockMetadata.Language) ? "code" : codeBlockMetadata.Language;
            AnsiConsole.WriteLine($"{code}:");
        }
        if (token.Metadata is HeadingMetadata meta)
        {
            var writer = new MarkupHeadingWriter(MarkupStyles.MarkupHeadingStyles);
            await writer.Write(meta);
        }
        else if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
        {
            var writer = new CSharpWriter(MarkupStyles.CSharpStyles);
            await writer.Write(csharpMeta);
        }
        else if (token.Metadata is XmlCodeBlockMetadata xmlMeta)
        {
            var writer = new XmlWriter(MarkupStyles.XmlStyles);
            await writer.Write(xmlMeta);
        }
        else if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
        {
            var writer = new TypescriptWriter(MarkupStyles.TypescriptStyles);
            await writer.Write(tsMeta);
        }
        else if (token.Metadata is JsonCodeBlockMetadata jsonMeta)
        {
            var writer = new JsonWriter(MarkupStyles.JsonStyles);
            await writer.Write(jsonMeta);
        }
        else if (token.Metadata is SqlCodeBlockMetadata sqlMeta)
        {
            var writer = new SqlWriter(MarkupStyles.SqlStyles);
            await writer.Write(sqlMeta);
        }
        else if (token.Metadata is GenericCodeBlockMetadata genericMeta)
        {
            var writer = new GenericWriter();
            await writer.Write(genericMeta);
        }
        else if (token.Metadata is LinkMetadata linkMeta)
        {
            MarkupLinkWriter.Write(linkMeta);
        }
        else if (token.Metadata is BlockquoteMetadata blockquoteMeta)
        {
            var writer = new MarkupBlockquoteWriter();
            await writer.Write(blockquoteMeta);
        }
        else if (token.Metadata is FootnoteMetadata footnoteMeta)
        {
            MarkupFootnoteWriter.Write(footnoteMeta);
        }
        else if (token.Metadata is EmojiMetadata emojiMeta)
        {
            MarkupEmojiWriter.Write(emojiMeta);
        }
        else if (token.Metadata is OrderedListItemMetadata orderedListItemMeta)
        {
            var writer = new MarkupOrderedListItemWriter(MarkupStyles.MarkupOrderedListItemStyles);
            await writer.WriteAsync(orderedListItemMeta);
        }
        else if (token.Metadata is ListItemMetadata listItemMeta)
        {
            var writer = new MarkupListItemWriter(MarkupStyles.MarkupListItemStyles);
            await writer.WriteAsync(listItemMeta);
        }
        else if (token.Metadata is TableMetadata tableMeta)
        {
            var writer = new MarkupTableWriter(MarkupStyles);
            await writer.WriteAsync(tableMeta);
        }
        else
        {
            WriteMarkup(liveTarget, token, defaultStyle);
        }
    }

    private void Write(Paragraph? liveTarget, string value, Style? style = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }
        Debug.WriteLine($"Writing token: `{value}` with style `[{style?.Foreground}/{style?.Background}]`");

        var text = Markup.Escape(value);
        if (liveTarget is not null)
        {
            liveTarget.Append(text, style);
        }
        else if (style is not null)
        {
            AnsiConsole.Write(new Markup(text, style));
        }
        else
        {
            AnsiConsole.Write(text);
        }
    }

    internal void WriteMarkup(Paragraph? liveTarget, MarkupToken token, Style? defaultStyle)
    {
        var style = token.TokenType switch
        {
            MarkupTokenType.Heading => MarkupStyles.Heading,
            MarkupTokenType.Bold => MarkupStyles.Bold,
            MarkupTokenType.Italic => MarkupStyles.Italic,
            MarkupTokenType.HorizontalRule => MarkupStyles.HorizontalRule,
            MarkupTokenType.CodeInline => MarkupStyles.CodeInline,
            MarkupTokenType.CodeBlock => MarkupStyles.CodeBlock,
            MarkupTokenType.Link => MarkupStyles.Link,
            MarkupTokenType.Image => MarkupStyles.Image,
            MarkupTokenType.Blockquote => MarkupStyles.Blockquote,
            MarkupTokenType.UnorderedListItem => MarkupStyles.UnorderedListItem,
            MarkupTokenType.OrderedListItem => MarkupStyles.OrderedListItem,
            MarkupTokenType.TableCell => MarkupStyles.TableCell,
            MarkupTokenType.Emphasis => MarkupStyles.Emphasis,
            MarkupTokenType.TypographicReplacement => MarkupStyles.TypographicReplacement,
            MarkupTokenType.FootnoteReference => MarkupStyles.FootnoteReference,
            MarkupTokenType.FootnoteDefinition => MarkupStyles.FootnoteDefinition,
            MarkupTokenType.DefinitionTerm => MarkupStyles.DefinitionTerm,
            MarkupTokenType.DefinitionDescription => MarkupStyles.DefinitionDescription,
            MarkupTokenType.Abbreviation => MarkupStyles.Abbreviation,
            MarkupTokenType.CustomContainer => MarkupStyles.CustomContainer,
            MarkupTokenType.HtmlTag => MarkupStyles.HtmlTag,
            MarkupTokenType.Subscript => MarkupStyles.Subscript,
            MarkupTokenType.Superscript => MarkupStyles.Superscript,
            MarkupTokenType.InsertedText => MarkupStyles.InsertedText,
            MarkupTokenType.MarkedText => MarkupStyles.MarkedText,
            MarkupTokenType.Emoji => MarkupStyles.Emoji,
            _ => defaultStyle ?? MarkupStyles.DefaultStyle
        };

        if (token.TokenType == MarkupTokenType.HorizontalRule)
        {
            var value = new string('─', System.Console.WindowWidth);
            Write(liveTarget, value, style);
        }
        else
        {
            Write(liveTarget, token.Value, style);
        }
    }
}

