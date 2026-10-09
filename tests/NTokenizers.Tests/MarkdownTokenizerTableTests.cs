using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;

namespace Markdown;

public class MarkdownTokenizerTableTests : MarkdownTokenizerTestBase
{
    private static (List<MarkdownToken> tokens, string text) Tokenize(string markdown)
    {
        var (tokens, text, _) = MarkdownTokenizerTestBase.Tokenize(markdown);
        return (tokens, text);
    }

    [Fact]
    public void TestTableCell()
    {
        var markdown = "| cell 1 |";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" cell 1 ", tokens[3].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableCellBold()
    {
        var markdown = "|**bold**|";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(4, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Bold, tokens[3].TokenType);
        Assert.Equal("bold", tokens[3].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableWithTwoCells()
    {
        var markdown = "| | |";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(6, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value); // Table cells have empty value
        Assert.Equal(MarkdownTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value); // Table cells have empty value
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value); // Table cells have empty value
        Assert.Equal(MarkdownTokenType.Text, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableWithTwoRows()
    {
        var markdown = "| |\r\n| |";
        var (tokens, text) = Tokenize(markdown);
        Assert.Equal(7, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[5].TokenType);
        Assert.Equal(string.Empty, tokens[5].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[6].TokenType);
        Assert.Equal(" ", tokens[6].Value);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableAlignments()
    {
        var markdown = "|----|----|---|";
        var (tokens, text) = Tokenize(markdown);
        var tableMetadata = tokens[0].Metadata as TableMetadata;
        Assert.Equal(2, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableAlignments, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(3, tableMetadata!.Alignments!.Count);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![0]);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![1]);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![2]);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableWithTwoCellsAndAlignment()
    {
        var markdown = "| | | |\r\n|:---|:---:|---:|\r\n";
        var (tokens, text) = Tokenize(markdown);
        var tableMetadata = tokens[0].Metadata as TableMetadata;
        Assert.Equal(9, tokens.Count);
        Assert.Equal(MarkdownTokenType.Table, tokens[0].TokenType);
        Assert.Equal(string.Empty, tokens[0].Value);
        Assert.Equal(MarkdownTokenType.TableRow, tokens[1].TokenType);
        Assert.Equal(string.Empty, tokens[1].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[2].TokenType);
        Assert.Equal(string.Empty, tokens[2].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[4].TokenType);
        Assert.Equal(string.Empty, tokens[4].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(MarkdownTokenType.TableCell, tokens[6].TokenType);
        Assert.Equal(string.Empty, tokens[6].Value);
        Assert.Equal(MarkdownTokenType.Text, tokens[7].TokenType);
        Assert.Equal(" ", tokens[7].Value);
        Assert.Equal(MarkdownTokenType.TableAlignments, tokens[8].TokenType);
        Assert.Equal(string.Empty, tokens[8].Value);
        Assert.Equal(Justify.Left, tableMetadata!.Alignments![0]);
        Assert.Equal(Justify.Center, tableMetadata!.Alignments![1]);
        Assert.Equal(Justify.Right, tableMetadata!.Alignments![2]);
        Assert.Equal(markdown, text);
    }

    [Fact]
    public void TestTableInvalidCells()
    {
        var markdown = "|||";
        var (tokens, text) = Tokenize(markdown);
        // A plain line is a paragraph: PStart, text, PEnd.
        Assert.Equal(3, tokens.Count);
        Assert.Equal(MarkdownTokenType.ParagraphBlockStart, tokens[0].TokenType);
        Assert.Equal(MarkdownTokenType.Text, tokens[1].TokenType);
        Assert.Equal("|||", tokens[1].Value);
        Assert.Equal(MarkdownTokenType.ParagraphBlockEnd, tokens[2].TokenType);
        Assert.Equal(markdown, text);
    }
}
