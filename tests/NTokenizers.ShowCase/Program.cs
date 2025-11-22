using NTokenizers.CSharp;
using NTokenizers.Json;
using NTokenizers.Markup;
using NTokenizers.ShowCase;
using NTokenizers.Sql;
using NTokenizers.Typescript;
using NTokenizers.Xml;
using Spectre.Console;
using System.IO.Pipes;
using System.Text;

class Program
{
    static async Task Main()
    {
        var markup = MarkupExample.GetSampleText();

        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

        var writerTask = EmitSlowlyAsync(markup, pipe);

        MarkupTokenizer.Parse(reader, token =>
        {
            if (token.Metadata is HeadingMetadata meta)
            {
                meta.OnInlineToken = inlineToken =>
                {
                    var inlineValue = Markup.Escape(inlineToken.Value);
                    var inlineColored = inlineToken.TokenType switch
                    {
                        MarkupTokenType.Bold => new Markup($"[blue]{inlineValue}[/]"),
                        _ => new Markup($"[yellow]{inlineValue}[/]")
                    };
                    AnsiConsole.Write(inlineColored);
                };
            }
            else if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
            {
                AnsiConsole.WriteLine($"{csharpMeta.Language}:");
                csharpMeta.OnInlineToken = inlineToken =>
                {
                    var inlineValue = Markup.Escape(inlineToken.Value);
                    var inlineColored = inlineToken.TokenType switch
                    {
                        CSharpTokenType.Keyword => new Markup($"[turquoise2]{inlineValue}[/]"),
                        CSharpTokenType.Number => new Markup($"[blue]{inlineValue}[/]"),
                        CSharpTokenType.StringValue => new Markup($"[darkslategray1]{inlineValue}[/]"),
                        CSharpTokenType.Comment => new Markup($"[green]{inlineValue}[/]"),
                        CSharpTokenType.Identifier => new Markup($"[white]{inlineValue}[/]"),

                        CSharpTokenType.And or CSharpTokenType.Or or CSharpTokenType.Not or
                        CSharpTokenType.Equals or CSharpTokenType.NotEquals or
                        CSharpTokenType.GreaterThan or CSharpTokenType.LessThan or
                        CSharpTokenType.GreaterThanOrEqual or CSharpTokenType.LessThanOrEqual or
                        CSharpTokenType.Plus or CSharpTokenType.Minus or
                        CSharpTokenType.Multiply or CSharpTokenType.Divide or
                        CSharpTokenType.Modulo or CSharpTokenType.Operator
                            => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),

                        CSharpTokenType.OpenParenthesis or
                        CSharpTokenType.CloseParenthesis or
                        CSharpTokenType.Comma or
                        CSharpTokenType.Dot or
                        CSharpTokenType.SequenceTerminator
                            => new Markup($"[yellow]{inlineValue}[/]"),

                        CSharpTokenType.Whitespace => new Markup(inlineValue),
                        _ => new Markup($"[white]{inlineValue}[/]")
                    };  
                    AnsiConsole.Write(inlineColored);
                };
            }
            else if (token.Metadata is XmlCodeBlockMetadata xmlMeta)
            {
                AnsiConsole.WriteLine($"{xmlMeta.Language}:");
                xmlMeta.OnInlineToken = inlineToken =>
                {
                    var inlineValue = Markup.Escape(inlineToken.Value);
                    var inlineColored = inlineToken.TokenType switch
                    {
                        XmlTokenType.ElementName => new Markup($"[deepskyblue3_1]{inlineValue}[/]"),
                        XmlTokenType.Text => new Markup($"[darkslategray1]{inlineValue}[/]"),
                        XmlTokenType.Comment => new Markup($"[green]{inlineValue}[/]"),
                        XmlTokenType.ProcessingInstruction => new Markup($"[turquoise2]{inlineValue}[/]"),
                        XmlTokenType.DocumentTypeDeclaration => new Markup($"[turquoise2]{inlineValue}[/]"),
                        XmlTokenType.CData => new Markup($"[magenta1]{inlineValue}[/]"),
                        XmlTokenType.Whitespace => new Markup($"[yellow]{inlineValue}[/]"),
                        XmlTokenType.EndElement => new Markup($"[deepskyblue3_1]{inlineValue}[/]"),
                        XmlTokenType.OpeningAngleBracket => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        XmlTokenType.ClosingAngleBracket => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        XmlTokenType.AttributeName => new Markup($"[turquoise2]{inlineValue}[/]"),
                        XmlTokenType.AttributeEquals => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        XmlTokenType.AttributeValue => new Markup($"[white]{inlineValue}[/]"),
                        XmlTokenType.AttributeQuote => new Markup($"[deepskyblue3_1]{inlineValue}[/]"),
                        XmlTokenType.SelfClosingSlash => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        _ => new Markup(inlineValue)
                    };
                    AnsiConsole.Write(inlineColored);
                };
            }
            else if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
            {
                AnsiConsole.WriteLine($"TypeScript:");
                tsMeta.OnInlineToken = inlineToken =>
                {
                    var inlineValue = Markup.Escape(inlineToken.Value);
                    var inlineColored = inlineToken.TokenType switch
                    {
                        TypescriptTokenType.OpenParenthesis or TypescriptTokenType.CloseParenthesis => new Markup($"[deepskyblue4_1]{inlineValue}[/]"),
                        TypescriptTokenType.Comma => new Markup($"[yellow]{inlineValue}[/]"),
                        TypescriptTokenType.StringValue => new Markup($"[darkslategray1]{inlineValue}[/]"),
                        TypescriptTokenType.Number => new Markup($"[blue]{inlineValue}[/]"),
                        TypescriptTokenType.Keyword => new Markup($"[turquoise2]{inlineValue}[/]"),
                        TypescriptTokenType.Identifier => new Markup($"[white]{inlineValue}[/]"),
                        TypescriptTokenType.Comment => new Markup($"[green]{inlineValue}[/]"),
                        TypescriptTokenType.Operator => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        TypescriptTokenType.And or TypescriptTokenType.Or => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        TypescriptTokenType.Equals or TypescriptTokenType.NotEquals => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        TypescriptTokenType.In or TypescriptTokenType.NotIn => new Markup($"[deepskyblue4_1]{inlineValue}[/]"),
                        TypescriptTokenType.Like or TypescriptTokenType.NotLike => new Markup($"[deepskyblue4_1]{inlineValue}[/]"),
                        TypescriptTokenType.Limit => new Markup($"[deepskyblue4_1]{inlineValue}[/]"),
                        TypescriptTokenType.Match => new Markup($"[deepskyblue4_1]{inlineValue}[/]"),
                        TypescriptTokenType.SequenceTerminator => new Markup($"[yellow]{inlineValue}[/]"),
                        TypescriptTokenType.Dot => new Markup($"[yellow]{inlineValue}[/]"),
                        TypescriptTokenType.Whitespace => new Markup($"[yellow]{inlineValue}[/]"),
                        TypescriptTokenType.DateTimeValue => new Markup($"[blue]{inlineValue}[/]"),
                        TypescriptTokenType.Fingerprint or TypescriptTokenType.Message or TypescriptTokenType.StackFrame or TypescriptTokenType.ExceptionType => new Markup($"[deepskyblue3_1]{inlineValue}[/]"),
                        TypescriptTokenType.Application => new Markup($"[deepskyblue3_1]{inlineValue}[/]"),
                        TypescriptTokenType.Between => new Markup($"[deepskyblue4_1]{inlineValue}[/]"),
                        TypescriptTokenType.NotDefined => new Markup(inlineValue),
                        TypescriptTokenType.Invalid => new Markup(inlineValue),
                        _ => new Markup(inlineValue)
                    };
                    AnsiConsole.Write(inlineColored);
                };
            }
            else if (token.Metadata is JsonCodeBlockMetadata jsonMeta)
            {
                AnsiConsole.WriteLine($"{jsonMeta.Language}:");
                jsonMeta.OnInlineToken = inlineToken =>
                {
                    var inlineValue = Markup.Escape(inlineToken.Value);
                    var inlineColored = inlineToken.TokenType switch
                    {
                        JsonTokenType.StartObject => new Markup($"[deepskyblue4_1]{inlineValue}[/]"),
                        JsonTokenType.EndObject => new Markup($"[deepskyblue4_1]{inlineValue}[/]"),
                        JsonTokenType.StartArray => new Markup($"[deepskyblue4_1]{inlineValue}[/]"),
                        JsonTokenType.EndArray => new Markup($"[deepskyblue4_1]{inlineValue}[/]"),
                        JsonTokenType.PropertyName => new Markup($"[deepskyblue3_1]{inlineValue}[/]"),
                        JsonTokenType.Colon => new Markup($"[yellow]{inlineValue}[/]"),
                        JsonTokenType.Comma => new Markup($"[yellow]{inlineValue}[/]"),
                        JsonTokenType.StringValue => new Markup($"[darkslategray1]{inlineValue}[/]"),
                        JsonTokenType.Number => new Markup($"[blue]{inlineValue}[/]"),
                        JsonTokenType.True => new Markup($"[blue]{inlineValue}[/]"),
                        JsonTokenType.False => new Markup($"[blue]{inlineValue}[/]"),
                        JsonTokenType.Null => new Markup($"[blue]{inlineValue}[/]"),
                        _ => new Markup(inlineValue)
                    };
                    AnsiConsole.Write(inlineColored);
                };
            }
            else if (token.Metadata is SqlCodeBlockMetadata sqlMeta)
            {
                AnsiConsole.WriteLine($"{sqlMeta.Language}:");
                sqlMeta.OnInlineToken = inlineToken =>
                {
                    var inlineValue = Markup.Escape(inlineToken.Value);
                    var inlineColored = inlineToken.TokenType switch
                    {
                        SqlTokenType.Number => new Markup($"[deepskyblue3_1]{inlineValue}[/]"),
                        SqlTokenType.StringValue => new Markup($"[darkslategray1]{inlineValue}[/]"),
                        SqlTokenType.Comment => new Markup($"[green]{inlineValue}[/]"),
                        SqlTokenType.Keyword => new Markup($"[turquoise2]{inlineValue}[/]"),
                        SqlTokenType.Identifier => new Markup($"[white]{inlineValue}[/]"),
                        SqlTokenType.Operator => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        SqlTokenType.Comma => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        SqlTokenType.Dot => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        SqlTokenType.OpenParenthesis => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        SqlTokenType.CloseParenthesis => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        SqlTokenType.SequenceTerminator => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        SqlTokenType.NotDefined => new Markup($"[deepskyblue4_2]{inlineValue}[/]"),
                        _ => new Markup(inlineValue)
                    };
                    AnsiConsole.Write(inlineColored);
                };
            }
            else if (token.Metadata is GenericCodeBlockMetadata genericMeta)
            {
                var code = string.IsNullOrWhiteSpace(genericMeta.Language) ? "code" : genericMeta.Language;
                AnsiConsole.WriteLine($"{code}:");
                genericMeta.OnInlineToken = inlineToken =>
                {
                    WriteMarkup(inlineToken);
                };
            }
            else
            {
                WriteMarkup(token);
            }
        });

        await writerTask;
        Console.WriteLine("\nDone.");
    }

    private static void WriteMarkup(MarkupToken token)
    {
        var value = Markup.Escape(token.Value);
        var colored = token.TokenType switch
        {
            MarkupTokenType.Heading => new Markup($"[yellow]{value}[/]"),
            MarkupTokenType.Bold => new Markup($"[blue]{value}[/]"),
            MarkupTokenType.Italic => new Markup($"[green]{value}[/]"),
            MarkupTokenType.HorizontalRule => new Markup($"[grey]====================================[/]"),
            MarkupTokenType.CodeInline => new Markup($"[cyan]{value}[/]"),
            MarkupTokenType.CodeBlock => new Markup($"[magenta]{value}[/]"),
            MarkupTokenType.Link => new Markup($"[blue underline]{value}[/]"),
            MarkupTokenType.Image => new Markup($"[purple]{value}[/]"),
            MarkupTokenType.Blockquote => new Markup($"[orange1]{value}[/]"),
            MarkupTokenType.UnorderedListItem => new Markup($"[red]{value}[/]"),
            MarkupTokenType.OrderedListItem => new Markup($"[red]{value}[/]"),
            MarkupTokenType.TableCell => new Markup($"[lime]{value}[/]"),
            MarkupTokenType.Emphasis => new Markup($"[yellow]{value}[/]"),
            MarkupTokenType.TypographicReplacement => new Markup($"[grey]{value}[/]"),
            MarkupTokenType.FootnoteReference => new Markup($"[pink]{value}[/]"),
            MarkupTokenType.FootnoteDefinition => new Markup($"[pink]{value}[/]"),
            MarkupTokenType.DefinitionTerm => new Markup($"[bold]{value}[/]"),
            MarkupTokenType.DefinitionDescription => new Markup($"[italic]{value}[/]"),
            MarkupTokenType.Abbreviation => new Markup($"[underline]{value}[/]"),
            MarkupTokenType.CustomContainer => new Markup($"[teal]{value}[/]"),
            MarkupTokenType.HtmlTag => new Markup($"[orange1]{value}[/]"),
            MarkupTokenType.Subscript => new Markup($"[gray]{value}[/]"),
            MarkupTokenType.Superscript => new Markup($"[white]{value}[/]"),
            MarkupTokenType.InsertedText => new Markup($"[green]{value}[/]"),
            MarkupTokenType.MarkedText => new Markup($"[yellow]{value}[/]"),
            MarkupTokenType.Emoji => new Markup($"[yellow]{value}[/]"),
            _ => new Markup(value)
        };
        AnsiConsole.Write(colored);
    }

    static async Task EmitSlowlyAsync(string text, Stream output)
    {
        var rng = new Random();
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        foreach (var b in bytes)
        {
            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
            await output.FlushAsync();
            await Task.Delay(rng.Next(1, 5));
        }
        output.Close();
    }
}
