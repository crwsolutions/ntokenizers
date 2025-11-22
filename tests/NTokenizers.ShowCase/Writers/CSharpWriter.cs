using NTokenizers.CSharp;
using NTokenizers.Markup;
using Spectre.Console;

internal static class CSharpWriter
{
    internal static void Write(CSharpCodeBlockMetadata csharpMeta)
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
}