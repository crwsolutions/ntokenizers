using NTokenizers.Markup;
using Spectre.Console.Extensions.NTokenizers.Styles;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public static class MarkupWriter
{
    public static MarkupStyles MarkupStyles { get; } = MarkupStyles.Default;

    public static void Write(MarkupToken token)
    {
        Write(null, token, null);
    }

    public static void Write(Paragraph? liveTarget, MarkupToken token, Style? defaultStyle)
    {
        if (token.Metadata is ICodeBlockMetadata codeBlockMetadata)
        {
            if (!string.IsNullOrEmpty(codeBlockMetadata.Language))
            {
                AnsiConsole.WriteLine($"{codeBlockMetadata.Language}:");
            }
        }

        if (token.Metadata is HeadingMetadata meta)
        {
            var writer = new MarkupHeadingWriter(MarkupStyles.MarkupHeadingStyles);
            writer.Write(meta);
        }
        else if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
        {
            var writer = new CSharpWriter(MarkupStyles.CSharpStyles);
            writer.Write(csharpMeta);
        }
        else if (token.Metadata is XmlCodeBlockMetadata xmlMeta)
        {
            var writer = new XmlWriter(MarkupStyles.XmlStyles);
            writer.Write(xmlMeta);
        }
        else if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
        {
            var writer = new TypescriptWriter(MarkupStyles.TypescriptStyles);
            writer.Write(tsMeta);
        }
        else if (token.Metadata is JsonCodeBlockMetadata jsonMeta)
        {
            var writer = new JsonWriter(MarkupStyles.JsonStyles);
            writer.Write(jsonMeta);
        }
        else if (token.Metadata is SqlCodeBlockMetadata sqlMeta)
        {
            var writer = new SqlWriter(MarkupStyles.SqlStyles);
            writer.Write(sqlMeta);
        }
        else if (token.Metadata is LinkMetadata linkMeta)
        {
            MarkupLinkWriter.Write(linkMeta);
        }
        else if (token.Metadata is BlockquoteMetadata blockquoteMeta)
        {
            var writer = new MarkupBlockquoteWriter();
            writer.Write(blockquoteMeta);
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
            writer.Write(orderedListItemMeta);
        }
        else if (token.Metadata is ListItemMetadata listItemMeta)
        {
            var writer = new MarkupListItemWriter(MarkupStyles.MarkupListItemStyles);
            writer.Write(listItemMeta);
        }
        else if (token.Metadata is GenericCodeBlockMetadata genericMeta)
        {
            var code = string.IsNullOrWhiteSpace(genericMeta.Language) ? "code" : genericMeta.Language;
            Write(liveTarget, $"{code}:");
            genericMeta.OnInlineToken = (token) => WriteMarkup(liveTarget, token, defaultStyle);
        }
        else
        {
            WriteMarkup(liveTarget, token, defaultStyle);
        }
    }

    private static void Write(Paragraph? liveTarget, string value, Style? style = null)
    {
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

    private static void WriteMarkup(Paragraph? liveTarget, MarkupToken token, Style? defaultStyle)
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

