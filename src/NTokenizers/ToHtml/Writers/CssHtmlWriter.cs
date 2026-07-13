using NTokenizers.Css;
using System.Text;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for CSS tokens.
/// </summary>
internal sealed class CssHtmlWriter : AbstractTokenToHtmlWriter<CssToken>
{
    internal override void WriteAdditionalCss(StringBuilder css)
    {
        // CSS-specific token styles
        css.AppendLine(".tok-css-selector { color: #800000; }");
        css.AppendLine(".tok-css-property { color: #795E26; }");
        css.AppendLine(".tok-css-unit { color: #098658; }");
        css.AppendLine(".tok-css-function { color: #795E26; }");
        css.AppendLine(".tok-css-atrule { color: #0000FF; font-weight: bold; }");
        css.AppendLine(".tok-css-pseudo { color: #800000; }");
        css.AppendLine(".tok-css-dotclass { color: #800000; }");
        css.AppendLine(".tok-css-equals { color: #393B34; }");
        css.AppendLine();
    }

    internal override void WriteHtml(CssToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case CssTokenType.StartRuleSet:
            case CssTokenType.EndRuleSet:
            case CssTokenType.OpenParen:
            case CssTokenType.CloseParen:
            case CssTokenType.LeftBracket:
            case CssTokenType.RightBracket:
            case CssTokenType.Colon:
            case CssTokenType.Semicolon:
            case CssTokenType.Comma:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case CssTokenType.Selector:
                WriteValue(writer, token.Value, "tok-css-selector", inPreBlock: true);
                break;

            case CssTokenType.PseudoElement:
                WriteValue(writer, token.Value, "tok-css-pseudo", inPreBlock: true);
                break;

            case CssTokenType.PropertyName:
                WriteValue(writer, token.Value, "tok-css-property", inPreBlock: true);
                break;

            case CssTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case CssTokenType.Quote:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case CssTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case CssTokenType.Unit:
                WriteValue(writer, token.Value, "tok-css-unit", inPreBlock: true);
                break;

            case CssTokenType.Function:
                WriteValue(writer, token.Value, "tok-css-function", inPreBlock: true);
                break;

            case CssTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case CssTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            case CssTokenType.AtRule:
                WriteValue(writer, token.Value, "tok-css-atrule", inPreBlock: true);
                break;

            case CssTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case CssTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case CssTokenType.DotClass:
                WriteValue(writer, token.Value, "tok-css-dotclass", inPreBlock: true);
                break;

            case CssTokenType.Equals:
                WriteValue(writer, token.Value, "tok-css-equals", inPreBlock: true);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
