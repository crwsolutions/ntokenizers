namespace Poc.Core;

/// <summary>
/// The sample markdown used by the console and web validation apps. It
/// exercises every architectural path: a heading, plain text, a code block
/// (sub-tokenizer with a different token type) and a blockquote containing
/// a code block (recursion through a nested sub-tokenizer).
/// </summary>
public static class PocSample
{
    /// <summary>The sample markdown document.</summary>
    public const string Markdown = """
        # PoC sample

        This is a plain paragraph.

        ```csharp
        var token = "async";
        // a comment
        int n = 42;
        ```

        > A quote with nested markdown:
        >
        > ```js
        > let x = 1;
        > ```
        >
        > And nested text.

        Last paragraph.
        """;
}
