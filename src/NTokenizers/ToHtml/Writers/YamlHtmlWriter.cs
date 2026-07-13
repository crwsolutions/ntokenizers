using NTokenizers.Yaml;
using System.Text;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for YAML tokens.
/// </summary>
internal sealed class YamlHtmlWriter : AbstractTokenToHtmlWriter<YamlToken>
{
    internal override void WriteAdditionalCss(StringBuilder css)
    {
        // YAML-specific token styles
        css.AppendLine(".tok-yaml-key { color: #800000; }");
        css.AppendLine(".tok-yaml-value { color: #0000FF; }");
        css.AppendLine(".tok-yaml-directive { color: #A9A9A9; font-style: italic; }");
        css.AppendLine(".tok-yaml-anchor { color: #795E26; }");
        css.AppendLine(".tok-yaml-alias { color: #795E26; }");
        css.AppendLine(".tok-yaml-tag { color: #008000; }");
        css.AppendLine();
    }

    internal override void WriteHtml(YamlToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case YamlTokenType.Directive:
                WriteValue(writer, token.Value, "tok-yaml-directive", inPreBlock: true);
                break;

            case YamlTokenType.DirectiveKey:
                WriteValue(writer, token.Value, "tok-yaml-directive", inPreBlock: true);
                break;

            case YamlTokenType.DirectiveValue:
                WriteValue(writer, token.Value, "tok-yaml-value", inPreBlock: true);
                break;

            case YamlTokenType.DocumentStart:
            case YamlTokenType.DocumentEnd:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case YamlTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case YamlTokenType.Key:
                WriteValue(writer, token.Value, "tok-yaml-key", inPreBlock: true);
                break;

            case YamlTokenType.Colon:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case YamlTokenType.Value:
                WriteValue(writer, token.Value, "tok-yaml-value", inPreBlock: true);
                break;

            case YamlTokenType.Quote:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case YamlTokenType.String:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case YamlTokenType.Anchor:
                WriteValue(writer, token.Value, "tok-yaml-anchor", inPreBlock: true);
                break;

            case YamlTokenType.Alias:
                WriteValue(writer, token.Value, "tok-yaml-alias", inPreBlock: true);
                break;

            case YamlTokenType.Tag:
                WriteValue(writer, token.Value, "tok-yaml-tag", inPreBlock: true);
                break;

            case YamlTokenType.FlowSeqStart:
            case YamlTokenType.FlowSeqEnd:
            case YamlTokenType.FlowMapStart:
            case YamlTokenType.FlowMapEnd:
            case YamlTokenType.FlowEntry:
            case YamlTokenType.BlockSeqEntry:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case YamlTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
