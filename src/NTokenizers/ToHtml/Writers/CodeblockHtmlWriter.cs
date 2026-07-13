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
using System.Text;

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
    internal async Task WriteCodeBlockAsync(ICodeBlockMetadata codeBlock, TextWriter writer)
    {
        var language = codeBlock.Language;
        var escapedLanguage = EscapeHtml(language);

        // Write container with header
        writer.Write("<div class=\"code-block-container\">\n");
        writer.Write("<div class=\"code-block-header\">\n");
        writer.Write($"<span class=\"code-block-language\">{escapedLanguage}</span>\n");
        writer.Write($"<button class=\"code-block-copy\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">Copy</button>\n");
        writer.Write("</div>\n");
        writer.Write("<pre><code class=\"language-" + escapedLanguage + "\">");

        if (codeBlock is CSharpCodeBlockMetadata csharpMeta)
        {
            await WriteCodeBlockContentAsync(csharpMeta, writer, t => _csharpWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is JsonCodeBlockMetadata jsonMeta)
        {
            await WriteCodeBlockContentAsync(jsonMeta, writer, t => _jsonWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is XmlCodeBlockMetadata xmlMeta)
        {
            await WriteCodeBlockContentAsync(xmlMeta, writer, t => _xmlWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is HtmlCodeBlockMetadata htmlMeta)
        {
            await WriteHtmlCodeBlockContentAsync(htmlMeta, writer);
        }
        else if (codeBlock is CssCodeBlockMetadata cssMeta)
        {
            await WriteCodeBlockContentAsync(cssMeta, writer, t => _cssWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is TypeScriptCodeBlockMetadata tsMeta)
        {
            await WriteCodeBlockContentAsync(tsMeta, writer, t => _tsWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is YamlCodeBlockMetadata yamlMeta)
        {
            await WriteCodeBlockContentAsync(yamlMeta, writer, t => _yamlWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is SqlCodeBlockMetadata sqlMeta)
        {
            await WriteCodeBlockContentAsync(sqlMeta, writer, t => _sqlWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is TomlCodeBlockMetadata tomlMeta)
        {
            await WriteCodeBlockContentAsync(tomlMeta, writer, t => _tomlWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is JavaCodeBlockMetadata javaMeta)
        {
            await WriteCodeBlockContentAsync(javaMeta, writer, t => _javaWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is CCodeBlockMetadata cMeta)
        {
            await WriteCodeBlockContentAsync(cMeta, writer, t => _cWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is CppCodeBlockMetadata cppMeta)
        {
            await WriteCodeBlockContentAsync(cppMeta, writer, t => _cppWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is RustCodeBlockMetadata rustMeta)
        {
            await WriteCodeBlockContentAsync(rustMeta, writer, t => _rustWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is KotlinCodeBlockMetadata kotlinMeta)
        {
            await WriteCodeBlockContentAsync(kotlinMeta, writer, t => _kotlinWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is GoCodeBlockMetadata goMeta)
        {
            await WriteCodeBlockContentAsync(goMeta, writer, t => _goWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is SwiftCodeBlockMetadata swiftMeta)
        {
            await WriteCodeBlockContentAsync(swiftMeta, writer, t => _swiftWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is PythonCodeBlockMetadata pythonMeta)
        {
            await WriteCodeBlockContentAsync(pythonMeta, writer, t => _pythonWriter.WriteHtml(t, writer));
        }
        else if (codeBlock is GenericCodeBlockMetadata genericMeta)
        {
            await WriteCodeBlockContentAsync(genericMeta, writer, t => _genericWriter.WriteHtml(t, writer));
        }
        else
        {
            // Fallback for any unrecognized metadata type
            await WriteCodeBlockContentFallbackAsync(codeBlock as InlineMetadata<IToken>, writer);
        }
    }

    private async Task WriteHtmlCodeBlockContentAsync(HtmlCodeBlockMetadata meta, TextWriter writer)
    {
        await meta.RegisterInlineTokenHandler(async token =>
        {
            if (token.Metadata is TypeScriptCodeBlockMetadata tsMeta)
            {
                // Nested script element — delegate to TypeScript writer
                await tsMeta.RegisterInlineTokenHandler(t => _tsWriter.WriteHtml(t, writer));
            }
            else if (token.Metadata is CssCodeBlockMetadata cssMeta)
            {
                // Nested style element — delegate to CSS writer
                await cssMeta.RegisterInlineTokenHandler(t => _cssWriter.WriteHtml(t, writer));
            }
            else
            {
                // Regular HTML token
                _htmlWriter.WriteHtml(token, writer);
            }
        },
        () => WriteCodeBlockCloseTags(writer));
    }

    private async Task WriteCodeBlockContentAsync<TToken>(InlineMetadata<TToken> meta, TextWriter writer, Action<TToken> writeToken) where TToken : IToken
    {
        await meta.RegisterInlineTokenHandler(writeToken, () => WriteCodeBlockCloseTags(writer));
    }

    private async Task WriteCodeBlockContentFallbackAsync(InlineMetadata<IToken>? meta, TextWriter writer)
    {
        if (meta == null)
        {
            WriteCodeBlockCloseTags(writer);
            return;
        }
        await meta.RegisterInlineTokenHandler(token =>
        {
            WriteValue(writer, token.Value, null, inPreBlock: true);
        },
        () => WriteCodeBlockCloseTags(writer));
    }

    private static void WriteCodeBlockCloseTags(TextWriter writer)
    {
        writer.Write("</code></pre>\n");
        writer.Write("</div>\n");
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
        bob.AppendLine(".tok-bold { font-weight: bold; }");
        bob.AppendLine(".tok-italic { font-style: italic; }");
        bob.AppendLine(".tok-strikethrough { text-decoration: line-through; }");
        bob.AppendLine(".tok-inline-code { background-color: #F4F4F4; padding: 2px 4px; border-radius: 3px; font-family: monospace; }");
        bob.AppendLine(".tok-inserted { background-color: #E6FFEC; }");
        bob.AppendLine(".tok-marked { background-color: #FFF5B8; }");
        bob.AppendLine(".tok-hr { display: block; border-top: 1px solid #DDD; margin: 16px 0; }");
        bob.AppendLine(".tok-link { color: #0366D6; text-decoration: none; }");
        bob.AppendLine(".tok-subscript { vertical-align: sub; font-size: 0.8em; }");
        bob.AppendLine(".tok-superscript { vertical-align: super; font-size: 0.8em; }");
        bob.AppendLine();

        bob.AppendLine("/* === Document layout === */");
        bob.AppendLine("body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif; line-height: 1.6; color: #24292E; max-width: 800px; margin: 0 auto; padding: 20px; }");
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
        bob.AppendLine("img { max-width: 100%; }");
        bob.AppendLine();
    }
}
