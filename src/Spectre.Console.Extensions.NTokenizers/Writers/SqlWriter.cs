using NTokenizers.Sql;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public sealed class SqlWriter : BaseInlineWriter<SqlToken, SqlTokenType>
{
    protected override Style GetStyle(SqlTokenType token) => token switch
    {
        SqlTokenType.Number => new Style(Color.DeepSkyBlue3_1),
        SqlTokenType.StringValue => new Style(Color.DarkSlateGray1),
        SqlTokenType.Comment => new Style(Color.Green),
        SqlTokenType.Keyword => new Style(Color.Turquoise2),
        SqlTokenType.Identifier => new Style(Color.White),
        SqlTokenType.Operator => new Style(Color.DeepSkyBlue4_2),
        SqlTokenType.Comma => new Style(Color.DeepSkyBlue4_2),
        SqlTokenType.Dot => new Style(Color.DeepSkyBlue4_2),
        SqlTokenType.OpenParenthesis => new Style(Color.DeepSkyBlue4_2),
        SqlTokenType.CloseParenthesis => new Style(Color.DeepSkyBlue4_2),
        SqlTokenType.SequenceTerminator => new Style(Color.DeepSkyBlue4_2),
        SqlTokenType.NotDefined => new Style(Color.DeepSkyBlue4_2),
        _ => new Style()
    };
}