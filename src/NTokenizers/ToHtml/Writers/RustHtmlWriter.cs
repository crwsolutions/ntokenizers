using NTokenizers.Rust;
using System.Text;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Rust tokens.
/// </summary>
internal sealed class RustHtmlWriter : AbstractTokenToHtmlWriter<RustToken>
{
    internal override void WriteAdditionalCss(StringBuilder css)
    {
        // Rust-only: lifetime annotation style
        css.AppendLine(".tok-rust-lifetime { color: #795E26; }");
        css.AppendLine();
    }

    internal override void WriteHtml(RustToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case RustTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case RustTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case RustTokenType.OpenParenthesis:
            case RustTokenType.CloseParenthesis:
            case RustTokenType.OpenBrace:
            case RustTokenType.CloseBrace:
            case RustTokenType.OpenBracket:
            case RustTokenType.CloseBracket:
            case RustTokenType.Comma:
            case RustTokenType.Dot:
            case RustTokenType.SequenceTerminator:
            case RustTokenType.Colon:
            case RustTokenType.DoubleColon:
            case RustTokenType.FatArrow:
            case RustTokenType.Arrow:
            case RustTokenType.Pound:
            case RustTokenType.At:
            case RustTokenType.QuestionMark:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case RustTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case RustTokenType.CharValue:
                WriteValue(writer, token.Value, "tok-char", inPreBlock: true);
                break;

            case RustTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case RustTokenType.Boolean:
                WriteValue(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case RustTokenType.Lifetime:
                WriteValue(writer, token.Value, "tok-rust-lifetime", inPreBlock: true);
                break;

            case RustTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case RustTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case RustTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case RustTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
