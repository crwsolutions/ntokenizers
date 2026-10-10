using NTokenizers.Markdown;

namespace Markdown;

public class MarkdownTokenizerBackslasTests : MarkdownTokenizerTestBase
{
    private static (List<MarkdownToken> tokens, string text) Tokenize(string markdown)
    {
        var (tokens, text, _) = MarkdownTokenizerTestBase.Tokenize(markdown);
        return (tokens, text);
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
        Assert.Equal(8, tokens.Count);
        Assert.Equal(MarkdownTokenType.ListStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.UnorderedListItem, tokens[1].TokenType);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[2].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal("*not ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[4].TokenType);
        Assert.Equal("italic* ", tokens[4].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[5].TokenType);
        Assert.Equal("item", tokens[5].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[6].TokenType);
        Assert.Equal(MarkdownTokenType.ListEnd, tokens[7].TokenType);
        Assert.Equal(markdown, text);
    }
}
