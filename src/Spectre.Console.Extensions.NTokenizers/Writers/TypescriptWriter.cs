using NTokenizers.Typescript;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

public sealed class TypescriptWriter : BaseInlineWriter<TypescriptToken, TypescriptTokenType>
{
    protected override Style GetStyle(TypescriptTokenType token) => token switch
    {
        TypescriptTokenType.OpenParenthesis or TypescriptTokenType.CloseParenthesis => new Style(Color.DeepSkyBlue4_1),
        TypescriptTokenType.Comma => new Style(Color.Yellow),
        TypescriptTokenType.StringValue => new Style(Color.DarkSlateGray1),
        TypescriptTokenType.Number => new Style(Color.Blue),
        TypescriptTokenType.Keyword => new Style(Color.Turquoise2),
        TypescriptTokenType.Identifier => new Style(Color.White),
        TypescriptTokenType.Comment => new Style(Color.Green),
        TypescriptTokenType.Operator => new Style(Color.DeepSkyBlue4_2),
        TypescriptTokenType.And or TypescriptTokenType.Or => new Style(Color.DeepSkyBlue4_2),
        TypescriptTokenType.Equals or TypescriptTokenType.NotEquals => new Style(Color.DeepSkyBlue4_2),
        TypescriptTokenType.In or TypescriptTokenType.NotIn => new Style(Color.DeepSkyBlue4_1),
        TypescriptTokenType.Like or TypescriptTokenType.NotLike => new Style(Color.DeepSkyBlue4_1),
        TypescriptTokenType.Limit => new Style(Color.DeepSkyBlue4_1),
        TypescriptTokenType.Match => new Style(Color.DeepSkyBlue4_1),
        TypescriptTokenType.SequenceTerminator => new Style(Color.Yellow),
        TypescriptTokenType.Dot => new Style(Color.Yellow),
        TypescriptTokenType.Whitespace => new Style(Color.Yellow),
        TypescriptTokenType.DateTimeValue => new Style(Color.Blue),
        TypescriptTokenType.Fingerprint or TypescriptTokenType.Message or TypescriptTokenType.StackFrame or TypescriptTokenType.ExceptionType => new Style(Color.DeepSkyBlue3_1),
        TypescriptTokenType.Application => new Style(Color.DeepSkyBlue3_1),
        TypescriptTokenType.Between => new Style(Color.DeepSkyBlue4_1),
        TypescriptTokenType.NotDefined => new Style(),
        TypescriptTokenType.Invalid => new Style(),
        _ => new Style()
    };
}