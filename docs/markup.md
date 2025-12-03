---
layout: default
title: "Markup"
---

# Markup Tokenizer

The Markup tokenizer is designed to parse Markdown/Markup code and break it down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large Markup files or real-time markup processing.

## Overview

The Markup tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing Markdown/Markup code. It can process Markup source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

> **Especially suitable for parsing AI chat streams**, the Markup tokenizer excels at processing real-time tokenized data from AI models, enabling efficient handling of streaming responses and chat conversations without buffering entire responses.

## Public API

The Markup tokenizer inherits from `BaseTokenizer<MarkupToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<MarkupToken> onToken)` - Asynchronously parses a stream of Markup code
- `Parse(Stream stream, Action<MarkupToken> onToken)` - Synchronously parses a stream of Markup code
- `Parse(string input)` - Parses a string of Markup code and returns a list of tokens
- `ParseAsync(TextReader reader, Action<MarkupToken> onToken)` - Asynchronously parses from a TextReader

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.Markup;
using NTokenizers.Markup.Metadata;
using Spectre.Console;
using System.Diagnostics;
using System.Text;

string markupCode = """
    # Heading
    
    This is **bold** and this is *italic*.
    
    - List item 1
    - List item 2
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markupCode));
await MarkupTokenizer.Create().ParseAsync(stream, onToken: async token =>
{
    if (token.Metadata is ListItemMetadata listMetadata)
    {
        AnsiConsole.Write(new Markup($"[bold lime]{listMetadata.Marker} [/]"));
        await listMetadata.RegisterInlineTokenHandler(inlineToken =>
        {
            var value = Markup.Escape(inlineToken.Value);
            AnsiConsole.Write(new Markup($"[bold red]{value}[/]"));
        });
        AnsiConsole.WriteLine();
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
    else
    {
        var value = Markup.Escape(token.Value);
        var colored = token.TokenType switch
        {
            MarkupTokenType.Bold => new Markup($"[bold]{value}[/]"),
            MarkupTokenType.Italic => new Markup($"[italic]{value}[/]"),
            MarkupTokenType.Text => new Markup($"{value}"),
            _ => new Markup(value)
        };
        AnsiConsole.Write(colored);
    }
});
```

### Advanced Usage with Inline Code Blocks

Here's an example showing how to use the `MarkupTokenizer` with a stream containing markup text and JSON inline code blocks:

```csharp
await MarkupTokenizer.Create().ParseAsync(reader, onToken: async token =>
{
    //Handle json code fence
    if (token.Metadata is JsonCodeBlockMetadata jsonMetadata)
    {
        await jsonMetadata.RegisterInlineTokenHandler( inlineToken =>
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
    else
    {
        // Handle regular markup tokens
        var value = Markup.Escape(token.Value);
        var colored = token.TokenType switch
        {
            MarkupTokenType.Text => new Markup($"{value}"),
            MarkupTokenType.Bold => new Markup($"[bold]{value}[/]"),
            MarkupTokenType.Italic => new Markup($"[italic]{value}[/]"),
            _ => new Markup(value)
        };

        AnsiConsole.Write(colored);
    }

    if (token.Metadata is InlineMarkupMetadata)
    {
        AnsiConsole.WriteLine();
    }
});
```

### Using with TextReader

```csharp
using NTokenizers.Markup;
using System.IO;

string markupCode = "This is **bold** text.";
using var reader = new StringReader(markupCode);
await MarkupTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.Markup;

string markupCode = "# Title\n\nSome *text* here.";
var tokens = MarkupTokenizer.Create().Parse(markupCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

### Use Processed stream as string

```csharp
using NTokenizers.Markup;
using System.Text;

string markupCode = "This is **bold** and *italic*.";
var processedString = await MarkupTokenizer.Create().ParseAsync(markupCode, token =>
{
    return token.TokenType switch
    {
        MarkupTokenType.Bold => $"<b>{token.Value}</b>",
        MarkupTokenType.Italic => $"<i>{token.Value}</i>",
        MarkupTokenType.Heading => $"<h1>{token.Value}</h1>",
        MarkupTokenType.Link => $"<a>{token.Value}</a>",
        _ => token.Value
    };
});
Console.WriteLine(processedString);
```

## Token Types

The Markup tokenizer produces tokens of type `MarkupTokenType` with the following token types:

- `Text` - Represents plain text content
- `Bold` - Represents bold text (value contains text without `**` markers)
- `Italic` - Represents italic text (value contains text without `*` markers)
- `Heading` - Represents a heading (value contains text without `#` markers, level in Metadata)
- `HorizontalRule` - Represents a horizontal rule (`---` or `***`)
- `TypographicReplacement` - Represents a typographic replacement (`(c)`, `(r)`, `(tm)`, `+-`)
- `Emphasis` - Represents a generic emphasis marker
- `Blockquote` - Represents a blockquote (value contains text without `>` marker)
- `UnorderedListItem` - Represents an unordered list item (value contains text without `+`, `-`, `*` markers)
- `OrderedListItem` - Represents an ordered list item (value contains text without number prefix)
- `CodeInline` - Represents inline code (value contains code without `` ` `` markers)
- `CodeBlock` - Represents a code block (value contains code without ``` markers, language in Metadata)
- `Table` - Represents a table (value is empty, structure in Metadata)
- `TableRow` - Represents a table row (value is empty, position in Metadata)
- `TableCell` - Represents a table cell (value contains cell content without `|` delimiters)
- `TableAlignments` - Represents table column alignments (value is empty, alignments in Metadata)
- `Link` - Represents a link (value contains link text without `[ ]` markers, URL in Metadata)
- `Image` - Represents an image (value contains alt text without `![ ]` markers, URL in Metadata)
- `Emoji` - Represents an emoji (value contains emoji name without `:` markers)
- `Subscript` - Represents subscript text (value contains content without `^` markers)
- `Superscript` - Represents superscript text (value contains content without `~` markers)
- `InsertedText` - Represents inserted text (value contains content without `++` markers)
- `MarkedText` - Represents marked text (value contains content without `==` markers)
- `FootnoteReference` - Represents a footnote reference (value contains reference ID without `[^ ]` markers)
- `FootnoteDefinition` - Represents a footnote definition (value contains definition content)
- `DefinitionTerm` - Represents a definition term
- `DefinitionDescription` - Represents a definition description (value contains description without `:` marker)
- `Abbreviation` - Represents an abbreviation (value contains definition)
- `CustomContainer` - Represents a custom container (value contains container type/name without `:::` markers)
- `HtmlTag` - Represents an HTML tag (value contains complete tag including `< >` markers)

More info: [MarkupTokenType.cs](https://github.com/crwsolutions/ntokenizers/blob/main/src/NTokenizers/Markup/MarkupTokenType.cs)

## See Also

- [Json Tokenizer](/ntokenizers/json)
- [Home](/ntokenizers/)
