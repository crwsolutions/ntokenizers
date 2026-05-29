using NTokenizers.C;
using NTokenizers.Cpp;
using NTokenizers.Core;
using NTokenizers.Css;
using NTokenizers.Go;
using NTokenizers.Html;
using NTokenizers.Java;
using NTokenizers.Json;
using NTokenizers.Kotlin;
using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;
using NTokenizers.Rust;
using NTokenizers.Swift;
using NTokenizers.Toml;
using NTokenizers.Typescript;
using NTokenizers.Xml;
using Spectre.Console;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

class Program
{
    static async Task Main()
    {
        string markdown = """
        - aaa
        - bbb

        Here is some **bold** text and some *italic* text.

        Here is some larger text: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.

        # NTokenizers Showcase

        ## Css example
        ```css
        .user {
            color: #FFFFFF;
            active: true;
        }
        ```

        ## XML example
        ```xml
        <user id="4821" active="true">
            <name>Laura Smith</name>
        </user>
        ```

        ## HTML example
        ```html
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; background-color: #f0f8ff; }
                .header { color: #4682b4; text-align: center; }
                .content { margin: 20px; padding: 15px; background-color: white; border-radius: 5px; }
            </style>
        </head>
        <body>
            <p>Hello world!</p>
            <script>
                console.log("Hello from the sample script!");
                document.addEventListener('DOMContentLoaded', function() {
                    console.log("DOM is fully loaded");
                });
            </script>
        </body>
        </html>
        ```

        ## JSON example
        ```json
        {
            "name": "Laura Smith",
            "active": true
        }
        ```

        ## TypeScript example
        ```typescript
        const user = {
            name: "Laura Smith",
            active: true
        };
        ```

        ## TOML example
        ```toml
        title = "My App"
        active = true
        count = 42
        ```

        ## C example
        ```c
        int main(void) {
            printf("Hello, World!\n");
            return 0;
        }
        ```

        ## C++ example
        ```cpp
        std::vector<int> numbers = {1, 2, 3};
        for (const auto& n : numbers)
            std::cout << n << std::endl;
        ```

        ## Rust example
        ```rust
        let numbers = vec![1, 2, 3];
        for n in &numbers {
            println!("{}", n);
        }
        ```

        ## Kotlin example
        ```kotlin
        val numbers = listOf(1, 2, 3)
        numbers.forEach { println(it) }
        ```

        ## Go example
        ```go
        numbers := []int{1, 2, 3}
        for _, n := range numbers {
            fmt.Println(n)
        }
        ```

        ## Swift example
        ```swift
        let numbers = [1, 2, 3]
        for n in numbers {
            print(n)
        }
        ```

        ## Java example
        ```java
        List<Integer> numbers = List.of(1, 2, 3);
        numbers.forEach(System.out::println);
        ```
        """;

        // Create connected streams
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

        // Start slow writer
        var writerTask = EmitSlowlyAsync(markdown, pipe);

        // Parse markup
        await MarkdownTokenizer.Create().ParseAsync(reader, onToken: async token =>
        {
            if (token.Metadata is ICodeBlockMetadata codeBlock)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Markup($"[bold lime]{codeBlock.Language}:[/]"));
                AnsiConsole.WriteLine();
            }

            if (token.Metadata is ListItemMetadata listMetadata)
            {
                AnsiConsole.Write(new Markup($"[bold lime]{listMetadata.Marker} [/]"));
                await listMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    AnsiConsole.Write(new Markup($"[bold red]{value}[/]"));
                });
                Debug.WriteLine("Written listItem inlines");

            }
            else if (token.Metadata is HeadingMetadata headingMetadata)
            {
                await headingMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = headingMetadata.Level != 1 ?
                        new Markup($"[bold GreenYellow]{value}[/]") :
                        new Markup($"[bold yellow]** {value} **[/]");
                    AnsiConsole.Write(colored);
                });
                Debug.WriteLine("Written Heading inlines");
            }
            else if (token.Metadata is XmlCodeBlockMetadata xmlMetadata)
            {
                await xmlMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        XmlTokenType.ElementName => new Markup($"[blue]{value}[/]"),
                        XmlTokenType.OpeningAngleBracket => new Markup($"[yellow]{value}[/]"),
                        XmlTokenType.ClosingAngleBracket => new Markup($"[yellow]{value}[/]"),
                        XmlTokenType.SelfClosingSlash => new Markup($"[yellow]{value}[/]"),
                        XmlTokenType.AttributeName => new Markup($"[cyan]{value}[/]"),
                        XmlTokenType.AttributeEquals => new Markup($"[yellow]{value}[/]"),
                        XmlTokenType.AttributeQuote => new Markup($"[grey]{value}[/]"),
                        XmlTokenType.AttributeValue => new Markup($"[green]{value}[/]"),
                        XmlTokenType.Text => new Markup($"[white]{value}[/]"),
                        XmlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is JsonCodeBlockMetadata jsonMetadata)
            {
                await jsonMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        JsonTokenType.StartObject => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.EndObject => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.StartArray => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.EndArray => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.PropertyName => new Markup($"[cyan]{value}[/]"),
                        JsonTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        JsonTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        JsonTokenType.True => new Markup($"[orange1]{value}[/]"),
                        JsonTokenType.False => new Markup($"[orange1]{value}[/]"),
                        JsonTokenType.Null => new Markup($"[grey]{value}[/]"),
                        JsonTokenType.Colon => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.Comma => new Markup($"[yellow]{value}[/]"),
                        JsonTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is HtmlCodeBlockMetadata htmlMetadata)
            {
                await htmlMetadata.RegisterInlineTokenHandler(async inlineToken =>
                {
                    if (inlineToken.Metadata is TypeScriptCodeBlockMetadata tsMeta)
                    {
                        await HandleScript(tsMeta);
                    }
                    else if (inlineToken.Metadata is CssCodeBlockMetadata cssMeta)
                    {
                        await HandleCss(cssMeta);
                    }
                    else
                    {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        HtmlTokenType.OpeningAngleBracket => new Markup($"[yellow]{value}[/]"),
                        HtmlTokenType.ClosingAngleBracket => new Markup($"[yellow]{value}[/]"),
                        HtmlTokenType.SelfClosingSlash => new Markup($"[yellow]{value}[/]"),
                        HtmlTokenType.AttributeName => new Markup($"[cyan]{value}[/]"),
                        HtmlTokenType.AttributeEquals => new Markup($"[yellow]{value}[/]"),
                        HtmlTokenType.AttributeQuote => new Markup($"[grey]{value}[/]"),
                        HtmlTokenType.AttributeValue => new Markup($"[green]{value}[/]"),
                        HtmlTokenType.Text => new Markup($"[white]{value}[/]"),
                        HtmlTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        HtmlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                    }
                });
            }
            else if (token.Metadata is TypeScriptCodeBlockMetadata tsMetadata)
            {
                await HandleScript(tsMetadata);
            }
            else if (token.Metadata is CssCodeBlockMetadata cssMetadata)
            {
                await HandleCss(cssMetadata);
            }
            else if (token.Metadata is TomlCodeBlockMetadata tomlMetadata)
            {
                await tomlMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        TomlTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
                        TomlTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        TomlTokenType.StringQuote => new Markup($"[green]{value}[/]"),
                        TomlTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        TomlTokenType.Boolean => new Markup($"[orange1]{value}[/]"),
                        TomlTokenType.DateTime => new Markup($"[blue]{value}[/]"),
                        TomlTokenType.Comment => new Markup($"[dim]{value}[/]"),
                        TomlTokenType.OpenBracket or TomlTokenType.CloseBracket => new Markup($"[yellow]{value}[/]"),
                        TomlTokenType.OpenBrace or TomlTokenType.CloseBrace => new Markup($"[yellow]{value}[/]"),
                        TomlTokenType.Equal => new Markup($"[grey]{value}[/]"),
                        TomlTokenType.Comma => new Markup($"[grey]{value}[/]"),
                        TomlTokenType.Dot => new Markup($"[grey]{value}[/]"),
                        TomlTokenType.Whitespace => new Markup($"[dim]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is CCodeBlockMetadata cMetadata)
            {
                await cMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        CTokenType.Keyword => new Markup($"[blue]{value}[/]"),
                        CTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
                        CTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        CTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        CTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                        CTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        CTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is CppCodeBlockMetadata cppMetadata)
            {
                await cppMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        CppTokenType.Keyword => new Markup($"[blue]{value}[/]"),
                        CppTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
                        CppTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        CppTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        CppTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                        CppTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        CppTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is RustCodeBlockMetadata rustMetadata)
            {
                await rustMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        RustTokenType.Keyword => new Markup($"[blue]{value}[/]"),
                        RustTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
                        RustTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        RustTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        RustTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                        RustTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        RustTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is KotlinCodeBlockMetadata kotlinMetadata)
            {
                await kotlinMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        KotlinTokenType.Keyword => new Markup($"[blue]{value}[/]"),
                        KotlinTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
                        KotlinTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        KotlinTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        KotlinTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                        KotlinTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        KotlinTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is GoCodeBlockMetadata goMetadata)
            {
                await goMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        GoTokenType.Keyword => new Markup($"[blue]{value}[/]"),
                        GoTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
                        GoTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        GoTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        GoTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                        GoTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        GoTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is SwiftCodeBlockMetadata swiftMetadata)
            {
                await swiftMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        SwiftTokenType.Keyword => new Markup($"[blue]{value}[/]"),
                        SwiftTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
                        SwiftTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        SwiftTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        SwiftTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                        SwiftTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        SwiftTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else if (token.Metadata is JavaCodeBlockMetadata javaMetadata)
            {
                await javaMetadata.RegisterInlineTokenHandler(inlineToken =>
                {
                    var value = Markup.Escape(inlineToken.Value);
                    var colored = inlineToken.TokenType switch
                    {
                        JavaTokenType.Keyword => new Markup($"[blue]{value}[/]"),
                        JavaTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
                        JavaTokenType.StringValue => new Markup($"[green]{value}[/]"),
                        JavaTokenType.Number => new Markup($"[magenta]{value}[/]"),
                        JavaTokenType.Operator => new Markup($"[yellow]{value}[/]"),
                        JavaTokenType.Comment => new Markup($"[grey]{value}[/]"),
                        JavaTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
                        _ => new Markup(value)
                    };
                    AnsiConsole.Write(colored);
                });
            }
            else
            {
                // Handle regular markup tokens
                var value = Markup.Escape(token.Value);
                var colored = token.TokenType switch
                {
                    MarkdownTokenType.Text => new Markup($"{value}"),
                    MarkdownTokenType.Bold => new Markup($"[bold]{value}[/]"),
                    MarkdownTokenType.Italic => new Markup($"[italic]{value}[/]"),
                    _ => new Markup(value)
                };

                AnsiConsole.Write(colored);
            }

            if (token.Metadata is InlineMetadata)
            {
                AnsiConsole.WriteLine();
            }
        });

        await writerTask;

        Console.WriteLine();
        Console.WriteLine("Done.");
    }

    private static async Task HandleScript(TypeScriptCodeBlockMetadata tsMetadata)
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

    private static async Task HandleCss(CssCodeBlockMetadata cssMetadata)
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

    static async Task EmitSlowlyAsync(string markdown, Stream output)
    {
        var rng = new Random();
        byte[] bytes = Encoding.UTF8.GetBytes(markdown);

        foreach (var b in bytes)
        {
            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
            await output.FlushAsync();
            await Task.Delay(rng.Next(0, 2));
        }

        output.Close(); // EOF
    }
}