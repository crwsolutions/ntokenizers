using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;

namespace Markdown;

public class MarkdownTokenizerListsTests : MarkdownTokenizerTestBase
{
    private static (List<MarkdownToken> tokens, string text) Tokenize(string markdown)
    {
        var (tokens, text, _) = MarkdownTokenizerTestBase.Tokenize(markdown);
        return (tokens, text);
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
    public void TestTopLevelOrderedMarkerInterruptsParagraph()
    {
        // Deviation from CommonMark (example 303): a list marker is a valid
        // paragraph-interrupting line-start construct, so any start number (not only
        // "1.") interrupts an open paragraph instead of lazy-continuing it.
        var markdown = "B\n4. A";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown, text);

        // The paragraph's held soft break (its closing newline) is flushed as a Text("\n")
        // separator after the ParagraphBlockEnd — the house paragraph-boundary rule, same as
        // for a heading or blockquote after a paragraph.
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("B", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("\n", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[4].TokenType);
        Assert.True(Assert.IsType<ListMetadata>(tokens[4].Metadata).IsOrdered);

        var item = Assert.Single(tokens, t => t.TokenType == MarkdownTokenType.OrderedListItem);
        Assert.Equal(4, Assert.IsType<OrderedListItemMetadata>(item.Metadata).Number);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[^1].TokenType);
    }

    [Fact]
    public void TestTopLevelUnorderedMarkerInterruptsParagraph()
    {
        // A list marker is a valid paragraph-interrupting line-start construct, so an
        // unordered marker interrupts an open paragraph the same way an ordered one does.
        var markdown = "B\n- A";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown, text);

        // The paragraph's closing newline is flushed as a Text("\n") separator after the
        // ParagraphBlockEnd (house paragraph-boundary rule).
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("\n", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[4].TokenType);
        Assert.False(Assert.IsType<ListMetadata>(tokens[4].Metadata).IsOrdered);

        var item = Assert.Single(tokens, t => t.TokenType == MarkdownTokenType.UnorderedListItem);
        Assert.Equal('-', Assert.IsType<ListItemMetadata>(item.Metadata).Marker);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[^1].TokenType);
    }

    [Fact]
    public void TestTopLevelUnorderedMarkerWithoutContentInterruptsParagraph()
    {
        // Deviation from CommonMark: a marker without content (no space follows it) is
        // still a list marker and interrupts an open paragraph; the item has no content.
        var markdown = "B\n-";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown, text);

        // The paragraph's closing newline is flushed as a Text("\n") separator after the
        // ParagraphBlockEnd (house paragraph-boundary rule); the item itself has no content.
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("\n", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[4].TokenType);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[5].TokenType);
        Assert.Equal('-', Assert.IsType<ListItemMetadata>(tokens[5].Metadata).Marker);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[6].TokenType);
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
