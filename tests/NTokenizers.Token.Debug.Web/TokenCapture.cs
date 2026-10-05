using NTokenizers.C;
using NTokenizers.Cpp;
using NTokenizers.Core;
using NTokenizers.CSharp;
using NTokenizers.Css;
using NTokenizers.Go;
using NTokenizers.Html;
using NTokenizers.Java;
using NTokenizers.Json;
using NTokenizers.Kotlin;
using NTokenizers.Markdown;
using NTokenizers.Python;
using NTokenizers.Rust;
using NTokenizers.Sql;
using NTokenizers.Swift;
using NTokenizers.Toml;
using NTokenizers.Typescript;
using NTokenizers.Xml;
using NTokenizers.Yaml;

namespace NTokenizers.Token.Debug.Web;

/// <summary>
/// A node in the captured token tree.
/// </summary>
/// <param name="Type">The token type as an enum string (TokenType.ToString()).</param>
/// <param name="Value">The token value.</param>
/// <param name="Metadata">A human readable description of the token metadata, or null.</param>
/// <param name="Children">Nested tokens captured through the token's inline metadata handler.</param>
public sealed class TokenEntry(
    string Type,
    string Value,
    string? Metadata,
    List<TokenEntry> Children
)
{
    public string Type { get; } = Type;
    public string Value { get; } = Value;
    public string? Metadata { get; } = Metadata;
    public List<TokenEntry> Children { get; } = Children;

    internal Task? InlinesTask;
}

/// <summary>
/// Captures every token produced by the Markdown tokenizer, including all inline (sub) tokens.
/// For every InlineMetadata&lt;T&gt; found anywhere in the tree a RegisterInlineTokenHandler is
/// registered, so no token is missed, at any nesting level.
/// </summary>
public static class TokenCapture
{
    private static readonly Dictionary<Type, Func<IToken, Core.Metadata?>> MetadataAccessors = new()
    {
        { typeof(MarkdownToken), t => ((MarkdownToken)t).Metadata },
        { typeof(HtmlToken), t => ((HtmlToken)t).Metadata },
    };

    /// <summary>
    /// Parses the input with the Markdown tokenizer and returns the full token tree.
    /// The returned list only contains root tokens; nested tokens live in Children.
    /// When this method returns, all inline handlers have completed.
    /// </summary>
    public static async Task<List<TokenEntry>> CaptureAsync(string input, CancellationToken ct = default)
    {
        var root = new List<TokenEntry>();
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(input));
        await MarkdownTokenizer.Create().ParseAsync(stream, ct, token =>
        {
            if (ct.IsCancellationRequested) return;
            root.Add(ToEntry(token));
        });
        return root;
    }

    /// <summary>
    /// Converts a token to a TokenEntry and, when its metadata is an InlineMetadata&lt;T&gt;,
    /// registers an inline token handler that captures the sub tokens recursively.
    /// </summary>
    private static TokenEntry ToEntry(IToken token)
    {
        var entry = new TokenEntry(GetTokenTypeString(token), token.Value, DescribeMetadata(GetMetadata(token)), new List<TokenEntry>());

        // One branch per InlineMetadata<T> in the library. Registering synchronously inside the
        // on-token callback is safe: the tokenizer only starts the inline parse after the
        // handler is registered, and RegisterInlineTokenHandler completes when the inline
        // parse is done.
        var metadata = GetMetadata(token);
        if (metadata is InlineMetadata<MarkdownToken> markdownInlines)
        {
            // Code fences (generic fallback), blockquotes, headings, lists, tables, indented code
            entry.InlinesTask = markdownInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<HtmlToken> htmlInlines)
        {
            // <script>/<style> elements inside an html code fence
            entry.InlinesTask = htmlInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<CSharpToken> csharpInlines)
        {
            entry.InlinesTask = csharpInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<CToken> cInlines)
        {
            entry.InlinesTask = cInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<CppToken> cppInlines)
        {
            entry.InlinesTask = cppInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<CssToken> cssInlines)
        {
            entry.InlinesTask = cssInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<GoToken> goInlines)
        {
            entry.InlinesTask = goInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<JavaToken> javaInlines)
        {
            entry.InlinesTask = javaInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<JsonToken> jsonInlines)
        {
            entry.InlinesTask = jsonInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<KotlinToken> kotlinInlines)
        {
            entry.InlinesTask = kotlinInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<PythonToken> pythonInlines)
        {
            entry.InlinesTask = pythonInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<RustToken> rustInlines)
        {
            entry.InlinesTask = rustInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<SqlToken> sqlInlines)
        {
            entry.InlinesTask = sqlInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<SwiftToken> swiftInlines)
        {
            entry.InlinesTask = swiftInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<TomlToken> tomlInlines)
        {
            entry.InlinesTask = tomlInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<TypescriptToken> typescriptInlines)
        {
            entry.InlinesTask = typescriptInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<XmlToken> xmlInlines)
        {
            entry.InlinesTask = xmlInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }
        else if (metadata is InlineMetadata<YamlToken> yamlInlines)
        {
            entry.InlinesTask = yamlInlines.RegisterInlineTokenHandler(sub => entry.Children.Add(ToEntry(sub)), () => { });
        }

        return entry;
    }

    private static Core.Metadata? GetMetadata(IToken token) =>
        MetadataAccessors.TryGetValue(token.GetType(), out var accessor) ? accessor(token) : null;

    private static string GetTokenTypeString(IToken token)
    {
        // All token classes implement IToken<TTokenType>; resolve it through the type's
        // interfaces because generic interfaces are invariant.
        var tokenType = token.GetType();
        var tokenTypeProperty = tokenType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IToken<>))
            .Select(i => i.GetProperty(nameof(IToken<Enum>.TokenType)))
            .FirstOrDefault();

        if (tokenTypeProperty != null)
        {
            return tokenTypeProperty.GetValue(token)?.ToString() ?? tokenType.Name;
        }

        return tokenType.Name;
    }

    private static string? DescribeMetadata(Core.Metadata? metadata)
    {
        if (metadata == null)
        {
            return null;
        }

        var typeName = metadata.GetType().Name;

        if (metadata is ICodeBlockMetadata codeBlock)
        {
            return $"{typeName} Language={codeBlock.Language}";
        }

        var properties = metadata.GetType()
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        var parts = new List<string>();
        foreach (var property in properties)
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var value = property.GetValue(metadata);
            parts.Add($"{property.Name}={FormatValue(value)}");
        }

        return parts.Count == 0 ? typeName : $"{typeName} {string.Join(" ", parts)}";
    }

    private static string FormatValue(object? value)
    {
        if (value == null)
        {
            return "null";
        }

        if (value is string s)
        {
            return $"\"{s}\"";
        }

        return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? value.GetType().Name;
    }
}
