---
layout: default
title: "Markdown"
---

# Markdown Tokenizer

The Markdown Tokenizer is designed to parse Markdown code and break it down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large Markdown files or real-time markdown processing.

## Overview

The Markdown Tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing Markdown code. It can process Markdown source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

> **Especially suitable for parsing AI chat streams**, the Markdown Tokenizer excels at processing real-time tokenized data from AI models, enabling efficient handling of streaming responses and chat conversations without buffering entire responses.

<blockquote class="warning">
 <b>Warning</b><br/><br/>
 The <code>MarkdownTokenizer</code> makes heavy use of inline tokenizers for features like code fences, links, tables, emojis, footnotes, and more.<br/><br/>
 To get the full functionality, you need to handle each token’s <code>Metadata</code> and process any inline tokens it contains. If you skip handling these metadata types, <b>some characters may be eaten or disappear</b>, because the inline tokenizers strip or transform markdown symbols during parsing.
</blockquote>

## Public API

The Markdown Tokenizer inherits from `BaseTokenizer<MarkdownToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<MarkdownToken> onToken)` - Asynchronously parses a stream of Markdown code
- `Parse(Stream stream, Action<MarkdownToken> onToken)` - Synchronously parses a stream of Markdown code
- `Parse(string input)` - Parses a string of Markdown code and returns a list of tokens
- `ParseAsync(TextReader reader, Action<MarkdownToken> onToken)` - Asynchronously parses from a TextReader

### Inline tokenizers

The `MarkdownTokenizer` produces tokens that carry **metadata** describing the type of content they represent. They also contain an inline token handler. Make sure to register to it:

```csharp
await listMetadata.RegisterInlineTokenHandler(async inlineToken => { /* Handle inline tokens here */ })
```

Handling this metadata correctly is essential to render the markdown accurately. Below is a breakdown of the different metadata types, separated into **code block types** and **other markdown types**:

#### Code block metadata
- `CSharpCodeBlockMetadata`
- `XmlCodeBlockMetadata`
- `TypeScriptCodeBlockMetadata`
- `JsonCodeBlockMetadata`
- `SqlCodeBlockMetadata`
- `GenericCodeBlockMetadata`

#### Other markdown metadata
- `HeadingMetadata`
- `BlockquoteMetadata`
- `ListItemMetadata`
- `OrderedListItemMetadata`
- `TableMetadata`
- `LinkMetadata`
- `FootnoteMetadata`
- `EmojiMetadata`

##### Example: Handling Inline Tokens

```csharp
// Main MarkdownTokenizer
await MarkdownTokenizer.Create().ParseAsync(stream, onToken: async token =>
{
    // Handle inline tokens for list items, do this for all the metadata types you expect
    if (token.Metadata is ListItemMetadata listMetadata)
    {
        await listMetadata.RegisterInlineTokenHandler(async inlineToken =>
        {
            // Example: simply write the inline token value
            await ansiConsole.WriteAsync(inlineToken.Value);

        });
    }

    // You can handle other token types here...
});
```

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.Markdown;
using NTokenizers.Markdown.Metadata;
using Spectre.Console;
using System.Diagnostics;
using System.Text;

string markdownCode = """
    # Heading
    
    This is **bold** and this is *italic*.
    
    - List item 1
    - List item 2
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdownCode));
await MarkdownTokenizer.Create().ParseAsync(stream, onToken: async token =>
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
            MarkdownTokenType.Bold => new Markup($"[bold]{value}[/]"),
            MarkdownTokenType.Italic => new Markup($"[italic]{value}[/]"),
            MarkdownTokenType.Text => new Markup($"{value}"),
            _ => new Markup(value)
        };
        AnsiConsole.Write(colored);
    }
});
```

### Advanced Usage with Inline Code Blocks

Here's an example showing how to use the `MarkdownTokenizer` with a stream containing markdown text and JSON inline code blocks:

```csharp
await MarkdownTokenizer.Create().ParseAsync(reader, onToken: async token =>
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
        // Handle regular markdown tokens
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

    if (token.Metadata is InlineMarkdownMetadata)
    {
        AnsiConsole.WriteLine();
    }
});
```

### Using with TextReader

```csharp
using NTokenizers.Markdown;
using System.IO;

string markdownCode = "This is **bold** text.";
using var reader = new StringReader(markdownCode);
await MarkdownTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.Markdown;

string markdownCode = "# Title\n\nSome *text* here.";
var tokens = MarkdownTokenizer.Create().Parse(markdownCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

### Use Processed stream as string

```csharp
using NTokenizers.Markdown;
using System.Text;

string markdownCode = "This is **bold** and *italic*.";
var processedString = await MarkdownTokenizer.Create().ParseAsync(markdownCode, token =>
{
    return token.TokenType switch
    {
        MarkdownTokenType.Bold => $"<b>{token.Value}</b>",
        MarkdownTokenType.Italic => $"<i>{token.Value}</i>",
        MarkdownTokenType.Heading => $"<h1>{token.Value}</h1>",
        MarkdownTokenType.Link => $"<a>{token.Value}</a>",
        _ => token.Value
    };
});
Console.WriteLine(processedString);
```

## Token Types

The Markdown Tokenizer produces tokens of type `MarkdownTokenType` with the following token types:

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

More info: [MarkdownTokenType.cs](https://github.com/crwsolutions/ntokenizers/blob/main/src/NTokenizers/Markdown/MarkdownTokenType.cs)

## See Also

- [Json Tokenizer](/ntokenizers/json)
- [Home](/ntokenizers/)
