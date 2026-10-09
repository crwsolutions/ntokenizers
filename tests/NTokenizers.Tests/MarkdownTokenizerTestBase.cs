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

        // Recursively registers inline token handlers for every token. A token that carries
        // markdown inline metadata (heading, blockquote, list item, indented code block,
        // table) can itself contain markdown tokens - most importantly, a blockquote can nest
        // another blockquote - so those handlers re-dispatch recursively to capture the whole
        // token tree in `tokens`; a flat registration would leave nested block content
        // unhandled. Language code-block metadata streams non-markdown tokens; a no-op
        // handler at every level lets the parser stream and discard that content (and avoids
        // the parser waiting on an unhandled inline token when a code fence appears inside a
        // quoted sub-document). When a cssTokens list is supplied, CSS sub-tokens are
        // captured into it instead of being discarded.
        static void RegisterInlineHandlers(MarkdownToken token, List<MarkdownToken> tokens, List<CssToken>? cssTokens)
        {
            if (token.Metadata is HeadingMetadata headingMeta)
            {
                headingMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens, cssTokens);
                });
            }
            else if (token.Metadata is BlockquoteMetadata blockquoteMeta)
            {
                blockquoteMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens, cssTokens);
                });
            }
            else if (token.Metadata is ListItemMetadata listMeta)
            {
                listMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens, cssTokens);
                });
            }
            else if (token.Metadata is OrderedListItemMetadata orderedListMeta)
            {
                orderedListMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens, cssTokens);
                });
            }
            else if (token.Metadata is IndentedCodeBlockMetadata indentedCodeMeta)
            {
                // Indented code blocks stream plain markdown Text tokens.
                indentedCodeMeta.RegisterInlineTokenHandler(tokens.Add);
            }
            else if (token.Metadata is TableMetadata tableMeta)
            {
                tableMeta.RegisterInlineTokenHandler(t =>
                {
                    tokens.Add(t);
                    RegisterInlineHandlers(t, tokens, cssTokens);
                });
            }
            else if (token.Metadata is CSharpCodeBlockMetadata csharpMeta)
            {
                csharpMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is JavaCodeBlockMetadata javaMeta)
            {
                javaMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is CCodeBlockMetadata cMeta)
            {
                cMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is CppCodeBlockMetadata cppMeta)
            {
                cppMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is RustCodeBlockMetadata rustMeta)
            {
                rustMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is KotlinCodeBlockMetadata kotlinMeta)
            {
                kotlinMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is GoCodeBlockMetadata goMeta)
            {
                goMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is SwiftCodeBlockMetadata swiftMeta)
            {
                swiftMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is JsonCodeBlockMetadata jsonMeta)
            {
                jsonMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is XmlCodeBlockMetadata xmlMeta)
            {
                xmlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is HtmlCodeBlockMetadata htmlMeta)
            {
                htmlMeta.RegisterInlineTokenHandler(token =>
                {
                    if (token.Metadata is CssCodeBlockMetadata cssMetadata)
                    {
                        cssMetadata.RegisterInlineTokenHandler(token => { });
                    }
                    else if (token.Metadata is TypeScriptCodeBlockMetadata tsMetadata)
                    {
                        tsMetadata.RegisterInlineTokenHandler(token => { });
                    }
                });
            }
            else if (token.Metadata is CssCodeBlockMetadata cssMeta)
            {
                cssMeta.RegisterInlineTokenHandler(token =>
                {
                    if (cssTokens is not null)
                    {
                        cssTokens.Add(token);
                    }
                });
            }
            else if (token.Metadata is SqlCodeBlockMetadata sqlMeta)
            {
                sqlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
            {
                tsMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is TomlCodeBlockMetadata tomlMeta)
            {
                tomlMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is PythonCodeBlockMetadata pythonMeta)
            {
                pythonMeta.RegisterInlineTokenHandler(token => { });
            }
            else if (token.Metadata is GenericCodeBlockMetadata gMeta)
            {
                gMeta.RegisterInlineTokenHandler(token => { });
            }
        }

        var result = MarkdownTokenizer.Create().ParseAsync(stream, token =>
        {
            tokens.Add(token);
            RegisterInlineHandlers(token, tokens, cssTokens);
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
