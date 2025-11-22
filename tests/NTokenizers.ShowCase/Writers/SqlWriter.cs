using NTokenizers.Markup;
using NTokenizers.Sql;
using Spectre.Console;

internal static class SqlWriter
{

    internal static void Write(SqlCodeBlockMetadata sqlMeta)
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
}