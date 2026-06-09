using NTokenizers.Typescript;

namespace NTokenizers.Tools.MarkdownToHtml.Writers;

/// <summary>
/// Writer for TypeScript/JavaScript tokens.
/// </summary>
internal sealed class TypeScriptHtmlWriter : AbstractTokenToHtmlWriter<TypescriptToken>
{
    internal override void WriteHtml(TypescriptToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case TypescriptTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case TypescriptTokenType.And:
            case TypescriptTokenType.Or:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case TypescriptTokenType.Application:
            case TypescriptTokenType.ExceptionType:
            case TypescriptTokenType.Fingerprint:
            case TypescriptTokenType.Message:
            case TypescriptTokenType.StackFrame:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case TypescriptTokenType.Between:
            case TypescriptTokenType.In:
            case TypescriptTokenType.Like:
            case TypescriptTokenType.Limit:
            case TypescriptTokenType.Match:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case TypescriptTokenType.CloseParenthesis:
            case TypescriptTokenType.OpenParenthesis:
            case TypescriptTokenType.Comma:
            case TypescriptTokenType.SequenceTerminator:
            case TypescriptTokenType.Dot:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case TypescriptTokenType.DateTimeValue:
                WriteValue(writer, token.Value, "tok-datetime", inPreBlock: true);
                break;

            case TypescriptTokenType.Equals:
            case TypescriptTokenType.NotEquals:
            case TypescriptTokenType.NotIn:
            case TypescriptTokenType.NotLike:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case TypescriptTokenType.Invalid:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case TypescriptTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case TypescriptTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case TypescriptTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case TypescriptTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case TypescriptTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case TypescriptTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case TypescriptTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
