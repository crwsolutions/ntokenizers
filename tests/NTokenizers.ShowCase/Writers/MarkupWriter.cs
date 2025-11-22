using NTokenizers.Markup;
using Spectre.Console;

internal static class MarkupWriter
{
    internal static void Write(MarkupToken token)
    {
        if (token.Metadata is HeadingMetadata meta)
        {
            MarkupHeadingWriter.Write(meta);
        }
        else if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
        {
            CSharpWriter.Write(csharpMeta);
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
            MarkupBlockquoteWriter.Write(blockquoteMeta);
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
            AnsiConsole.WriteLine($"{code}:");
            genericMeta.OnInlineToken = WriteMarkup;
        }
        else
        {
            WriteMarkup(token);
        }
    }

    private static void WriteMarkup(MarkupToken token)
    {
        var value = Markup.Escape(token.Value);
        var colored = token.TokenType switch
        {
            MarkupTokenType.Heading => new Markup($"[yellow]{value}[/]"),
            MarkupTokenType.Bold => new Markup($"[blue]{value}[/]"),
            MarkupTokenType.Italic => new Markup($"[green]{value}[/]"),
            MarkupTokenType.HorizontalRule => new Markup($"[grey]====================================[/]"),
            MarkupTokenType.CodeInline => new Markup($"[cyan]{value}[/]"),
            MarkupTokenType.CodeBlock => new Markup($"[magenta]{value}[/]"),
            MarkupTokenType.Link => new Markup($"[blue underline]{value}[/]"),
            MarkupTokenType.Image => new Markup($"[purple]{value}[/]"),
            MarkupTokenType.Blockquote => new Markup($"[orange1]{value}[/]"),
            MarkupTokenType.UnorderedListItem => new Markup($"[red]{value}[/]"),
            MarkupTokenType.OrderedListItem => new Markup($"[red]{value}[/]"),
            MarkupTokenType.TableCell => new Markup($"[lime]{value}[/]"),
            MarkupTokenType.Emphasis => new Markup($"[yellow]{value}[/]"),
            MarkupTokenType.TypographicReplacement => new Markup($"[grey]{value}[/]"),
            MarkupTokenType.FootnoteReference => new Markup($"[pink]{value}[/]"),
            MarkupTokenType.FootnoteDefinition => new Markup($"[pink]{value}[/]"),
            MarkupTokenType.DefinitionTerm => new Markup($"[bold]{value}[/]"),
            MarkupTokenType.DefinitionDescription => new Markup($"[italic]{value}[/]"),
            MarkupTokenType.Abbreviation => new Markup($"[underline]{value}[/]"),
            MarkupTokenType.CustomContainer => new Markup($"[teal]{value}[/]"),
            MarkupTokenType.HtmlTag => new Markup($"[orange1]{value}[/]"),
            MarkupTokenType.Subscript => new Markup($"[gray]{value}[/]"),
            MarkupTokenType.Superscript => new Markup($"[white]{value}[/]"),
            MarkupTokenType.InsertedText => new Markup($"[green]{value}[/]"),
            MarkupTokenType.MarkedText => new Markup($"[yellow]{value}[/]"),
            MarkupTokenType.Emoji => new Markup($"[yellow]{value}[/]"),
            _ => new Markup(value)
        };
        AnsiConsole.Write(colored);
    }
}

