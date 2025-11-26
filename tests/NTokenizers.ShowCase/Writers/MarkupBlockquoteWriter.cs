using NTokenizers.Markup;
using Spectre.Console;

namespace NTokenizers.ShowCase.Writers;

internal sealed class MarkupBlockquoteWriter : BaseInlineWriter<MarkupToken, MarkupTokenType>
{
    protected override void WriteToken(Paragraph liveParagraph, MarkupToken token) => 
        MarkupWriter.Write(liveParagraph, token, GetStyle(token.TokenType));
}
