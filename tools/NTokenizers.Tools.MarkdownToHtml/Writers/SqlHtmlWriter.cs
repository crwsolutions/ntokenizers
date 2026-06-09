using NTokenizers.Sql;

namespace NTokenizers.Tools.MarkdownToHtml.Writers;

/// <summary>
/// Writer for SQL tokens.
/// </summary>
internal sealed class SqlHtmlWriter : AbstractTokenToHtmlWriter<SqlToken>
{
    internal override void WriteHtml(SqlToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case SqlTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case SqlTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case SqlTokenType.Keyword:
                WriteValue(writer, token.Value, "tok-keyword", inPreBlock: true);
                break;

            case SqlTokenType.Identifier:
                WriteValue(writer, token.Value, "tok-identifier", inPreBlock: true);
                break;

            case SqlTokenType.Operator:
                WriteValue(writer, token.Value, "tok-operator", inPreBlock: true);
                break;

            case SqlTokenType.Comma:
            case SqlTokenType.Dot:
            case SqlTokenType.OpenParenthesis:
            case SqlTokenType.CloseParenthesis:
            case SqlTokenType.SequenceTerminator:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case SqlTokenType.NotDefined:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;

            case SqlTokenType.Comment:
                WriteValue(writer, token.Value, "tok-comment", inPreBlock: true);
                break;

            case SqlTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
