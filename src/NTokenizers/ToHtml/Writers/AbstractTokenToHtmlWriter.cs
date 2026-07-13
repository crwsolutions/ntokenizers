using NTokenizers.Core;
using System.Text;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Abstract base for token-to-HTML writers. Provides shared escaping and value-writing utilities,
/// a default no-op <see cref="WriteAdditionalCss"/>, and an abstract <see cref="WriteHtml"/> contract.
/// </summary>
/// <typeparam name="TToken">The token type, constrained to <see cref="IToken"/>.</typeparam>
internal abstract class AbstractTokenToHtmlWriter<TToken> : BaseHtmlWriter, IAdditionalCssWriter
    where TToken : IToken
{
    /// <inheritdoc cref="IAdditionalCssWriter.WriteAdditionalCss"/>
    void IAdditionalCssWriter.WriteAdditionalCss(StringBuilder css) => WriteAdditionalCss(css);

    /// <summary>
    /// Appends writer-specific CSS rules. Override to add language-specific styles.
    /// Default implementation does nothing.
    /// </summary>
    internal virtual void WriteAdditionalCss(StringBuilder css)
    {
    }

    /// <summary>
    /// Writes a single token as HTML markup.
    /// </summary>
    /// <param name="token">The token to render.</param>
    /// <param name="writer">The text writer to write HTML to.</param>
    internal abstract void WriteHtml(TToken token, TextWriter writer);
}
