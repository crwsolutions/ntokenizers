using NTokenizers.Sql;
using Spectre.Console.Extensions.NTokenizers.Styles;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal sealed class SqlWriter(SqlStyles styles) : BaseInlineWriter<SqlToken, SqlTokenType>
{
    protected override Style GetStyle(SqlTokenType token) => token switch
    {
        SqlTokenType.Number => styles.Number,
        SqlTokenType.StringValue => styles.StringValue,
        SqlTokenType.Comment => styles.Comment,
        SqlTokenType.Keyword => styles.Keyword,
        SqlTokenType.Identifier => styles.Identifier,
        SqlTokenType.Operator => styles.Operator,
        SqlTokenType.Comma => styles.Comma,
        SqlTokenType.Dot => styles.Dot,
        SqlTokenType.OpenParenthesis => styles.OpenParenthesis,
        SqlTokenType.CloseParenthesis => styles.CloseParenthesis,
        SqlTokenType.SequenceTerminator => styles.SequenceTerminator,
        SqlTokenType.NotDefined => styles.NotDefined,
        _ => new Style()
    };
}
