---
title: Encoding Support
---

# Encoding in Tokenizers

Encoding plays a crucial role in how tokenizers process and interpret text data. When working with tokenizers, especially those that handle streams or files, understanding the encoding ensures proper text handling and prevents data corruption or misinterpretation.

## Understanding Encoding in Tokenizers

In the context of tokenizers, encoding determines how characters are represented in memory and how they are read from input streams. The package provides methods for parsing streams with specific encodings, ensuring that text data is correctly interpreted regardless of its original format. 

## Key Methods for Encoding Handling

The `BaseTokenizer` class offers several methods for handling different encodings:

```csharp
public async Task<string> ParseAsync(Stream stream, Encoding encoding, Action<TToken> onToken)
public string Parse(Stream stream, Encoding encoding, Action<TToken> onToken)
```

## Default Encoding Behavior

When no explicit encoding is specified, tokenizers typically default to UTF-8 encoding, which is the most common and widely supported encoding for text data. However, when dealing with legacy systems or specific file formats, it's important to explicitly specify the correct encoding to avoid character corruption.

## Practical Example

```csharp
// Using default UTF-8 encoding
var tokens = tokenizer.Parse(inputStream, token => { /* handle token */ });

// Explicitly specifying encoding
var tokens = tokenizer.Parse(inputStream, Encoding.UTF8, token => { /* handle token */ });
```

## Troubleshooting

If you encounter encoding issues:

1. Verify the source encoding of your input data
2. Check that your system locale supports the encoding
3. Consider using `Encoding.Default` for system-specific encoding
4. Use `Encoding.GetEncoding()` for more specific encoding definitions

For more information on .NET encoding, see [Microsoft's Encoding documentation](https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding).