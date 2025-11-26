using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupWriter
{
    internal static void Write(MarkupToken token)
    {
        Write(null, token, null);
    }

    internal static void Write(Paragraph? liveTarget, MarkupToken token, Style? defaultStyle)
    {
        if (token.Metadata is HeadingMetadata meta)
        {
            var writer = new MarkupHeadingWriter();
            writer.Write(meta);
        }
        else if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
        {
            var writer = new CSharpWriter();
            writer.Write(csharpMeta);
        }
        else if (token.Metadata is XmlCodeBlockMetadata xmlMeta)
        {
            XmlWriter.Write(xmlMeta);
        }
        else if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
        {
            TypescriptWriter.Write(tsMeta);
        }
        else if (token.Metadata is JsonCodeBlockMetadata jsonMeta)
        {
            JsonWriter.Write(jsonMeta);
        }
        else if (token.Metadata is SqlCodeBlockMetadata sqlMeta)
        {
            SqlWriter.Write(sqlMeta);
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
            MarkupOrderedListItemWriter.Write(orderedListItemMeta);
        }
        else if (token.Metadata is ListItemMetadata listItemMeta)
        {
            MarkupListItemWriter.Write(listItemMeta);
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
            MarkupTokenType.Heading => new Style(Color.Yellow),
            MarkupTokenType.Bold => new Style(Color.Blue, decoration: Decoration.Bold),
            MarkupTokenType.Italic => new Style(Color.Green, decoration: Decoration.Italic),
            MarkupTokenType.HorizontalRule => new Style(Color.Grey),
            MarkupTokenType.CodeInline => new Style(Color.Cyan),
            MarkupTokenType.CodeBlock => new Style(Color.Magenta),
            MarkupTokenType.Link => new Style(Color.Blue, decoration: Decoration.Underline),
            MarkupTokenType.Image => new Style(Color.Purple),
            MarkupTokenType.Blockquote => new Style(Color.Orange1),
            MarkupTokenType.UnorderedListItem => new Style(Color.Red),
            MarkupTokenType.OrderedListItem => new Style(Color.Red),
            MarkupTokenType.TableCell => new Style(Color.Lime),
            MarkupTokenType.Emphasis => new Style(Color.Yellow, decoration: Decoration.Italic),
            MarkupTokenType.TypographicReplacement => new Style(Color.Grey),
            MarkupTokenType.FootnoteReference => new Style(Color.Pink1),
            MarkupTokenType.FootnoteDefinition => new Style(Color.Pink1),
            MarkupTokenType.DefinitionTerm => new Style(decoration: Decoration.Bold),
            MarkupTokenType.DefinitionDescription => new Style(decoration: Decoration.Italic),
            MarkupTokenType.Abbreviation => new Style(decoration: Decoration.Underline),
            MarkupTokenType.CustomContainer => new Style(Color.Teal),
            MarkupTokenType.HtmlTag => new Style(Color.Orange1),
            MarkupTokenType.Subscript => new Style(Color.Grey),
            MarkupTokenType.Superscript => new Style(Color.White),
            MarkupTokenType.InsertedText => new Style(Color.Green),
            MarkupTokenType.MarkedText => new Style(Color.Yellow),
            MarkupTokenType.Emoji => new Style(Color.Yellow),
            _ => defaultStyle ?? new Style() // default style
        };

        if (token.TokenType == MarkupTokenType.HorizontalRule)
        {
            var value = new string('─', Console.WindowWidth);
            Write(liveTarget, value, style);
        }
        else
        { 
            Write(liveTarget, token.Value, style);
        }
    }
}

