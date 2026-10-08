using NTokenizers.C;
using NTokenizers.Cpp;
using NTokenizers.CSharp;
using NTokenizers.Css;
using NTokenizers.Generic;
using NTokenizers.Go;
using NTokenizers.Html;
using NTokenizers.Java;
using NTokenizers.Json;
using NTokenizers.Kotlin;
using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;
using NTokenizers.Rust;
using NTokenizers.Sql;
using NTokenizers.Swift;
using NTokenizers.Toml;
using NTokenizers.Typescript;
using NTokenizers.Xml;
using System.Linq;
using System.Text;

namespace Markdown;

public class MarkdownTokenizerListsTests
{
    private static (List<MarkdownToken> tokens, string text) Tokenize(string markdown)
    {
        var tokens = new List<MarkdownToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));

        // Recursively registers inline token handlers for markdown tokens. A token that
        // carries InlineMetadata<MarkdownToken> (heading, blockquote, list item, indented
        // code block, table) can itself contain markdown tokens - most importantly, a
        // blockquote can nest another blockquote. Registering the handler here (recursively)
        // ensures the whole token tree is captured in `tokens`; a flat registration would
        // leave nested block content unhandled.
        static void RegisterInlineHandlers(MarkdownToken token, List<MarkdownToken> tokens)
        {
            if (token.Metadata is HeadingMetadata headingMeta)
            {
                headingMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens);
                });
            }
            else if (token.Metadata is BlockquoteMetadata blockquoteMeta)
            {
                blockquoteMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens);
                });
            }
            else if (token.Metadata is ListItemMetadata listMeta)
            {
                listMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens);
                });
            }
            else if (token.Metadata is OrderedListItemMetadata orderedListMeta)
            {
                orderedListMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens);
                });
            }
            else if (token.Metadata is IndentedCodeBlockMetadata indentedCodeMeta)
            {
                // Indented code blocks stream plain markdown Text tokens.
                indentedCodeMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is TableMetadata tableMeta)
            {
                tableMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens);
                });
            }
        }
        var result = MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            tokens.Add(token);
            RegisterInlineHandlers(token, tokens);

            // Code block metadata streams language-specific (non-markdown) tokens; the
            // no-op handlers below let the parser stream and discard that content.
            if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
            {
                // For C# code blocks, we receive CSharpToken objects
                csharpMeta.RegisterInlineTokenHandler(token => { /* Capture C# tokens if needed */ });
            }
            else if (token.Metadata is JavaCodeBlockMetadata javaMeta)
            {
                javaMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is CCodeBlockMetadata cMeta)
            {
                cMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is CppCodeBlockMetadata cppMeta)
            {
                cppMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is RustCodeBlockMetadata rustMeta)
            {
                rustMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is KotlinCodeBlockMetadata kotlinMeta)
            {
                kotlinMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is GoCodeBlockMetadata goMeta)
            {
                goMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is SwiftCodeBlockMetadata swiftMeta)
            {
                swiftMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is JsonCodeBlockMetadata jsonMeta)
            {
                // For JSON code blocks, we receive JsonToken objects
                jsonMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is XmlCodeBlockMetadata xmlMeta)
            {
                // For XML code blocks, we receive XmlToken objects
                xmlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is HtmlCodeBlockMetadata htmlMeta)
            {
                // For XML code blocks, we receive XmlToken objects
                htmlMeta.RegisterInlineTokenHandler(token =>
                {
                    if (token.Metadata is CssCodeBlockMetadata cssMetadata)
                    {
                        cssMetadata.RegisterInlineTokenHandler(token => { });
                    }
                    else if (token.Metadata is TypeScriptCodeBlockMetadata tsMetadata)
                    {
                        tsMetadata.RegisterInlineTokenHandler(token => { });
                    }
                });
            }
            else if (token.Metadata is SqlCodeBlockMetadata sqlMeta)
            {
                // For SQL code blocks, we receive SqlToken objects
                sqlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
            {
                // For TypeScript code blocks, we receive TypescriptToken objects
                tsMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is TomlCodeBlockMetadata tomlMeta)
            {
                // For TOML code blocks, we receive TomlToken objects
                tomlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is GenericCodeBlockMetadata gMeta)
            {
                gMeta.RegisterInlineTokenHandler(token => { });
            }
        }).GetAwaiter().GetResult();
        return (tokens, result);
    }

    [Fact]
    public void TestUnorderedListWithDash()
    {
        var markdown = "- item 1";
        var (tokens, text) = Tokenize(markdown);
        // P2: the item is a paragraph sub-document within a balanced list group.
        Assert.Equal(7, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.IsType<ListMetadata>(tokens[0].Metadata);
        Assert.False(Assert.IsType<ListMetadata>(tokens[0].Metadata).IsOrdered);

        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // Unordered list items have empty value
        Assert.Equal('-', Assert.IsType<ListItemMetadata>(tokens[1].Metadata).Marker);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("item ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("1", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[5].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[6].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithPlus()
    {
        var markdown = "+ item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(7, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal('+', Assert.IsType<ListItemMetadata>(tokens[1].Metadata).Marker);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("item ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("1", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[5].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[6].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithAsterisk()
    {
        var markdown = "* item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(7, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // Unordered list items have empty value
        Assert.Equal('*', Assert.IsType<ListItemMetadata>(tokens[1].Metadata).Marker);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("item ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("1", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[5].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[6].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithLeadingSpaces()
    {
        // Up to three leading spaces are part of the list item; four or more make the
        // line an indented code block (CommonMark 4.4), so this is code, not a list item.
        // The leading indentation is emitted as a whitespace Text token on the list level
        // (decoration, not item content); the item Value stays empty.
        var markdown = "   * item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(8, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("   ", tokens[1].Value); // Leading indentation as whitespace text
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal('*', Assert.IsType<ListItemMetadata>(tokens[2].Metadata).Marker);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[3].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("item ", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[5].TokenType);
        Assert.Equal("1", tokens[5].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[6].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[7].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListWithLeadingSpaces()
    {
        var markdown = "  1. item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(8, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.IsType<ListMetadata>(tokens[0].Metadata);
        Assert.True(Assert.IsType<ListMetadata>(tokens[0].Metadata).IsOrdered);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("  ", tokens[1].Value); // Leading indentation as whitespace text
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.NotNull(tokens[2].Metadata);
        var orderedMeta = Assert.IsType<OrderedListItemMetadata>(tokens[2].Metadata);
        Assert.Equal(1, orderedMeta.Number);
        Assert.Equal('.', orderedMeta.Marker);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[3].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("item ", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[5].TokenType);
        Assert.Equal("1", tokens[5].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[6].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[7].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestFlatListGrouping()
    {
        // Consecutive list markers at the item's content offset become nested sub-document
        // lists, while the shallower marker is a sibling in the outer list.
        var markdown = "- top\n  - nested\n   - deeper";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown, text);
        // A nested list interrupts the item's paragraph the same way a sibling item does: the
        // newline the nested marker sits on is consumed, not emitted as a Text("\n") separator.
        Assert.Equal(17, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[^1].TokenType);

        // Exactly one list group with three items and their content in order.
        var items = tokens.Where(t => t.TokenType == MarkdownTokenType.UnorderedListItem).ToList();
        Assert.Equal(3, items.Count);
        var content = tokens.Where(t => t.TokenType == MarkdownTokenType.Text)
            .Select(t => t.Value).ToList();
        Assert.Contains("top", content);
        Assert.Contains("nested", content);
        Assert.Contains("deeper", content);
    }

    [Fact]
    public void TestUnorderedMarkerMixing()
    {
        // CommonMark: the unordered marker (-, *, +) may change between items of one list.
        var markdown = "- a\n* b\n+ c";
        var (tokens, text) = Tokenize(markdown);
        // One balanced list group with three items carrying their own markers.
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[^1].TokenType);

        var items = tokens.Where(t => t.TokenType == MarkdownTokenType.UnorderedListItem).ToList();
        Assert.Equal(3, items.Count);
        Assert.Equal(new[] { '-', '*', '+' }, items.Select(i => Assert.IsType<ListItemMetadata>(i.Metadata).Marker));

        // No whitespace text between tightly packed items (the line separators are consumed).
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListGrouping()
    {
        var markdown = "1. a\n2. b";
        var (tokens, text) = Tokenize(markdown);
        // Each item's own closing line break is consumed, not emitted (a list item is a box
        // that already closes on a fresh line), so the stream is marker + paragraph only.
        Assert.Equal(10, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.True(Assert.IsType<ListMetadata>(tokens[0].Metadata).IsOrdered);

        var items = tokens.Where(t => t.TokenType == MarkdownTokenType.OrderedListItem).ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal(1, Assert.IsType<OrderedListItemMetadata>(items[0].Metadata).Number);
        Assert.Equal(2, Assert.IsType<OrderedListItemMetadata>(items[1].Metadata).Number);

        Assert.Equal(MarkdownTokenType.ListEnd, tokens[^1].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListParenDelimiter()
    {
        // The ')' delimiter is accepted; the delimiter is carried on each item's metadata.
        var markdown = "1) a\n2) b";
        var (tokens, text) = Tokenize(markdown);
        var items = tokens.Where(t => t.TokenType == MarkdownTokenType.OrderedListItem).ToList();
        Assert.Equal(2, items.Count);
        Assert.All(items, i => Assert.Equal(')', Assert.IsType<OrderedListItemMetadata>(i.Metadata).Marker));
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[^1].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedDelimiterSwitchStartsNewList()
    {
        // A '.'-item followed by a ')'-item: the delimiter switch closes the first list and
        // opens a second (two <ol> groups), per the approved grouping rule.
        var markdown = "1. a\n2) b";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count(t => t.TokenType == MarkdownTokenType.ListStart));
        Assert.Equal(2, tokens.Count(t => t.TokenType == MarkdownTokenType.ListEnd));
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestListTypeSwitchStartsNewList()
    {
        // An unordered run followed by an ordered run produces two separate list groups,
        // with the separating blank line preserved as a whitespace Text token.
        var markdown = "* a\n\n1. b";
        var (tokens, text) = Tokenize(markdown);
        var listStarts = tokens.Where(t => t.TokenType == MarkdownTokenType.ListStart).ToList();
        Assert.Equal(2, listStarts.Count);
        Assert.False(Assert.IsType<ListMetadata>(listStarts[0].Metadata).IsOrdered);
        Assert.True(Assert.IsType<ListMetadata>(listStarts[1].Metadata).IsOrdered);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestBlankLineBetweenItemsIsPreserved()
    {
        // A blank line continues the first item's sub-document when its next line is
        // indented to the content offset; the blank run before the sibling is handed back
        // to the parent as whitespace Text.
        var markdown = "- a\n\n  continued\n\n- b";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[^1].TokenType);

        var whitespace = tokens.Where(t => t.TokenType == MarkdownTokenType.Text && t.Value == "\n").ToList();
        // Item 1's internal blank line is preserved as two Text("\n") tokens (the paragraph
        // closing newline plus the blank line's own), and the blank line before the sibling
        // item is preserved at the list level. Each item's own closing line break (the line
        // ending of the item's final line) is consumed, not emitted: a list item is a box
        // that already closes on a fresh line.
        Assert.Equal(3, whitespace.Count);

        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnindentedBlockquoteAfterBlankLineEndsListAndPreservesWhitespace()
    {
        var markdown = "- Item 1\n  * Nested item\n- Item 2\n\n> ## Css example";
        var (tokens, text) = Tokenize(markdown);
        var blockquoteIndex = tokens.FindIndex(token => token.TokenType == MarkdownTokenType.Blockquote);

        Assert.Equal(markdown, text);
        Assert.True(blockquoteIndex > 0);

        var listEndIndex = tokens.FindLastIndex(blockquoteIndex - 1, token => token.TokenType == MarkdownTokenType.ListEnd);
        Assert.True(listEndIndex >= 0);
        var itemTextIndex = tokens.FindLastIndex(listEndIndex - 1, token => token.TokenType == MarkdownTokenType.Text && token.Value == "2");
        var separatorIndex = tokens.FindIndex(itemTextIndex + 1, token => token.TokenType == MarkdownTokenType.Text && token.Value == "\n");
        Assert.True(itemTextIndex >= 0);
        Assert.True(separatorIndex > itemTextIndex && separatorIndex < listEndIndex);
    }

    [Fact]
    public void TestIndentedBlockquoteAfterBlankLineRemainsInListItem()
    {
        var markdown = "- Item 1\n\n  > Nested quote";
        var (tokens, text) = Tokenize(markdown);
        var blockquoteIndex = tokens.FindIndex(token => token.TokenType == MarkdownTokenType.Blockquote);
        var listEndIndex = tokens.FindLastIndex(token => token.TokenType == MarkdownTokenType.ListEnd);

        Assert.Equal(markdown, text);
        Assert.True(blockquoteIndex >= 0);
        Assert.True(blockquoteIndex < listEndIndex);
    }

    [Fact]
    public void TestTenDigitMarkerIsNotAListItem()
    {
        // Ten or more digits before the delimiter is not a list marker (deviation: capped at
        // nine digits to keep the value within a 32-bit number); the line stays a paragraph.
        var markdown = "1234567890. foo";
        var (tokens, text) = Tokenize(markdown);
        Assert.DoesNotContain(tokens, t => t.TokenType == MarkdownTokenType.ListStart);
        Assert.DoesNotContain(tokens, t => t.TokenType == MarkdownTokenType.OrderedListItem);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedList()
    {
        var markdown = "1. item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(7, tokens.Count); // ListStart + item paragraph + ListEnd
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // Value is empty when OnInlineToken is used
        Assert.NotNull(tokens[1].Metadata);
        var orderedMeta = Assert.IsType<OrderedListItemMetadata>(tokens[1].Metadata);
        Assert.Equal(1, orderedMeta.Number);

        // Inline content
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("item ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("1", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[5].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[6].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListMultipleDigits()
    {
        var markdown = "42. item";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count); // ListStart + item paragraph + ListEnd
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(42, Assert.IsType<OrderedListItemMetadata>(tokens[1].Metadata).Number);

        // Inline content
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("item", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[4].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[5].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestNestedOrderedListWithDifferentNumber()
    {
        // A nested ordered marker with a start number other than 1 opens a nested list
        // inside the item. This deviates from CommonMark (where only "1." interrupts an
        // open paragraph): inside a list item an indented ordered marker unambiguously
        // starts a nested list, so any start number is accepted.
        var markdown = "1. AA\n   2. AA";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown, text);

        // Two balanced, ordered list groups (outer + nested) and two items numbered 1 and 2.
        Assert.Equal(2, tokens.Count(t => t.TokenType == MarkdownTokenType.ListStart));
        Assert.Equal(2, tokens.Count(t => t.TokenType == MarkdownTokenType.ListEnd));
        Assert.All(tokens.Where(t => t.TokenType == MarkdownTokenType.ListStart),
            t => Assert.True(Assert.IsType<ListMetadata>(t.Metadata).IsOrdered));
        var items = tokens.Where(t => t.TokenType == MarkdownTokenType.OrderedListItem).ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal(1, Assert.IsType<OrderedListItemMetadata>(items[0].Metadata).Number);
        Assert.Equal(2, Assert.IsType<OrderedListItemMetadata>(items[1].Metadata).Number);
    }

    [Fact]
    public void TestNestedOrderedListHasNoSeparatorNewline()
    {
        // The newline between the item's paragraph and the nested ordered list is consumed,
        // not emitted as a Text("\n") separator (a list item renders as a box that already
        // closes on a fresh line). The stream carries the two content lines and no bare
        // newline.
        var markdown = "1. AA\n   2. AA";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown, text);
        Assert.DoesNotContain(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "\n");
        Assert.Equal(new[] { "AA", "AA" },
            tokens.Where(t => t.TokenType == MarkdownTokenType.Text).Select(t => t.Value).ToArray());
    }

    [Fact]
    public void TestNestedOrderedListKeepsTopLevelSiblingSeparate()
    {
        // A nested ordered list closes when a shallower sibling marker follows, so the
        // sibling belongs to the outer list, not the nested one.
        var markdown = "1. AA\n   2. BB\n3. CC";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown, text);

        Assert.Equal(2, tokens.Count(t => t.TokenType == MarkdownTokenType.ListStart));
        var items = tokens.Where(t => t.TokenType == MarkdownTokenType.OrderedListItem).ToList();
        Assert.Equal(3, items.Count);
        Assert.Equal(new[] { 1, 2, 3 },
            items.Select(i => Assert.IsType<OrderedListItemMetadata>(i.Metadata).Number).ToArray());
        Assert.DoesNotContain(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "\n");
    }

    [Fact]
    public void TestTopLevelOrderedMarkerDoesNotInterruptParagraph()
    {
        // Outside a list item, only a "1." marker interrupts an open paragraph (CommonMark
        // example 303); the relaxation to any start number applies only inside list items.
        // So "2." here stays a lazy continuation line and the whole input is one paragraph.
        var markdown = "AA\n2. BB";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown, text);
        Assert.DoesNotContain(tokens, t => t.TokenType == MarkdownTokenType.ListStart);
        Assert.DoesNotContain(tokens, t => t.TokenType == MarkdownTokenType.OrderedListItem);
        Assert.Equal(1, tokens.Count(t => t.TokenType == MarkdownTokenType.ParagraphBlockStart));
        Assert.Equal(1, tokens.Count(t => t.TokenType == MarkdownTokenType.ParagraphBlockEnd));
    }

    [Fact]
    public void TestNestedUnorderedListHasNoSeparatorNewline()
    {
        // A nested unordered list after a sibling item interrupts the item's paragraph the
        // same way a sibling does: the newline the nested marker sits on is consumed, not
        // emitted as a Text("\n") separator (a list item renders as a box that already
        // closes on a fresh line). The nested "- C" list belongs inside item "B", and the
        // stream carries no bare newline.
        var markdown = "+ A\n+ B\n  - C";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown, text);

        Assert.Equal(2, tokens.Count(t => t.TokenType == MarkdownTokenType.ListStart));
        Assert.Equal(2, tokens.Count(t => t.TokenType == MarkdownTokenType.ListEnd));
        var items = tokens.Where(t => t.TokenType == MarkdownTokenType.UnorderedListItem).ToList();
        Assert.Equal(3, items.Count);
        Assert.DoesNotContain(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "\n");
        Assert.Equal(new[] { "A", "B", "C" },
            tokens.Where(t => t.TokenType == MarkdownTokenType.Text).Select(t => t.Value).ToArray());
    }
}
