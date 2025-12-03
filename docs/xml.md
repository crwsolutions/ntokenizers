---
layout: default
title: "Xml"
---

# XML Tokenizer

The XML tokenizer is designed to parse XML code and break it down into meaningful components (tokens) for processing. It provides stream-capable functionality for handling large XML files or real-time XML data analysis.

## Overview

The XML tokenizer is part of the NTokenizers library and provides a stream-capable approach to parsing XML code. It can process XML source code in real-time, making it suitable for large files or streaming scenarios where loading everything into memory at once is impractical.

## Public API

The XML tokenizer inherits from `BaseSubTokenizer<XmlToken>` and provides the following key methods:

- `ParseAsync(Stream stream, Action<XmlToken> onToken)` - Asynchronously parses a stream of XML code
- `Parse(Stream stream, Action<XmlToken> onToken)` - Synchronously parses a stream of XML code
- `Parse(string input)` - Parses a string of XML code and returns a list of tokens
- `ParseAsync(TextReader reader, Action<XmlToken> onToken)` - Asynchronously parses from a TextReader

## Usage Examples

### Basic Usage with Stream

```csharp
using NTokenizers.Xml;
using Spectre.Console;
using System.Text;

string xmlCode = """
    <user id="4821" active="true">
        <name>Laura Smith</name>
    </user>
    """;

using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlCode));
await XmlTokenizer.Create().ParseAsync(stream, onToken: token =>
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        XmlTokenType.ElementName => new Markup($"[blue]{value}[/]"),
        XmlTokenType.EndElement => new Markup($"[blue]{value}[/]"),
        XmlTokenType.OpeningAngleBracket => new Markup($"[yellow]{value}[/]"),
        XmlTokenType.ClosingAngleBracket => new Markup($"[yellow]{value}[/]"),
        XmlTokenType.SelfClosingSlash => new Markup($"[yellow]{value}[/]"),
        XmlTokenType.AttributeName => new Markup($"[cyan]{value}[/]"),
        XmlTokenType.AttributeEquals => new Markup($"[yellow]{value}[/]"),
        XmlTokenType.AttributeQuote => new Markup($"[grey]{value}[/]"),
        XmlTokenType.AttributeValue => new Markup($"[green]{value}[/]"),
        XmlTokenType.Text => new Markup($"[white]{value}[/]"),
        XmlTokenType.Comment => new Markup($"[grey]{value}[/]"),
        XmlTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
});
```

### Using with TextReader

```csharp
using NTokenizers.Xml;
using System.IO;

string xmlCode = """
    <?xml version="1.0"?>
    <root><item id="1">Text</item></root>
    """;
using var reader = new StringReader(xmlCode);
await XmlTokenizer.Create().ParseAsync(reader, onToken: token =>
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
});
```

### Parsing String Directly

```csharp
using NTokenizers.Xml;

string xmlCode = "<note><to>User</to></note>";
var tokens = XmlTokenizer.Create().Parse(xmlCode);
foreach (var token in tokens)
{
    Console.WriteLine($"Token: {token.TokenType} = '{token.Value}'");
}
```

### Use Processed stream as string

```csharp
using NTokenizers.Xml;
using System.Text;

string xmlCode = "<user id=\"123\"><name>Laura</name></user>";
var processedString = await XmlTokenizer.Create().ParseAsync(xmlCode, token =>
{
    return token.TokenType switch
    {
        XmlTokenType.ElementName => $"[blue]{token.Value}[/]",
        XmlTokenType.AttributeName => $"[cyan]{token.Value}[/]",
        XmlTokenType.AttributeValue => $"[green]{token.Value}[/]",
        XmlTokenType.Text => $"[white]{token.Value}[/]",
        XmlTokenType.Comment => $"[grey]{token.Value}[/]",
        _ => token.Value
    };
});
Console.WriteLine(processedString);
```

## Token Types

The XML tokenizer produces tokens of type `XmlTokenType` with the following token types:

- `None` - Represents no specific kind
- `ElementName` - Represents an XML element name (e.g. `<element>`)
- `Text` - Represents text content within an XML element
- `Comment` - Represents an XML comment (including `<!-- ... -->`)
- `ProcessingInstruction` - Represents an XML processing instruction (e.g. `<?xml version="1.0"?>`)
- `DocumentTypeDeclaration` - A document type declaration (DOCTYPE)
- `CData` - Represents a CDATA section
- `Whitespace` - Represents whitespace
- `EndElement` - Represents the end of an XML element (e.g. `</element>`)
- `OpeningAngleBracket` - Represents an opening angle bracket (`<`)
- `ClosingAngleBracket` - Represents a closing angle bracket (`>`)
- `AttributeName` - Name of the attribute
- `AttributeEquals` - Represents an equals sign (`=`)
- `AttributeValue` - Represents the value of an XML attribute
- `AttributeQuote` - Represents the quote character (`"` or `'`)
- `SelfClosingSlash` - Represents a self-closing slash (`/`) in self-closing tags

More info: [XmlTokenType.cs](https://github.com/crwsolutions/ntokenizers/blob/main/src/NTokenizers/Xml/XmlTokenType.cs)

## See Also

- [Json Tokenizer](/ntokenizers/json)
- [Home](/ntokenizers/)
