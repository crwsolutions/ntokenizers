using NTokenizers.Markdown;
using System.Text;

namespace Markdown;

/// <summary>
/// Tokenizer tests for CommonMark section 4.4 indented code blocks (CommonMark 0.31.2,
/// examples 107-118).
///
/// Streaming note: code content is emitted line by line as Text tokens as soon as a line is
/// known to belong to the block. Blank lines inside the block are held until the next line is
/// known, so they are merged into the Text token of the following code line.
/// </summary>
public class MarkdownTokenizerIndentedCodeBlockTests
{
    private static (List<MarkdownToken> tokens, string text) Tokenize(string markdown)
    {
        var tokens = new List<MarkdownToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        var result = MarkdownTokenizer.Create().ParseAsync(stream, tokens.Add).GetAwaiter().GetResult();
        return (tokens, result);
    }

    // Asserts the full (TokenType, Value) sequence of the token stream.
    private static void AssertTokens(List<MarkdownToken> tokens, params (MarkdownTokenType type, string value)[] expected)
    {
        Assert.Equal(expected.Length, tokens.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].type, tokens[i].TokenType);
            Assert.Equal(expected[i].value, tokens[i].Value);
        }
    }

    // The full text of the token stream (concatenated token values).
    private static string TokenText(List<MarkdownToken> tokens)
    {
        var sb = new StringBuilder();
        foreach (var token in tokens)
        {
            sb.Append(token.Value);
        }
        return sb.ToString();
    }

    // Example 107: a simple indented code block, one Text token per content line. A line
    // ending is only included when the input line has one (the final line does not), so the
    // token stream stays faithful to the input. The first four columns of indentation are
    // stripped from the code content.
    [Fact]
    public void TestSimpleIndentedCodeBlock()
    {
        var markdown = "    a simple\n      indented code block";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "a simple\n"),
            (MarkdownTokenType.Text, "  indented code block"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        Assert.Equal("a simple\n  indented code block", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // Example 110: blank lines and lines that look like other constructs stay code.
    [Fact]
    public void TestBlankLinesAndConstructLikeLinesStayCode()
    {
        var markdown = "    <a/>\n    *hi*\n\n    - one";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "<a/>\n"),
            (MarkdownTokenType.Text, "*hi*\n"),
            (MarkdownTokenType.Text, "\n- one"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        Assert.Equal("<a/>\n*hi*\n\n- one", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // Example 111: blank lines between indented lines are kept (indented blanks keep their
    // indentation after the first four columns are removed).
    [Fact]
    public void TestBlankLinesBetweenIndentedLines()
    {
        var markdown = "    chunk1\n\n    chunk2\n  \n \n \n    chunk3";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "chunk1\n"),
            (MarkdownTokenType.Text, "\nchunk2\n"),
            (MarkdownTokenType.Text, "\n\n\nchunk3"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        Assert.Equal("chunk1\n\nchunk2\n\n\n\nchunk3", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // Example 112: an indented blank line (four or more spaces) keeps its extra indentation.
    [Fact]
    public void TestIndentedBlankLineKeepsExtraIndentation()
    {
        var markdown = "    chunk1\n      \n      chunk2";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "chunk1\n"),
            (MarkdownTokenType.Text, "  \n  chunk2"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        Assert.Equal("chunk1\n  \n  chunk2", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // Example 113: an indented line cannot interrupt a paragraph. (The paragraph continuation
    // line keeps its leading spaces and the soft break is a separate Text token, matching the
    // existing paragraph behavior.)
    [Fact]
    public void TestIndentedCodeCannotInterruptParagraph()
    {
        var markdown = "Foo\n    bar\n";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.ParagraphBlockStart, string.Empty),
            (MarkdownTokenType.Text, "Foo"),
            (MarkdownTokenType.Text, "\n"),
            (MarkdownTokenType.Text, "    bar"),
            (MarkdownTokenType.ParagraphBlockEnd, string.Empty));
        // The paragraph keeps its own whitespace/soft-break behavior; only the code block
        // content has its indentation removed (here there is none, the line stays in the paragraph).
        Assert.Equal("Foo\n    bar", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // Example 114: a non-indented line ends the block and is processed as a new paragraph.
    [Fact]
    public void TestNonIndentedLineEndsBlock()
    {
        var markdown = "    foo\nbar";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "foo\n"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty),
            (MarkdownTokenType.ParagraphBlockStart, string.Empty),
            (MarkdownTokenType.Text, "bar"),
            (MarkdownTokenType.ParagraphBlockEnd, string.Empty));
        Assert.Equal("foo\nbar", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // Example 115: line-start constructs after the block are still parsed normally.
    [Fact]
    public void TestBlockBeforeHeadingAndHorizontalRule()
    {
        var markdown = "# Heading\n    foo\nHeading\n------\n    foo\n----";
        var (tokens, text) = Tokenize(markdown);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Heading);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.IndentedCodeBlockStart && t.Value == string.Empty);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.Text && t.Value == "foo\n");
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.IndentedCodeBlockEnd && t.Value == string.Empty);
        Assert.Contains(tokens, t => t.TokenType == MarkdownTokenType.HorizontalRule && t.Value == "------");
        Assert.Equal(markdown, text);
    }

    // Example 116: only the first four columns of indentation are removed.
    [Fact]
    public void TestExtraIndentationIsPreserved()
    {
        var markdown = "        foo\n    bar";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "    foo\n"),
            (MarkdownTokenType.Text, "bar"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        Assert.Equal("    foo\nbar", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // Example 117: leading and trailing blank lines are not part of the block. (The blank
    // lines before the block are emitted as plain whitespace Text tokens, matching the
    // existing behavior for leading blank lines; the writer drops them.)
    [Fact]
    public void TestTrailingBlankLinesAreDropped()
    {
        var markdown = "\n    \n    foo\n    \n";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.Text, "\n"),
            (MarkdownTokenType.Text, "    \n"),
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "foo\n"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        // The leading blank lines are emitted as plain text tokens, but the trailing blank
        // line after the block is dropped, so the token text lacks the final line ending.
        Assert.Equal("\n    \nfoo\n", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // Example 118: a block may end at end of stream; trailing spaces of the last line are kept
    // and no line ending is introduced (the input line has none).
    [Fact]
    public void TestBlockAtEndOfStream()
    {
        var markdown = "    foo  ";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "foo  "),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        Assert.Equal("foo  ", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // Tabs count as spaces to the next 4-column tab stop (Tabs section, examples 1 and 8).
    [Fact]
    public void TestTabsInIndentation()
    {
        var markdown = "\tfoo\n\tbar";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "foo\n"),
            (MarkdownTokenType.Text, "bar"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        Assert.Equal("foo\nbar", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // A blank line followed by a non-indented line ends the block; the blank line is dropped.
    [Fact]
    public void TestBlankLineFollowedByTextEndsBlock()
    {
        var markdown = "    foo\n\nbar";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "foo\n"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty),
            (MarkdownTokenType.ParagraphBlockStart, string.Empty),
            (MarkdownTokenType.Text, "bar"),
            (MarkdownTokenType.ParagraphBlockEnd, string.Empty));
        // The blank line between the block and the paragraph is not part of the block, so it
        // is not in the token text; the paragraph content is.
        Assert.Equal("foo\nbar", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // A blank line at the end of the stream ends the block; the blank line is dropped.
    [Fact]
    public void TestBlankLineAtEndOfStreamEndsBlock()
    {
        var markdown = "    foo\n\n";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "foo\n"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        // The trailing blank line is not part of the block, so only the code content is in the token text.
        Assert.Equal("foo\n", TokenText(tokens));
        Assert.Equal(markdown, text);
    }

    // Two separate code blocks with a paragraph in between.
    [Fact]
    public void TestTwoCodeBlocksWithParagraphBetween()
    {
        var markdown = "    indented code\n\nparagraph\n\n    more code";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "indented code\n"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty),
            (MarkdownTokenType.ParagraphBlockStart, string.Empty),
            (MarkdownTokenType.Text, "paragraph"),
            (MarkdownTokenType.ParagraphBlockEnd, string.Empty),
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "more code"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty));
        Assert.Equal(markdown, text);
    }

    // CRLF line endings produce the same tokens as LF.
    [Fact]
    public void TestCrlfLineEndings()
    {
        var markdown = "    foo\r\nbar\r\n";
        var (tokens, text) = Tokenize(markdown);
        AssertTokens(tokens,
            (MarkdownTokenType.IndentedCodeBlockStart, string.Empty),
            (MarkdownTokenType.Text, "foo\n"),
            (MarkdownTokenType.IndentedCodeBlockEnd, string.Empty),
            (MarkdownTokenType.ParagraphBlockStart, string.Empty),
            (MarkdownTokenType.Text, "bar"),
            (MarkdownTokenType.ParagraphBlockEnd, string.Empty));
        Assert.Equal("foo\nbar", TokenText(tokens));
        Assert.Equal(markdown, text);
    }
}
