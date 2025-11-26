using NTokenizers.Markup;
using NTokenizers.ShowCase.Writers;
using Spectre.Console;

public sealed class MarkupBlockquoteWriter : BaseInlineWriter<MarkupToken, MarkupTokenType>
{
    protected override void WriteToken(Paragraph liveParagraph, MarkupToken token) => 
        MarkupWriter.Write(liveParagraph, token, GetStyle(token.TokenType));
}
