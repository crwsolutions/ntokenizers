using NTokenizers.Markup;

namespace Spectre.Console.Extensions.NTokenizers.Writers;

internal sealed class GenericWriter() : BaseInlineWriter<MarkupToken, MarkupTokenType>;