using System.Text;
using NTokenizers.Css;

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

    internal override async Task WriteHtmlAsync(CssToken token, TextWriter writer, CancellationToken ct = default)
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
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case CssTokenType.Selector:
                await WriteValueAsync(writer, token.Value, "tok-css-selector", inPreBlock: true);
                break;

            case CssTokenType.PseudoElement:
                await WriteValueAsync(writer, token.Value, "tok-css-pseudo", inPreBlock: true);
                break;

            case CssTokenType.PropertyName:
                await WriteValueAsync(writer, token.Value, "tok-css-property", inPreBlock: true);
                break;

            case CssTokenType.StringValue:
                await WriteValueAsync(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case CssTokenType.Quote:
                await WriteValueAsync(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case CssTokenType.Number:
                await WriteValueAsync(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case CssTokenType.Unit:
                await WriteValueAsync(writer, token.Value, "tok-css-unit", inPreBlock: true);
                break;

            case CssTokenType.Function:
                await WriteValueAsync(writer, token.Value, "tok-css-function", inPreBlock: true);
                break;

            case CssTokenType.Comment:
                await WriteValueAsync(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case CssTokenType.Whitespace:
                await writer.WriteAsync(token.Value);
                break;

            case CssTokenType.AtRule:
                await WriteValueAsync(writer, token.Value, "tok-css-atrule", inPreBlock: true);
                break;

            case CssTokenType.Identifier:
                await WriteValueAsync(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case CssTokenType.Operator:
                await WriteValueAsync(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case CssTokenType.DotClass:
                await WriteValueAsync(writer, token.Value, "tok-css-dotclass", inPreBlock: true);
                break;

            case CssTokenType.Equals:
                await WriteValueAsync(writer, token.Value, "tok-css-equals", inPreBlock: true);
                break;

            default:
                await WriteValueAsync(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
