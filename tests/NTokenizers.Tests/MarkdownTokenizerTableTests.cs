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

public class MarkdownTokenizerTableTests
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
