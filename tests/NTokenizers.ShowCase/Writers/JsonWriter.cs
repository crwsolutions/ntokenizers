using NTokenizers.Json;
using NTokenizers.Markup;
using Spectre.Console;

internal static class JsonWriter
{

    internal static void Write(JsonCodeBlockMetadata jsonMeta)
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
}