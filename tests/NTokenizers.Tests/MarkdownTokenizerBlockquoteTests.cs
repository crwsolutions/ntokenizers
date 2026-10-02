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
using NTokenizers.Python;
using NTokenizers.Rust;
using NTokenizers.Sql;
using NTokenizers.Swift;
using NTokenizers.Toml;
using NTokenizers.Typescript;
using NTokenizers.Xml;
using System.Text;

namespace Markdown;

/// <summary>
/// Blockquote decision-table tests. A blockquote streams its content as a full markdown
/// sub-document through the <see cref="BlockquoteMetadata"/> inline token handler: at the
/// start of every input line the tokenizer offers the line to the blockquote decision table
/// (strip the <c>&gt;</c> prefix, or end the quote when the line no longer fits the grammar)
/// and the remainder is parsed as regular markdown. Nested blockquotes are represented as
/// nested <see cref="MarkdownTokenType.Blockquote"/> tokens.
/// </summary>
public class MarkdownTokenizerBlockquoteTests
{
    private static (List<MarkdownToken> tokens, string text) Tokenize(string markdown)
    {
        var tokens = new List<MarkdownToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));

        // Recursively registers inline token handlers for every token. A token that carries
        // markdown inline metadata (heading, blockquote, list item, indented code block,
        // table) can itself contain markdown tokens - most importantly, a blockquote can nest
        // another blockquote - so those handlers re-dispatch recursively to capture the whole
        // token tree. Language code-block metadata streams non-markdown tokens; a no-op
        // handler at every level lets the parser stream and discard that content (and avoids
        // the parser waiting on an unhandled inline token when a code fence appears inside a
        // quoted sub-document).
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
            else if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
            {
                csharpMeta.RegisterInlineTokenHandler(token => { });
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
                jsonMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is XmlCodeBlockMetadata xmlMeta)
            {
                xmlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is HtmlCodeBlockMetadata htmlMeta)
            {
                htmlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is CssCodeBlockMetadata cssMeta)
            {
                cssMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is SqlCodeBlockMetadata sqlMeta)
            {
                sqlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
            {
                tsMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is TomlCodeBlockMetadata tomlMeta)
            {
                tomlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is PythonCodeBlockMetadata pythonMeta)
            {
                pythonMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is GenericCodeBlockMetadata gMeta)
            {
                gMeta.RegisterInlineTokenHandler(token => { });
            }
        }

        var result = MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            tokens.Add(token);
            RegisterInlineHandlers(token, tokens);
        }).GetAwaiter().GetResult();
        return (tokens, result);
    }

    private static void AssertToken(MarkdownToken token, MarkdownTokenType type, string? value = null)
    {
        Assert.Equal(type, token.TokenType);
        if (value is not null)
        {
            Assert.Equal(value, token.Value);
        }
    }

    // Decision table, row 1 (prefix): the trigger line's content is a sub-document line start.

    [Fact]
    public void TestSingleLineBlockquoteIsParagraph()
    {
        var markdown = "> quoted text";
        var (tokens, text) = Tokenize(markdown);
        // Blockquote wrapping a single-line paragraph.
        Assert.Equal(5, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "quoted ");
        AssertToken(tokens[3], MarkdownTokenType.Text, "text");
        AssertToken(tokens[4], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 1 (prefix): the prefix is stripped on every quoted line; soft
    // line breaks inside the paragraph are preserved.

    [Fact]
    public void TestMultiLineBlockquoteSingleParagraph()
    {
        var markdown = "> line1\n> line2";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "line1");
        AssertToken(tokens[3], MarkdownTokenType.Text, "\n"); // soft line break
        AssertToken(tokens[4], MarkdownTokenType.Text, "line2");
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 1 (indented prefix): a blockquote marker may be indented by up to
    // three leading spaces (CommonMark). Those lines belong to ONE blockquote, not to nested
    // quotes - the leading spaces plus the '>' are absorbed as this level's prefix, and the
    // remainder continues the open paragraph. Regression: these lines used to open three
    // nested blockquotes.

    [Fact]
    public void TestIndentedPrefixIsSingleQuote()
    {
        var markdown = " > a\n > b\n > c";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(9, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Text, " "); // trigger line's leading space
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[3], MarkdownTokenType.Text, "a");
        AssertToken(tokens[4], MarkdownTokenType.Text, "\n"); // soft line break
        AssertToken(tokens[5], MarkdownTokenType.Text, "b");
        AssertToken(tokens[6], MarkdownTokenType.Text, "\n"); // soft line break
        AssertToken(tokens[7], MarkdownTokenType.Text, "c");
        AssertToken(tokens[8], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 1 (indented prefix): up to three leading spaces before the '>' are
    // part of the blockquote marker even when the content is a heading, so all three lines of
    // CommonMark spec example 229 belong to a single blockquote (one quote, one heading, one
    // paragraph).

    [Fact]
    public void TestIndentedPrefixHeadingContent()
    {
        var markdown = "   > # Foo\n   > bar\n > baz";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(9, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Text, "   "); // trigger line's leading spaces
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.Heading);
        AssertToken(tokens[3], MarkdownTokenType.Text, "Foo");
        AssertToken(tokens[4], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[5], MarkdownTokenType.Text, "bar");
        AssertToken(tokens[6], MarkdownTokenType.Text, "\n"); // soft line break
        AssertToken(tokens[7], MarkdownTokenType.Text, "baz");
        AssertToken(tokens[8], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 1 boundary: FOUR leading columns before the '>' is NOT a blockquote
    // prefix (it is indented code territory). With a paragraph open, such a line is a lazy
    // continuation and stays as paragraph text (the '>' is not a marker, so no nested quote);
    // a following properly-prefixed line continues the same paragraph.

    [Fact]
    public void TestFourColumnsIsNotAQuotePrefix()
    {
        var markdown = "> a\n    > b\n> c";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(9, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "a");
        AssertToken(tokens[3], MarkdownTokenType.Text, "\n"); // soft line break
        AssertToken(tokens[4], MarkdownTokenType.Text, "    > "); // lazy continuation, not a prefix
        AssertToken(tokens[5], MarkdownTokenType.Text, "b");
        AssertToken(tokens[6], MarkdownTokenType.Text, "\n"); // soft line break
        AssertToken(tokens[7], MarkdownTokenType.Text, "c");
        AssertToken(tokens[8], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 1 (prefix): a blank quoted line (prefix only) is a paragraph
    // separator inside the quote; the quote stays open.

    [Fact]
    public void TestBlankQuotedLineSeparatesParagraphs()
    {
        var markdown = "> line1\n>\n> line2";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(7, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "line1");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[4], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[5], MarkdownTokenType.Text, "line2");
        AssertToken(tokens[6], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 1 (prefix) at end of stream: a bare '>' is an empty blockquote.

    [Fact]
    public void TestBareBlockquoteMarkerIsEmptyBlockquote()
    {
        var markdown = ">";
        var (tokens, text) = Tokenize(markdown);
        Assert.Single(tokens);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 2 (blank line, no prefix): closes the quote; the line is emitted
    // as a faithful Text token and the next quoted line opens a new blockquote.

    [Fact]
    public void TestBlankLineClosesQuoteAndNextLineReopens()
    {
        var markdown = "> foo\n\n> bar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(9, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[4], MarkdownTokenType.Text, "\n"); // the blank line, faithful
        AssertToken(tokens[5], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[6], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[7], MarkdownTokenType.Text, "bar");
        AssertToken(tokens[8], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 2 (blank line, no prefix): closes the quote; the following plain
    // line belongs to the outer scope.

    [Fact]
    public void TestBlankLineClosesQuoteAndPlainLineIsTopLevel()
    {
        var markdown = "> foo\n\nbar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(8, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[4], MarkdownTokenType.Text, "\n"); // the blank line, faithful
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[6], MarkdownTokenType.Text, "bar");
        AssertToken(tokens[7], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 5 (other line, paragraph open): a plain line lazily continues the
    // quoted paragraph; no new blockquote is started.

    [Fact]
    public void TestLazyContinuationKeepsLineInsideQuote()
    {
        var markdown = "> foo\nbar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[3], MarkdownTokenType.Text, "\n"); // soft line break
        AssertToken(tokens[4], MarkdownTokenType.Text, "bar");
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 4 (line-start construct): a list marker line ends the quote and
    // the line is left untouched for the outer scope.

    [Fact]
    public void TestListMarkerBreaksQuote()
    {
        var markdown = "> foo\n- bar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(10, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[4], MarkdownTokenType.ListStart);
        AssertToken(tokens[5], MarkdownTokenType.UnorderedListItem);
        AssertToken(tokens[6], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[7], MarkdownTokenType.Text, "bar");
        AssertToken(tokens[8], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[9], MarkdownTokenType.ListEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 4 (line-start construct): a heading line ends the quote.

    [Fact]
    public void TestHeadingBreaksQuote()
    {
        var markdown = "> foo\n# bar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[4], MarkdownTokenType.Heading);
        AssertToken(tokens[5], MarkdownTokenType.Text, "bar");
        Assert.Equal(markdown, text);
    }

    // Decision table, row 4 (line-start construct): a code fence ends the quote.

    [Fact]
    public void TestCodeFenceBreaksQuote()
    {
        var markdown = "> foo\n```";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(5, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[4], MarkdownTokenType.CodeBlock);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 4 (line-start construct): a thematic break ends the quote.

    [Fact]
    public void TestThematicBreakBreaksQuote()
    {
        var markdown = "> foo\n---";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(5, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[4], MarkdownTokenType.HorizontalRule, "---");
        Assert.Equal(markdown, text);
    }

    // Decision table, row 3 (indented line, paragraph open): a four-space-indented line is
    // a lazy continuation; the indentation is preserved in the paragraph text.

    [Fact]
    public void TestIndentedLineIsLazyContinuationWhenParagraphOpen()
    {
        var markdown = "> foo\n    bar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[3], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[4], MarkdownTokenType.Text, "    bar"); // indentation preserved
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Decision table, row 3 (indented line, no paragraph open): ends the quote so the outer
    // scope can start an indented code block.

    [Fact]
    public void TestIndentedLineBreaksQuoteWhenNoParagraphOpen()
    {
        var markdown = ">\n    bar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[2], MarkdownTokenType.IndentedCodeBlock);
        AssertToken(tokens[3], MarkdownTokenType.Text, "bar");
        Assert.Equal(markdown, text);
    }

    // Decision table, row 5 (other line, no paragraph open): ends the quote; the line
    // belongs to the outer scope.

    [Fact]
    public void TestPlainLineWithoutOpenParagraphEndsQuote()
    {
        var markdown = "> \nbar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(5, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[2], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[3], MarkdownTokenType.Text, "bar");
        AssertToken(tokens[4], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Nested blockquotes: the remainder after the outer prefix goes through the normal
    // line-start grammar, so a following '>' opens a nested blockquote. Opening the nested
    // blockquote interrupts the outer paragraph, so 'a' is a sibling paragraph that is
    // closed before the nested quote begins.
    [Fact]
    public void TestNestedBlockquote()
    {
        var markdown = "> a\n> > b\n> c";
        var (tokens, text) = Tokenize(markdown);
        // Outer: closed paragraph 'a', then a nested blockquote holding the paragraph
        // 'b' and its lazy continuation 'c'.
        Assert.Equal(10, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "a");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[4], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[6], MarkdownTokenType.Text, "b");
        AssertToken(tokens[7], MarkdownTokenType.Text, "\n"); // soft line break
        AssertToken(tokens[8], MarkdownTokenType.Text, "c"); // lazy continuation
        AssertToken(tokens[9], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // A nested blockquote that ends on a blank line must not leave the outer level's
    // per-line prefix flag stale. The nested level consumes the blank line (and its line
    // ending) itself, so the outer level never processes a '\n' after the nested construct
    // returns; the flag is reset by the line-start construct path instead. Without the reset
    // the outer decision table is skipped for the next line, and a plain line such as
    // 'Hallo' is swallowed as a paragraph inside the outer blockquote instead of ending it.
    // Regression: 'Hallo' used to render inside the blockquote.

    [Fact]
    public void TestNestedBlockquoteEndingOnBlankLineClosesOuterQuote()
    {
        var markdown = "> > # Hoi\n\nHallo";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(8, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.Heading);
        AssertToken(tokens[3], MarkdownTokenType.Text, "Hoi");
        AssertToken(tokens[4], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[6], MarkdownTokenType.Text, "Hallo");
        AssertToken(tokens[7], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Same stale-flag scenario with a nested paragraph: the blank line closes the nested
    // blockquote, and 'Hallo' starts a top-level paragraph (outside both blockquotes).

    [Fact]
    public void TestNestedParagraphEndingOnBlankLineClosesOuterQuote()
    {
        var markdown = "> > foo\n\nHallo";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(9, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[3], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[4], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[5], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[6], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[7], MarkdownTokenType.Text, "Hallo");
        AssertToken(tokens[8], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Same stale-flag scenario with a nested list: the blank line closes the nested
    // blockquote, and 'Hallo' starts a top-level paragraph (outside both blockquotes).

    [Fact]
    public void TestNestedListEndingOnBlankLineClosesOuterQuote()
    {
        var markdown = "> > - x\n\nHallo";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(10, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.ListStart);
        AssertToken(tokens[3], MarkdownTokenType.UnorderedListItem);
        AssertToken(tokens[4], MarkdownTokenType.Text, "x");
        AssertToken(tokens[5], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[6], MarkdownTokenType.ListEnd);
        AssertToken(tokens[7], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[8], MarkdownTokenType.Text, "Hallo");
        AssertToken(tokens[9], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Three marker columns on one line open three nested blockquotes (the per-line prefix
    // flag limits each level to one '>' per input line).

    [Fact]
    public void TestThreeMarkerColumnsOpenThreeNestedQuotes()
    {
        var markdown = "> > > x";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[4], MarkdownTokenType.Text, "x");
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestThreeConsecutiveMarkersOpenThreeNestedQuotes()
    {
        var markdown = ">>> x";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[4], MarkdownTokenType.Text, "x");
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // A blockquote can hold a heading and a paragraph (sub-document grammar).

    [Fact]
    public void TestHeadingAndParagraphInQuote()
    {
        var markdown = "> # Foo\n> bar\n> baz";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(8, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Heading);
        AssertToken(tokens[2], MarkdownTokenType.Text, "Foo");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[4], MarkdownTokenType.Text, "bar");
        AssertToken(tokens[5], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[6], MarkdownTokenType.Text, "baz");
        AssertToken(tokens[7], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // A blockquote can hold a list (sub-document grammar).

    [Fact]
    public void TestListInQuote()
    {
        var markdown = "> - foo\n> - bar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Blockquote);
        Assert.Equal(2, tokens.Count(t => t.TokenType == MarkdownTokenType.UnorderedListItem));
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "foo");
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "bar");
        Assert.Equal(markdown, text);
    }

    // A code fence inside a quote streams its content through the fence's language metadata.
    // Fence content reads the raw stream, so the quote prefixes on content lines are not
    // stripped (documented simplification); the CodeBlock token carries the fence's language
    // metadata.
    [Fact]
    public void TestCodeFenceInQuoteCarriesLanguageMetadata()
    {
        var markdown = ">\n> ```python\n> def hallo():\n> ```";
        var (tokens, text) = Tokenize(markdown);
        var codeBlock = tokens.SingleOrDefault(t => t.TokenType == MarkdownTokenType.CodeBlock);
        Assert.NotNull(codeBlock);
        var metadata = Assert.IsType<PythonCodeBlockMetadata>(codeBlock!.Metadata);
        Assert.Equal("python", metadata.Language);
    }

    // CRLF input: newlines after the prefix strip are faithful soft breaks.

    [Fact]
    public void TestCrlfMultiLineBlockquote()
    {
        var markdown = "> foo\r\n> bar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[3], MarkdownTokenType.Text, "\n"); // soft line break
        AssertToken(tokens[4], MarkdownTokenType.Text, "bar");
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // A lazy continuation line that ends the quote is left untouched in the stream: the
    // concatenated token values equal the input (nothing was eaten by the parser).
    [Fact]
    public void TestLazyContinuationIsFaithfulToInput()
    {
        var markdown = "> foo\nbar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(markdown, text);
        Assert.Single(tokens.Where(t => t.TokenType == MarkdownTokenType.Blockquote));
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "bar");
    }

    // Depth-aware marker assignment: a continuation line that carries fewer markers than the
    // inner level requires but a valid lazy continuation remainder keeps the paragraph open
    // at the innermost level; the markers are decoration, not a new nested quote.

    [Fact]
    public void TestNestedLazyContinuationStaysInInnerQuote()
    {
        var markdown = "> > Dit is cool\n> Hoi";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(9, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[3], MarkdownTokenType.Text, "Dit ");
        AssertToken(tokens[4], MarkdownTokenType.Text, "is ");
        AssertToken(tokens[5], MarkdownTokenType.Text, "cool");
        AssertToken(tokens[6], MarkdownTokenType.Text, "\n"); // soft line break
        AssertToken(tokens[7], MarkdownTokenType.Text, "Hoi"); // lazy, stays in inner quote
        AssertToken(tokens[8], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // A blank quoted line (prefix only) that carries fewer markers than the inner level
    // closes the innermost paragraph; the next quoted line continues the OUTER level, not the
    // inner one. This is the fix for the nested-quote bug where 'Hoi' used to stay nested.

    [Fact]
    public void TestNestedBlankQuotedLineClosesInnerParagraph()
    {
        var markdown = "> > Dit is cool\n> \n> Hoi";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(11, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[3], MarkdownTokenType.Text, "Dit ");
        AssertToken(tokens[4], MarkdownTokenType.Text, "is ");
        AssertToken(tokens[5], MarkdownTokenType.Text, "cool");
        AssertToken(tokens[6], MarkdownTokenType.ParagraphBlockEnd); // inner paragraph closed
        AssertToken(tokens[7], MarkdownTokenType.Text, "\n"); // the blank quoted line
        AssertToken(tokens[8], MarkdownTokenType.ParagraphBlockStart); // outer paragraph
        AssertToken(tokens[9], MarkdownTokenType.Text, "Hoi"); // outer level, not nested
        AssertToken(tokens[10], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Same as the previous test but the blank quoted line has no trailing space.

    [Fact]
    public void TestNestedBlankQuotedLineNoTrailingSpaceClosesInnerParagraph()
    {
        var markdown = "> > Dit is cool\n>\n> Hoi";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(11, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[3], MarkdownTokenType.Text, "Dit ");
        AssertToken(tokens[4], MarkdownTokenType.Text, "is ");
        AssertToken(tokens[5], MarkdownTokenType.Text, "cool");
        AssertToken(tokens[6], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[7], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[8], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[9], MarkdownTokenType.Text, "Hoi");
        AssertToken(tokens[10], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Spec 249: a continuation line of a triple-nested quote may omit every '>' marker;
    // 'bar' lazily continues the innermost paragraph.

    [Fact]
    public void TestDeeplyNestedLazyContinuation()
    {
        var markdown = "> > > foo\nbar";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(8, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[4], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[5], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[6], MarkdownTokenType.Text, "bar"); // lazy at innermost level
        AssertToken(tokens[7], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // Spec 250: three consecutive markers open three blockquotes, and later lines with fewer
    // markers lazily continue the innermost paragraph. The depth must not grow past three.

    [Fact]
    public void TestThreeLevelsStayOpenWithLazyContinuations()
    {
        var markdown = ">>> foo\n> bar\n>>baz";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(10, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[2], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[4], MarkdownTokenType.Text, "foo");
        AssertToken(tokens[5], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[6], MarkdownTokenType.Text, "bar"); // lazy, one marker
        AssertToken(tokens[7], MarkdownTokenType.Text, "\n");
        AssertToken(tokens[8], MarkdownTokenType.Text, "baz"); // lazy, two markers
        AssertToken(tokens[9], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // A continuation line with more markers than the current depth: only this level's markers
    // are consumed; the remainder (starting with a '>') opens a deeper nested quote. Guards
    // the depth-aware prefix strip against over-consuming the marker chain.

    [Fact]
    public void TestContinuationWithMoreMarkersOpensDeeperQuote()
    {
        var markdown = "> a\n> > b";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(8, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "a");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[4], MarkdownTokenType.Blockquote, string.Empty); // deeper quote
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[6], MarkdownTokenType.Text, "b");
        AssertToken(tokens[7], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }

    // A continuation line with more markers than the current depth opens a deeper quote even
    // when the extra marker is consecutive (no space between them).

    [Fact]
    public void TestContinuationWithConsecutiveExtraMarkerOpensDeeperQuote()
    {
        var markdown = "> a\n>> b";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(8, tokens.Count);
        AssertToken(tokens[0], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[1], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[2], MarkdownTokenType.Text, "a");
        AssertToken(tokens[3], MarkdownTokenType.ParagraphBlockEnd);
        AssertToken(tokens[4], MarkdownTokenType.Blockquote, string.Empty);
        AssertToken(tokens[5], MarkdownTokenType.ParagraphBlockStart);
        AssertToken(tokens[6], MarkdownTokenType.Text, "b");
        AssertToken(tokens[7], MarkdownTokenType.ParagraphBlockEnd);
        Assert.Equal(markdown, text);
    }
}
