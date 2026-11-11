using System.Text;
using NTokenizers.C;
using NTokenizers.Cpp;
using NTokenizers.CSharp;
using NTokenizers.Css;
using NTokenizers.Generic;
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

namespace Markdown;

/// <summary>
/// Shared infrastructure for the <see cref="MarkdownTokenizer"/> test classes. A single
/// <see cref="Tokenize(string, List{CssToken}?)"/> implementation replaces the per-class
/// copies that were previously duplicated verbatim in each test file.
/// </summary>
public abstract class MarkdownTokenizerTestBase
{
    /// <summary>
    /// Tokenizes <paramref name="markdown"/> and flattens the whole token tree into a single
    /// list: every top-level token, plus every token delivered through inline token handlers
    /// (headings, blockquotes, list items, indented code blocks, tables, and language code
    /// blocks, recursively). Language code-block sub-tokens are consumed by no-op handlers
    /// unless <paramref name="cssTokens"/> is supplied, in which case CSS sub-tokens are
    /// captured into it. The second item is the concatenated text of the token stream.
    /// </summary>
    protected static (List<MarkdownToken> tokens, string text, List<CssToken>? cssTokens) Tokenize(string markdown, List<CssToken>? cssTokens = null)
    {
        var tokens = new List<MarkdownToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));

        // Recursively consumes the inline tokens of every token. A token that carries
        // markdown inline metadata (heading, blockquote, list item, indented code block,
        // table) can itself contain markdown tokens - most importantly, a blockquote can nest
        // another blockquote - so those handlers re-dispatch recursively to capture the whole
        // token tree in `tokens`; a flat registration would leave nested block content
        // unhandled. Language code-block metadata streams non-markdown tokens; a no-op
        // handler at every level lets the parser stream and discard that content (and avoids
        // the parser waiting on an unhandled inline token when a code fence appears inside a
        // quoted sub-document). When a cssTokens list is supplied, CSS sub-tokens are
        // captured into it instead of being discarded.
        static async Task RegisterInlineHandlers(MarkdownToken token, List<MarkdownToken> tokens, List<CssToken>? cssTokens)
        {
            if (token.Metadata is HeadingMetadata headingMeta)
            {
                await headingMeta.ParseInlinesAsync(async t =>
                {
                    tokens.Add(t);
                    await RegisterInlineHandlers(t, tokens, cssTokens);
                });
            }
            else if (token.Metadata is BlockquoteMetadata blockquoteMeta)
            {
                await blockquoteMeta.ParseInlinesAsync(async t =>
                {
                    tokens.Add(t);
                    await RegisterInlineHandlers(t, tokens, cssTokens);
                });
            }
            else if (token.Metadata is ListItemMetadata listMeta)
            {
                await listMeta.ParseInlinesAsync(async t =>
                {
                    tokens.Add(t);
                    await RegisterInlineHandlers(t, tokens, cssTokens);
                });
            }
            else if (token.Metadata is OrderedListItemMetadata orderedListMeta)
            {
                await orderedListMeta.ParseInlinesAsync(async t =>
                {
                    tokens.Add(t);
                    await RegisterInlineHandlers(t, tokens, cssTokens);
                });
            }
            else if (token.Metadata is IndentedCodeBlockMetadata indentedCodeMeta)
            {
                // Indented code blocks stream plain markdown Text tokens.
                await indentedCodeMeta.ParseInlinesAsync(async t =>
                {
                    tokens.Add(t);
                });
            }
            else if (token.Metadata is TableMetadata tableMeta)
            {
                await tableMeta.ParseInlinesAsync(async t =>
                {
                    tokens.Add(t);
                    await RegisterInlineHandlers(t, tokens, cssTokens);
                });
            }
            else if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
            {
                await csharpMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is JavaCodeBlockMetadata javaMeta)
            {
                await javaMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is CCodeBlockMetadata cMeta)
            {
                await cMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is CppCodeBlockMetadata cppMeta)
            {
                await cppMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is RustCodeBlockMetadata rustMeta)
            {
                await rustMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is KotlinCodeBlockMetadata kotlinMeta)
            {
                await kotlinMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is GoCodeBlockMetadata goMeta)
            {
                await goMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is SwiftCodeBlockMetadata swiftMeta)
            {
                await swiftMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is JsonCodeBlockMetadata jsonMeta)
            {
                await jsonMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is XmlCodeBlockMetadata xmlMeta)
            {
                await xmlMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is HtmlCodeBlockMetadata htmlMeta)
            {
                await htmlMeta.ParseInlinesAsync(async token =>
                {
                    if (token.Metadata is CssCodeBlockMetadata cssMetadata)
                    {
                        await cssMetadata.ParseInlinesAsync(t =>
                        {
                            return Task.CompletedTask;
                        });
                    }
                    else if (token.Metadata is TypeScriptCodeBlockMetadata tsMetadata)
                    {
                        await tsMetadata.ParseInlinesAsync(t =>
                        {
                            return Task.CompletedTask;
                        });
                    }
                });
            }
            else if (token.Metadata is CssCodeBlockMetadata cssMeta)
            {
                await cssMeta.ParseInlinesAsync(async token =>
                {
                    if (cssTokens is not null)
                    {
                        cssTokens.Add(token);
                    }
                });
            }
            else if (token.Metadata is SqlCodeBlockMetadata sqlMeta)
            {
                await sqlMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
            {
                await tsMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is TomlCodeBlockMetadata tomlMeta)
            {
                await tomlMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is PythonCodeBlockMetadata pythonMeta)
            {
                await pythonMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
            else if (token.Metadata is GenericCodeBlockMetadata gMeta)
            {
                await gMeta.ParseInlinesAsync(t =>
                {
                    return Task.CompletedTask;
                });
            }
        }

        var result = MarkdownTokenizer.Create().ParseAsync(stream, async token =>
        {
            tokens.Add(token);
            await RegisterInlineHandlers(token, tokens, cssTokens);
        }).GetAwaiter().GetResult();
        return (tokens, result, cssTokens);
    }

    /// <summary>
    /// Concatenates the value of every token in <paramref name="tokens"/>, which
    /// <see cref="Tokenize(string, List{CssToken}?)"/> flattens into a single list (including
    /// tokens delivered through inline handlers). For a prose-only document this equals the
    /// input (with the house CRLF-to-LF line-ending normalization), which is the structural
    /// fidelity invariant.
    /// </summary>
    protected static string TokenText(List<MarkdownToken> tokens)
    {
        var sb = new StringBuilder();
        foreach (var token in tokens)
        {
            sb.Append(token.Value);
        }
        return sb.ToString();
    }
}
