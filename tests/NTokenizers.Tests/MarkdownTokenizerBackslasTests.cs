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

public class MarkdownTokenizerBackslasTests
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
    public void TestBackslashEscapeAsterisk()
    {
        var markdown = "\\*italic\\*";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, text, PEnd.
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("*italic*", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
    }

    [Fact]
    public void TestBackslashEscapeUnderscore()
    {
        var markdown = "\\_em\\_";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, text, PEnd.
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("_em_", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
    }

    [Fact]
    public void TestBackslashEscapeBacktick()
    {
        var markdown = "\\`code\\`";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, text, PEnd.
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("`code`", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
    }

    [Fact]
    public void TestBackslashEscapeBrackets()
    {
        var markdown = "\\[not a link\\]";
        var (tokens, text) = Tokenize(markdown);
        // Spaces trigger text emission, so we get multiple Text tokens; wrapped in a paragraph.
        Assert.Equal(5, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("[not ", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("a ", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("link]", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[4].TokenType);
    }

    [Fact]
    public void TestBackslashEscapeDoubleBackslash()
    {
        var markdown = "\\\\";
        var (tokens, text) = Tokenize(markdown);
        // A line that starts with an escaped character is still a paragraph.
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("\\", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
    }

    [Fact]
    public void TestBackslashNonPunctuation()
    {
        var markdown = "\\a";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, text, PEnd.
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("\\a", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
    }

    [Fact]
    public void TestBackslashAtEndOfInput()
    {
        var markdown = "text\\";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, text, PEnd.
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("text\\", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
    }

    [Fact]
    public void TestBackslashEscapeAllPunctuation()
    {
        // All 32 CommonMark ASCII punctuation characters, each backslash-escaped
        var markdown = "\\!\\\"\\#\\$\\%\\&\\'\\(\\)\\*\\+\\,\\-\\.\\/\\:\\;\\<\\=\\>\\?\\@\\[\\\\\\]\\^\\_\\`\\{\\|\\}\\~";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
    }

    [Fact]
    public void TestBackslashEscapeInHeading()
    {
        var markdown = "# \\*not italic\\* heading";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.Heading, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("*not italic* heading", tokens[1].Value);
    }

    [Fact]
    public void TestBackslashEscapeInBlockquote()
    {
        var markdown = "> \\*not italic\\* quote";
        var (tokens, text) = Tokenize(markdown);
        // The blockquote streams its content as a paragraph; backslash escapes resolve to
        // literal characters. Expected: Blockquote, PStart, Text '*not ', Text 'italic* ',
        // Text 'quote', PEnd.
        Assert.Equal(6, tokens.Count);
        Assert.Equal(MarkdownTokenType.Blockquote, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[1].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("*not ", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("italic* ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("quote", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[5].TokenType);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestBackslashEscapeInListItem()
    {
        var markdown = "- \\*not italic\\* item";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[2].TokenType);
        Assert.Equal("*not italic* item", tokens[2].Value);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[3].TokenType);
    }
}
