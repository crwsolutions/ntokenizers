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
using NTokenizers.Markdown.Metadata;
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
}

/// <summary>
/// Captures every token produced by the Markdown tokenizer, including all inline (sub) tokens.
/// For every InlineMetadata&lt;T&gt; found anywhere in the tree the inline parse is awaited,
/// so no token is missed, at any nesting level.
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
        await MarkdownTokenizer.Create().ParseAsync(stream, ct, async token => 
        {
            if (ct.IsCancellationRequested) return;
            root.Add(await ToEntryAsync(token, ct));
        });
        return root;
    }

    /// <summary>
    /// Parses HTML (e.g. the ToHtml output fragment) with the Html tokenizer and returns
    /// the full token tree, including nested script/style inline tokens.
    /// </summary>
    public static async Task<List<TokenEntry>> CaptureHtmlAsync(string input, CancellationToken ct = default)
    {
        var root = new List<TokenEntry>();
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(input));
        await HtmlTokenizer.Create().ParseAsync(stream, ct, async token => 
        {
            if (ct.IsCancellationRequested) return;
            root.Add(await ToEntryAsync(token, ct));
        });
        return root;
    }

    /// <summary>
    /// Converts a token to a TokenEntry and, when its metadata is an InlineMetadata&lt;T&gt;,
    /// awaits the inline parse that captures the sub tokens recursively. Completes when the
    /// whole subtree of the token has been captured.
    /// </summary>
    private static async Task<TokenEntry> ToEntryAsync(IToken token, CancellationToken ct)
    {
        var entry = new TokenEntry(GetTokenTypeString(token), token.Value, DescribeMetadata(GetMetadata(token)), new List<TokenEntry>());

        // One branch per InlineMetadata<T> in the library. The inline parse is awaited here,
        // inside the container token's handler, so the sub tokens arrive in order and the
        // subtree is fully captured before the parse continues.
        var metadata = GetMetadata(token);
        if (metadata is InlineMetadata<MarkdownToken> markdownInlines and not ListMetadata)
        {
            // Code fences (generic fallback), blockquotes, headings, tables, indented code.
            // ListMetadata (ListStart/ListEnd) never carries a section: the list items are
            // streamed by the tokenizer itself, so ParseInlinesAsync would throw.
            await markdownInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<HtmlToken> htmlInlines)
        {
            // <script>/<style> elements inside an html code fence
            await htmlInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<CSharpToken> csharpInlines)
        {
            await csharpInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<CToken> cInlines)
        {
            await cInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<CppToken> cppInlines)
        {
            await cppInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<CssToken> cssInlines)
        {
            await cssInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<GoToken> goInlines)
        {
            await goInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<JavaToken> javaInlines)
        {
            await javaInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<JsonToken> jsonInlines)
        {
            await jsonInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<KotlinToken> kotlinInlines)
        {
            await kotlinInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<PythonToken> pythonInlines)
        {
            await pythonInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<RustToken> rustInlines)
        {
            await rustInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<SqlToken> sqlInlines)
        {
            await sqlInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<SwiftToken> swiftInlines)
        {
            await swiftInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<TomlToken> tomlInlines)
        {
            await tomlInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<TypescriptToken> typescriptInlines)
        {
            await typescriptInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<XmlToken> xmlInlines)
        {
            await xmlInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
        }
        else if (metadata is InlineMetadata<YamlToken> yamlInlines)
        {
            await yamlInlines.ParseInlinesAsync(async sub =>
            {
                if (ct.IsCancellationRequested) return;
                entry.Children.Add(await ToEntryAsync(sub, ct));
            });
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
