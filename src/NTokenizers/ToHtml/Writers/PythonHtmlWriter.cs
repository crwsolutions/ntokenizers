using NTokenizers.Python;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for Python tokens.
/// </summary>
internal sealed class PythonHtmlWriter : AbstractTokenToHtmlWriter<PythonToken>
{
    internal override void WriteHtml(PythonToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case PythonTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case PythonTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case PythonTokenType.OpenParenthesis:
            case PythonTokenType.CloseParenthesis:
            case PythonTokenType.OpenBrace:
            case PythonTokenType.CloseBrace:
            case PythonTokenType.OpenBracket:
            case PythonTokenType.CloseBracket:
            case PythonTokenType.Comma:
            case PythonTokenType.Dot:
            case PythonTokenType.Colon:
            case PythonTokenType.Semicolon:
            case PythonTokenType.Hash:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case PythonTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case PythonTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case PythonTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case PythonTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case PythonTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case PythonTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
