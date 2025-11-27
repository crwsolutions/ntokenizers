using NTokenizers.CSharp;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public sealed class CSharpWriter : BaseInlineWriter<CSharpToken, CSharpTokenType>
{
    protected override Style GetStyle(CSharpTokenType token) => token switch
    {
        CSharpTokenType.Keyword => new Style(Color.Turquoise2),
        CSharpTokenType.Number => new Style(Color.Blue),
        CSharpTokenType.StringValue => new Style(Color.DarkSlateGray2),
        CSharpTokenType.Comment => new Style(Color.Green),
        CSharpTokenType.Identifier => new Style(Color.White),

        CSharpTokenType.And or CSharpTokenType.Or or CSharpTokenType.Not or
        CSharpTokenType.Equals or CSharpTokenType.NotEquals or
        CSharpTokenType.GreaterThan or CSharpTokenType.LessThan or
        CSharpTokenType.GreaterThanOrEqual or CSharpTokenType.LessThanOrEqual or
        CSharpTokenType.Plus or CSharpTokenType.Minus or
        CSharpTokenType.Multiply or CSharpTokenType.Divide or
        CSharpTokenType.Modulo or CSharpTokenType.Operator
            => new Style(Color.DeepSkyBlue4_2),

        CSharpTokenType.OpenParenthesis or
        CSharpTokenType.CloseParenthesis or
        CSharpTokenType.Comma or
        CSharpTokenType.Dot or
        CSharpTokenType.SequenceTerminator
            => new Style(Color.Yellow),

        CSharpTokenType.Whitespace => new Style(Color.White),
        _ => new Style(Color.White)
    };
}