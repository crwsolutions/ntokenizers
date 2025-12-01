using NTokenizers.Markup;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal sealed class MarkupBlockquoteWriter : BaseInlineWriter<MarkupToken, MarkupTokenType>
{
    protected override void WriteToken(Paragraph liveParagraph, MarkupToken token) =>
        MarkupWriter.WriteAsync(liveParagraph, token, GetStyle(token.TokenType)).Wait();
}
