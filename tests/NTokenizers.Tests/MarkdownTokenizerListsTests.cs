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
        // P1: a single item is wrapped in a balanced ListStart / ListEnd group.
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.IsType<ListMetadata>(tokens[0].Metadata);
        Assert.False(((ListMetadata)tokens[0].Metadata).IsOrdered);

        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // Unordered list items have empty value
        Assert.Equal('-', ((ListItemMetadata)tokens[1].Metadata).Marker);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("item 1", tokens[2].Value);

        Assert.Equal(MarkdownTokenType.ListEnd, tokens[3].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithPlus()
    {
        var markdown = "+ item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal('+', ((ListItemMetadata)tokens[1].Metadata).Marker);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("item 1", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[3].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithAsterisk()
    {
        var markdown = "* item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // Unordered list items have empty value
        Assert.Equal('*', ((ListItemMetadata)tokens[1].Metadata).Marker);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("item 1", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[3].TokenType);
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
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("   ", tokens[1].Value); // Leading indentation as whitespace text
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal('*', ((ListItemMetadata)tokens[2].Metadata).Marker);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("item 1", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[4].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListWithLeadingSpaces()
    {
        var markdown = "  1. item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.IsType<ListMetadata>(tokens[0].Metadata);
        Assert.True(((ListMetadata)tokens[0].Metadata).IsOrdered);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("  ", tokens[1].Value); // Leading indentation as whitespace text
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.NotNull(tokens[2].Metadata);
        Assert.IsType<OrderedListItemMetadata>(tokens[2].Metadata);
        Assert.Equal(1, ((OrderedListItemMetadata)tokens[2].Metadata).Number);
        Assert.Equal('.', ((OrderedListItemMetadata)tokens[2].Metadata).Marker);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("item 1", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[4].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestFlatListGrouping()
    {
        // P1: consecutive unordered items with indent under four columns are grouped in a
        // single balanced ListStart / ListEnd run (one <ul> in HTML), not one list per item.
        // True nesting (a nested ListStart inside an item) arrives in P2. The leading
        // whitespace of the indented siblings is emitted as Text on the list level.
        var markdown = "- top\n  - nested\n   - deeper";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(10, tokens.Count);
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
        Assert.Equal(markdown, text);
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
        Assert.Equal(new[] { '-', '*', '+' }, items.Select(i => ((ListItemMetadata)i.Metadata).Marker));

        // No whitespace text between tightly packed items (the line separators are consumed).
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListGrouping()
    {
        var markdown = "1. a\n2. b";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.True(((ListMetadata)tokens[0].Metadata).IsOrdered);

        var items = tokens.Where(t => t.TokenType == MarkdownTokenType.OrderedListItem).ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal(1, ((OrderedListItemMetadata)items[0].Metadata).Number);
        Assert.Equal(2, ((OrderedListItemMetadata)items[1].Metadata).Number);

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
        Assert.All(items, i => Assert.Equal(')', ((OrderedListItemMetadata)i.Metadata).Marker));
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
        Assert.False(((ListMetadata)listStarts[0].Metadata).IsOrdered);
        Assert.True(((ListMetadata)listStarts[1].Metadata).IsOrdered);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestBlankLineBetweenItemsIsPreserved()
    {
        // A blank line between list items is preserved as a whitespace Text token on the
        // parent (list) level — console output keeps the newline; HTML may ignore it.
        var markdown = "- a\n\n- b";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[^1].TokenType);

        var whitespace = tokens.Where(t => t.TokenType == MarkdownTokenType.Text && t.Value == "\n").ToList();
        Assert.Single(whitespace);

        Assert.Equal(markdown, text);
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
        Assert.Equal(4, tokens.Count); // ListStart + OrderedListItem + inline text + ListEnd
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // Value is empty when OnInlineToken is used
        Assert.NotNull(tokens[1].Metadata);
        Assert.IsType<OrderedListItemMetadata>(tokens[1].Metadata);
        Assert.Equal(1, ((OrderedListItemMetadata)tokens[1].Metadata).Number);

        // Inline content
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("item 1", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[3].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListMultipleDigits()
    {
        var markdown = "42. item";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count); // ListStart + OrderedListItem + inline text + ListEnd
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(42, ((OrderedListItemMetadata)tokens[1].Metadata).Number);

        // Inline content
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("item", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[3].TokenType);
        Assert.Equal(markdown, text);
    }
}
