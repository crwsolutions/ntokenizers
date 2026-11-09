namespace Poc.Core;

/// <summary>
/// The kind of a markdown token.
/// </summary>
public enum MarkdownTokenType
{
    /// <summary>A plain text line (paragraph content).</summary>
    Text,

    /// <summary>A heading line (# ...).</summary>
    Heading,

    /// <summary>A fenced code block; the content is parsed by a CodeTokenizer sub-tokenizer.</summary>
    CodeBlock,

    /// <summary>A blockquote; the content is parsed by a nested MarkdownTokenizer sub-tokenizer.</summary>
    Blockquote,
}

/// <summary>
/// A token produced by <see cref="MarkdownTokenizer"/>.
/// </summary>
public sealed class MarkdownToken : IToken
{
    public MarkdownToken(MarkdownTokenType type, string value, Metadata? metadata = null)
    {
        Type = type;
        Value = value;
        Metadata = metadata;
    }

    /// <summary>The kind of this token.</summary>
    public MarkdownTokenType Type { get; }

    /// <summary>The raw text of the token.</summary>
    public string Value { get; }

    /// <summary>Block structure metadata, or null for flat tokens.</summary>
    public Metadata? Metadata { get; }
}
