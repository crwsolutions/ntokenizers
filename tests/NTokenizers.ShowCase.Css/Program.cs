using NTokenizers.Css;
using Spectre.Console;
using System.Text;

string css = """
        /* Complex CSS with strings, urls, and mixed quotes */
        @media screen and (max-width: 768px) and (orientation: "portrait") { /* AtRule, StartRuleSet */

            :root { /* Selector, StartRuleSet */
                --font-name: "Open Sans"; /* CustomProperty, Colon, String, Semicolon */
                --icon-path: url('/assets/icons/ui/icon\'s-set.svg'); /* Function, String, EscapedQuote */
                --label-text: 'Click "Here"'; /* String with double quotes inside single quotes */
            } /* EndRuleSet */

            .header::before { /* Selector, PseudoElement, StartRuleSet */
                content: "Welcome to \"My Site\""; /* String, EscapedDoubleQuote */
                font-family: var(--font-name), 'Segoe UI', sans-serif; /* Function, String, Identifier */
                background-image: url("https://example.com/images/bg(\"dark\").png"); /* URL, EscapedChars */
                padding: calc(1rem + 2px); /* Function, Operator */
            } /* EndRuleSet */

            .button[data-action='save']::after { /* AttributeSelector, String */
                content: 'Saved at ' attr(data-time); /* String, Function */
                display: block; /* Identifier */
                background: linear-gradient(
                    45deg,
                    rgba(0, 0, 0, 0.5),
                    rgba(255, 255, 255, 0.2)
                ); /* Function, Numbers */
            } /* EndRuleSet */

            .icon {
                mask-image: url( "/icons/mask.svg#icon-\"user\"" ); /* URL, Mixed Quotes */
                width: 32px; /* Number, Unit */
                height: 32px; /* Number, Unit */
            }

            a[href^="https://"]::after { /* AttributeSelector, String */
                content: " ↗"; /* Unicode symbol in string */
                font-size: 0.85em; /* Number, Unit */
            }

            /* Multiline escaped string example */
            .tooltip::before {
                content: "Line one\
        Line two"; /* Escaped newline */
                white-space: pre; /* Identifier */
            }

        } /* EndRuleSet */
        
        """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(css));
await CssTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        CssTokenType.StartRuleSet => new Markup($"[yellow]{value}[/]"),
        CssTokenType.EndRuleSet => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Selector => new Markup($"[cyan]{value}[/]"),
        CssTokenType.PropertyName => new Markup($"[blue]{value}[/]"),
        CssTokenType.StringValue => new Markup($"[white]{value}[/]"),
        CssTokenType.Quote => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Number => new Markup($"[magenta]{value}[/]"),
        CssTokenType.Unit => new Markup($"[orange1]{value}[/]"),
        CssTokenType.Function => new Markup($"[yellow]{value}[/]"),
        CssTokenType.OpenParen => new Markup($"[yellow]{value}[/]"),
        CssTokenType.CloseParen => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Comment => new Markup($"[green]{value}[/]"),
        CssTokenType.Colon => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Semicolon => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Comma => new Markup($"[yellow]{value}[/]"),
        CssTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        CssTokenType.AtRule => new Markup($"[orange1]{value}[/]"),
        CssTokenType.Identifier => new Markup($"[blue]{value}[/]"),
        CssTokenType.Operator => new Markup($"[red]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});

Console.WriteLine();
Console.WriteLine("Done.");