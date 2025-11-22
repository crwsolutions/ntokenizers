using NTokenizers.Markup;
using NTokenizers.Typescript;
using Spectre.Console;

internal static class TypescriptWriter
{

    internal static void Write(TypeScriptCodeBlockMetadata tsMeta)
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
}