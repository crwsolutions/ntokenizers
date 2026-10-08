using NTokenizers.CSharp;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for C# tokens.
/// </summary>
internal sealed class CSharpHtmlWriter : AbstractTokenToHtmlWriter<CSharpToken>
{
    internal override void WriteHtml(CSharpToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case CSharpTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case CSharpTokenType.And:
            case CSharpTokenType.Or:
            case CSharpTokenType.Not:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case CSharpTokenType.Equals:
            case CSharpTokenType.NotEquals:
            case CSharpTokenType.GreaterThan:
            case CSharpTokenType.LessThan:
            case CSharpTokenType.GreaterThanOrEqual:
            case CSharpTokenType.LessThanOrEqual:
            case CSharpTokenType.Plus:
            case CSharpTokenType.Minus:
            case CSharpTokenType.Multiply:
            case CSharpTokenType.Divide:
            case CSharpTokenType.Modulo:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case CSharpTokenType.OpenParenthesis:
            case CSharpTokenType.CloseParenthesis:
            case CSharpTokenType.Comma:
            case CSharpTokenType.Dot:
            case CSharpTokenType.SequenceTerminator:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case CSharpTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case CSharpTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case CSharpTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case CSharpTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case CSharpTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case CSharpTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case CSharpTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
