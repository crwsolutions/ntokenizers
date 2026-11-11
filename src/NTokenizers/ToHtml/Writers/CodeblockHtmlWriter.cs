using System.Text;
using NTokenizers.C;
using NTokenizers.Core;
using NTokenizers.Cpp;
using NTokenizers.CSharp;
using NTokenizers.Css;
using NTokenizers.Generic;
using NTokenizers.Go;
using NTokenizers.Html;
using NTokenizers.Java;
using NTokenizers.Json;
using NTokenizers.Kotlin;
using NTokenizers.Python;
using NTokenizers.Rust;
using NTokenizers.Sql;
using NTokenizers.Swift;
using NTokenizers.Toml;
using NTokenizers.Typescript;
using NTokenizers.Xml;
using NTokenizers.Yaml;

namespace NTokenizers.ToHtml.Writers;

internal class CodeblockHtmlWriter : BaseHtmlWriter, IAdditionalCssWriter
{
    private readonly HtmlHtmlWriter _htmlWriter = new();
    private readonly CssHtmlWriter _cssWriter = new();
    private readonly JsonHtmlWriter _jsonWriter = new();
    private readonly XmlHtmlWriter _xmlWriter = new();
    private readonly TypeScriptHtmlWriter _tsWriter = new();
    private readonly CSharpHtmlWriter _csharpWriter = new();
    private readonly CHtmlWriter _cWriter = new();
    private readonly CppHtmlWriter _cppWriter = new();
    private readonly GoHtmlWriter _goWriter = new();
    private readonly JavaHtmlWriter _javaWriter = new();
    private readonly KotlinHtmlWriter _kotlinWriter = new();
    private readonly PythonHtmlWriter _pythonWriter = new();
    private readonly RustHtmlWriter _rustWriter = new();
    private readonly SqlHtmlWriter _sqlWriter = new();
    private readonly SwiftHtmlWriter _swiftWriter = new();
    private readonly YamlHtmlWriter _yamlWriter = new();
    private readonly TomlHtmlWriter _tomlWriter = new();
    private readonly GenericHtmlWriter _genericWriter = new();

    /// <summary>
    /// Writes a code block token, delegating to the appropriate language writer.
    /// </summary>
    internal async Task WriteCodeBlockAsync(ICodeBlockMetadata codeBlock, TextWriter writer, CancellationToken ct = default)
    {
        var language = codeBlock.Language;
        var escapedLanguage = EscapeHtml(language);

        // Write container with header
        await writer.WriteAsync("<div class=\"code-block-container\">\n");
        await writer.WriteAsync("<div class=\"code-block-header\">\n");
        await writer.WriteAsync($"<span class=\"code-block-language\">{escapedLanguage}</span>\n");
        await writer.WriteAsync($"<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n");
        await writer.WriteAsync("</div>\n");
        await writer.WriteAsync("<pre><code class=\"language-" + escapedLanguage + "\">");

        if (codeBlock is CSharpCodeBlockMetadata csharpMeta)
        {
            await WriteCodeBlockContentAsync(csharpMeta, writer, async t => await _csharpWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is JsonCodeBlockMetadata jsonMeta)
        {
            await WriteCodeBlockContentAsync(jsonMeta, writer, async t => await _jsonWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is XmlCodeBlockMetadata xmlMeta)
        {
            await WriteCodeBlockContentAsync(xmlMeta, writer, async t => await _xmlWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is HtmlCodeBlockMetadata htmlMeta)
        {
            await WriteHtmlCodeBlockContentAsync(htmlMeta, writer, ct);
        }
        else if (codeBlock is CssCodeBlockMetadata cssMeta)
        {
            await WriteCodeBlockContentAsync(cssMeta, writer, async t => await _cssWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is TypeScriptCodeBlockMetadata tsMeta)
        {
            await WriteCodeBlockContentAsync(tsMeta, writer, async t => await _tsWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is YamlCodeBlockMetadata yamlMeta)
        {
            await WriteCodeBlockContentAsync(yamlMeta, writer, async t => await _yamlWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is SqlCodeBlockMetadata sqlMeta)
        {
            await WriteCodeBlockContentAsync(sqlMeta, writer, async t => await _sqlWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is TomlCodeBlockMetadata tomlMeta)
        {
            await WriteCodeBlockContentAsync(tomlMeta, writer, async t => await _tomlWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is JavaCodeBlockMetadata javaMeta)
        {
            await WriteCodeBlockContentAsync(javaMeta, writer, async t => await _javaWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is CCodeBlockMetadata cMeta)
        {
            await WriteCodeBlockContentAsync(cMeta, writer, async t => await _cWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is CppCodeBlockMetadata cppMeta)
        {
            await WriteCodeBlockContentAsync(cppMeta, writer, async t => await _cppWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is RustCodeBlockMetadata rustMeta)
        {
            await WriteCodeBlockContentAsync(rustMeta, writer, async t => await _rustWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is KotlinCodeBlockMetadata kotlinMeta)
        {
            await WriteCodeBlockContentAsync(kotlinMeta, writer, async t => await _kotlinWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is GoCodeBlockMetadata goMeta)
        {
            await WriteCodeBlockContentAsync(goMeta, writer, async t => await _goWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is SwiftCodeBlockMetadata swiftMeta)
        {
            await WriteCodeBlockContentAsync(swiftMeta, writer, async t => await _swiftWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is PythonCodeBlockMetadata pythonMeta)
        {
            await WriteCodeBlockContentAsync(pythonMeta, writer, async t => await _pythonWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else if (codeBlock is GenericCodeBlockMetadata genericMeta)
        {
            await WriteCodeBlockContentAsync(genericMeta, writer, async t => await _genericWriter.WriteHtmlAsync(t, writer, ct), ct);
        }
        else
        {
            // Fallback for any unrecognized metadata type
            await WriteCodeBlockContentFallbackAsync(codeBlock as InlineMetadata<IToken>, writer, ct);
        }
    }

    private async Task WriteHtmlCodeBlockContentAsync(HtmlCodeBlockMetadata meta, TextWriter writer, CancellationToken ct = default)
    {
        // Html tokens arrive in stream order; nested script/style sections are consumed
        // through their own metadata before the Html tokenizer continues.
        await meta.ParseInlinesAsync(async token =>
        {
            if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
            {
                // Nested script element — delegate to TypeScript writer
                await tsMeta.ParseInlinesAsync(async t => await _tsWriter.WriteHtmlAsync(t, writer, ct));
            }
            else if (token.Metadata is CssCodeBlockMetadata cssMeta)
            {
                // Nested style element — delegate to CSS writer
                await cssMeta.ParseInlinesAsync(async t => await _cssWriter.WriteHtmlAsync(t, writer, ct));
            }
            else
            {
                // Regular HTML token
                await _htmlWriter.WriteHtmlAsync(token, writer, ct);
            }
        });
        await WriteCodeBlockCloseTags(writer, ct);
    }

    private async Task WriteCodeBlockContentAsync<TToken>(InlineMetadata<TToken> meta, TextWriter writer, Func<TToken, Task> writeToken, CancellationToken ct) where TToken : IToken
    {
        await meta.ParseInlinesAsync(async token => await writeToken(token));
        await WriteCodeBlockCloseTags(writer, ct);
    }

    private async Task WriteCodeBlockContentFallbackAsync(InlineMetadata<IToken>? meta, TextWriter writer, CancellationToken ct = default)
    {
        if (meta == null)
        {
            await WriteCodeBlockCloseTags(writer, ct);
            return;
        }
        await meta.ParseInlinesAsync(async token => await WriteValueAsync(writer, token.Value, null, inPreBlock: true));
        await WriteCodeBlockCloseTags(writer, ct);
    }

    private static async Task WriteCodeBlockCloseTags(TextWriter writer, CancellationToken ct)
    {
        await writer.WriteAsync("</code></pre>\n");
        await writer.WriteAsync("</div>\n");
    }

    public void WriteAdditionalCss(StringBuilder bob)
    {
        WriteCommonCss(bob);

        _cssWriter.WriteAdditionalCss(bob);
        _jsonWriter.WriteAdditionalCss(bob);
        _xmlWriter.WriteAdditionalCss(bob);
        _tsWriter.WriteAdditionalCss(bob);
        _csharpWriter.WriteAdditionalCss(bob);
        _cWriter.WriteAdditionalCss(bob);
        _cppWriter.WriteAdditionalCss(bob);
        _goWriter.WriteAdditionalCss(bob);
        _javaWriter.WriteAdditionalCss(bob);
        _kotlinWriter.WriteAdditionalCss(bob);
        _pythonWriter.WriteAdditionalCss(bob);
        _rustWriter.WriteAdditionalCss(bob);
        _sqlWriter.WriteAdditionalCss(bob);
        _swiftWriter.WriteAdditionalCss(bob);
        _yamlWriter.WriteAdditionalCss(bob);
        _tomlWriter.WriteAdditionalCss(bob);
        _genericWriter.WriteAdditionalCss(bob);
    }

    /// <summary>
    /// Writes common CSS rules shared across all tokenizers.
    /// </summary>
    private static void WriteCommonCss(StringBuilder bob)
    {
        bob.AppendLine("/* === Common token styles === */");
        bob.AppendLine(".tok-keyword { color: #0000FF; font-weight: bold; }");
        bob.AppendLine(".tok-comment { color: #008000; font-style: italic; }");
        bob.AppendLine(".tok-string { color: #A31515; }");
        bob.AppendLine(".tok-number { color: #098658; }");
        bob.AppendLine(".tok-identifier { color: #267F99; }");
        bob.AppendLine(".tok-operator { color: #393B34; }");
        bob.AppendLine(".tok-punctuation { color: #393B34; }");
        bob.AppendLine(".tok-boolean { color: #0000FF; }");
        bob.AppendLine(".tok-null { color: #0000FF; }");
        bob.AppendLine(".tok-undefined { color: #393B34; }");
        bob.AppendLine(".tok-char { color: #A31515; }");
        bob.AppendLine(".tok-datetime { color: #098658; }");
        bob.AppendLine();

        bob.AppendLine("/* === Markup (ML) token styles — shared by HTML and XML === */");
        bob.AppendLine(".tok-ml-element { color: #800000; }");
        bob.AppendLine(".tok-ml-attribute { color: #FF0000; }");
        bob.AppendLine(".tok-ml-attr-value { color: #0000FF; }");
        bob.AppendLine(".tok-ml-doctype { color: #A9A9A9; font-style: italic; }");
        bob.AppendLine(".tok-ml-cdata { color: #008000; }");
        bob.AppendLine(".tok-ml-processing { color: #A9A9A9; font-style: italic; }");
        bob.AppendLine();

        bob.AppendLine("/* === Markdown styles === */");
        bob.AppendLine(".tok-inline-code { background-color: #F4F4F4; padding: 2px 4px; border-radius: 3px; font-family: monospace; }");
        bob.AppendLine();

        bob.AppendLine("/* === Document layout === */");
        bob.AppendLine("body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif; line-height: 1.6; color: #24292E; }");
        bob.AppendLine(".markdown-document { max-width: 800px; margin: 0 auto; padding: 20px; }");
        bob.AppendLine("pre { background-color: #F6F8FA; padding: 16px; overflow: auto; line-height: 1.45; border-radius: 6px; }");
        bob.AppendLine("code { font-family: SFMono-Regular, Consolas, 'Liberation Mono', Menlo, monospace; font-size: 85%; }");
        bob.AppendLine("pre code { background: none; padding: 0; font-size: 100%; }");
        bob.AppendLine();
        bob.AppendLine("/* === Code block header === */");
        bob.AppendLine(".code-block-container { margin: 16px 0; border-radius: 6px; overflow: hidden; }");
        bob.AppendLine(".code-block-header { display: flex; justify-content: space-between; align-items: center; padding: 6px 12px; background-color: #E8EAED; border-bottom: 1px solid #D0D7DE; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif; font-size: 12px; }");
        bob.AppendLine(".code-block-language { color: #57606A; font-weight: 600; text-transform: lowercase; }");
        bob.AppendLine(".code-block-copy { background: none; border: 1px solid #D0D7DE; border-radius: 6px; padding: 2px 10px; font-size: 12px; color: #57606A; cursor: pointer; font-family: inherit; transition: background-color 0.15s, color 0.15s; }");
        bob.AppendLine(".code-block-copy:hover { background-color: #D0D7DE; color: #24292E; }");
        bob.AppendLine(".code-block-copied { background-color: #2DA44E; color: #FFFFFF; border-color: #2DA44E; }");
        bob.AppendLine(".code-block-container pre { margin: 0; border-radius: 0; }");
        bob.AppendLine("blockquote { border-left: 4px solid #DFE2E5; color: #6A737D; padding: 0 1em; margin: 0; }");
        bob.AppendLine("table { border-collapse: collapse; width: 100%; margin-bottom: 16px; }");
        bob.AppendLine("th, td { border: 1px solid #Dfe2E5; padding: 6px 13px; }");
        bob.AppendLine("th { background-color: #F6F8FA; }");
        bob.AppendLine("tr:nth-child(even) { background-color: #F6F8FA; }");
        bob.AppendLine("hr { border-top: 1px solid #Dfe2E5; }");
        bob.AppendLine("em { font-style: italic; }");
        bob.AppendLine("strong { font-weight: bold; }");
        bob.AppendLine("a { color: #0366D6; text-decoration: none; }");
        bob.AppendLine("img { max-width: 100%; }");
        bob.AppendLine();
    }
}
