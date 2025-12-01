using NTokenizers.Markup;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal sealed class MarkupBlockquoteWriter : BaseInlineWriter<MarkupToken, MarkupTokenType>
{
    protected override async Task WriteTokenAsync(Paragraph liveParagraph, MarkupToken token) =>
        await MarkupWriter.Create().WriteAsync(liveParagraph, token, GetStyle(token.TokenType));
}
