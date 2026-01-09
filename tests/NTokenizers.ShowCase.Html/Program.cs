using NTokenizers.Css;
using NTokenizers.Html;
using NTokenizers.Typescript;
using Spectre.Console;
using System.IO.Pipes;
using System.Text;

class Program
{
    static async Task Main()
    {
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

        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

        // Start slow writer
        var writerTask = EmitSlowlyAsync(html, pipe);

        await HtmlTokenizer.Create().ParseAsync(reader, onToken: async token =>
        {
            if (token.Metadata is TypeScriptCodeBlockMetadata tsMetadata)
            {
                await tsMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        TypescriptTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
                        TypescriptTokenType.Keyword => new Markup($"[blue]{value}[/]"),
                        TypescriptTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        TypescriptTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        TypescriptTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                        TypescriptTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        TypescriptTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is CssCodeBlockMetadata cssMetadata)
            {
                await cssMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        CssTokenType.Identifier => new Markup($"[white]{value}[/]"),
                        CssTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        CssTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                        CssTokenType.Selector => new Markup($"[yellow]{value}[/]"),
                        CssTokenType.Comment => new Markup($"[green]{value}[/]"),
                        CssTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else
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
            }
        });

        Console.WriteLine();
        Console.WriteLine("Done.");
    }

    static async Task EmitSlowlyAsync(string markdown, Stream output)
    {
        var rng = new Random();
        byte[] bytes = Encoding.UTF8.GetBytes(markdown);

        foreach (var b in bytes)
        {
            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
            await output.FlushAsync();
            await Task.Delay(rng.Next(2, 8));
        }

        output.Close(); // EOF
    }
}
