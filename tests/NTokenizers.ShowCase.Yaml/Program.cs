using NTokenizers.Yaml;
using Spectre.Console;
using System.Text;

string yaml = """
        %YAML 1.2
        %TAG !yaml! tag:yaml.org,2002:

        --- # Document Start (optional for the first one)
        name: Alice
        age: 30
        --- # Second Document Start
        name: Bob
        age: 25
        ... # Document End (optional for the last one)

        # Block Mapping
        person: &p
          name: "Alice"
          age: 30
          hobbies:
            - reading
            - coding
            - chess

        # Block Sequence
        fruits:
          - apple
          - banana
          - cherry

        # Flow Sequence
        numbers: [1, 2, 3, 4]

        # Flow Mapping
        employee: { name: "Bob", position: "Developer" }

        # Key / Value explicit
        ? question_key
        : answer_value

        # Anchors & Aliases
        manager: *p

        # Tag examples
        !customTag "some value"
        !!str "string as type"
        """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));
await YamlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        YamlTokenType.Directive => new Markup($"[cyan]{value}[/]"),
        YamlTokenType.DirectiveKey => new Markup($"[DeepSkyBlue3_1]{value}[/]"),
        YamlTokenType.DirectiveValue => new Markup($"[white]{value}[/]"),
        YamlTokenType.DocumentStart => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.DocumentEnd => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.Comment => new Markup($"[green]{value}[/]"),
        YamlTokenType.Key => new Markup($"[DeepSkyBlue3_1]{value}[/]"),
        YamlTokenType.Colon => new Markup($"[cyan]{value}[/]"),
        YamlTokenType.Value => new Markup($"[DeepSkyBlue4_1]{value}[/]"),
        YamlTokenType.Quote => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.String => new Markup($"[DarkSlateGray1]{value}[/]"),
        YamlTokenType.Anchor => new Markup($"[orange1]{value}[/]"),
        YamlTokenType.Alias => new Markup($"[grey]{value}[/]"),
        YamlTokenType.Tag => new Markup($"[DeepSkyBlue3_1]{value}[/]"),
        YamlTokenType.FlowSeqStart => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.FlowSeqEnd => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.FlowMapStart => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.FlowMapEnd => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.FlowEntry => new Markup($"[yellow]{value}[/]"),
        YamlTokenType.BlockSeqEntry => new Markup($"[cyan]{value}[/]"),
        YamlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});

Console.WriteLine();
Console.WriteLine("Done.");
