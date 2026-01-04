using NTokenizers.Html;
using Spectre.Console;
using System.Text;

string html = """
    <!DOCTYPE html>
    <html>
    <head>
        <style>
            body { font-family: Arial, sans-serif; }
            .container { max-width: 600px; margin: 0 auto; }
            button { padding: 10px 15px; background: #007bff; color: white; }
        </style>
    </head>
    <body>
        <div class="container">
            <h1>Hello World</h1>
            <p>This is a sample HTML page.</p>
            <button onclick="alert('Clicked!')">Click Me</button>
        </div>
        <script>
            document.addEventListener('DOMContentLoaded', () => {
                console.log('Page loaded');
            });
        </script>
    </body>
    </html>
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(html));
await HtmlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        HtmlTokenType.ElementName => new Markup($"[blue]{value}[/]"),
        HtmlTokenType.OpeningAngleBracket => new Markup($"[yellow]{value}[/]"),
        HtmlTokenType.ClosingAngleBracket => new Markup($"[yellow]{value}[/]"),
        HtmlTokenType.SelfClosingSlash => new Markup($"[yellow]{value}[/]"),
        HtmlTokenType.AttributeName => new Markup($"[cyan]{value}[/]"),
        HtmlTokenType.AttributeEquals => new Markup($"[yellow]{value}[/]"),
        HtmlTokenType.AttributeQuote => new Markup($"[grey]{value}[/]"),
        HtmlTokenType.AttributeValue => new Markup($"[green]{value}[/]"),
        HtmlTokenType.Text => new Markup($"[white]{value}[/]"),
        HtmlTokenType.Comment => new Markup($"[grey]{value}[/]"),
        HtmlTokenType.DocumentTypeDeclaration => new Markup($"[magenta]{value}[/]"),
        HtmlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});

Console.WriteLine();
Console.WriteLine("Done.");
