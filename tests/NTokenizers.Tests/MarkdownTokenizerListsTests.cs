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
using System.Text;

namespace Markdown;

public class MarkdownTokenizerListsTests
{
    private static (List<MarkdownToken> tokens, string text) Tokenize(string markdown)
    {
        var tokens = new List<MarkdownToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        var result = MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            tokens.Add(token);

            // Automatically set OnInlineToken to capture inline tokens
            // Note: We just register the handler without waiting - the processing happens during parsing
            if (token.Metadata is HeadingMetadata headingMeta)
            {
                headingMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is BlockquoteMetadata blockquoteMeta)
            {
                blockquoteMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is ListItemMetadata listMeta)
            {
                listMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is OrderedListItemMetadata orderedListMeta)
            {
                orderedListMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
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
                // For XML, XAML, and SVG code blocks, we receive XmlToken objects
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
            else if (token.Metadata is TableMetadata tableMeta)
            {
                tableMeta.RegisterInlineTokenHandler(tokens.Add);
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
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithPlus()
    {
        var markdown = "+ item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithAsterisk()
    {
        var markdown = "* item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Unordered list items have empty value
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item 1", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestUnorderedListWithLeadingSpaces()
    {
        // Up to three leading spaces are part of the list item; four or more make the
        // line an indented code block (CommonMark 4.4), so this is code, not a list item.
        var markdown = "   * item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal("   ", tokens[0].Value); // Indentation is in the Value
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item 1", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListWithLeadingSpaces()
    {
        var markdown = "  1. item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[0].TokenType);
        Assert.Equal("  ", tokens[0].Value); // Indentation is in the Value
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<OrderedListItemMetadata>(tokens[0].Metadata);
        Assert.Equal(1, ((OrderedListItemMetadata)tokens[0].Metadata!).Number);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item 1", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestNestedUnorderedListItems()
    {
        // Nesting keeps the markers within three columns of indentation; a fourth column
        // would start an indented code block (CommonMark 4.4), not a deeper list item.
        var markdown = "- top\n  - nested\n   - deeper";
        var (tokens, text) = Tokenize(markdown);

        // top-level: no indentation
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);

        // nested: 2 spaces indentation
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[2].TokenType);
        Assert.Equal("  ", tokens[2].Value);

        // deeper: 3 spaces indentation
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[4].TokenType);
        Assert.Equal("   ", tokens[4].Value);

        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedList()
    {
        var markdown = "1. item 1";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count); // OrderedListItem token + inline text token
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.NotNull(tokens[0].Metadata);
        Assert.IsType<OrderedListItemMetadata>(tokens[0].Metadata);
        Assert.Equal(1, ((OrderedListItemMetadata)tokens[0].Metadata!).Number);

        // Inline content
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item 1", tokens[1].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestOrderedListMultipleDigits()
    {
        var markdown = "42. item";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count); // OrderedListItem token + inline text token
        Assert.Equal(MarkdownTokenType.OrderedListItem, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value); // Value is empty when OnInlineToken is used
        Assert.Equal(42, ((OrderedListItemMetadata)tokens[0].Metadata!).Number);

        // Inline content
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("item", tokens[1].Value);
        Assert.Equal(markdown, text);
    }
}
