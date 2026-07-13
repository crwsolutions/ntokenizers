using NTokenizers.Core;
using System.Text;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Abstract base for metadata-to-HTML writers. Provides a default no-op <see cref="WriteAdditionalCss"/>
/// and an abstract <see cref="WriteContentAsync"/> contract.
/// </summary>
/// <typeparam name="TMetadata">The metadata type, constrained to <see cref="InlineMetadata"/>.</typeparam>
internal abstract class AbstractMetadataToHtmlWriter<TMetadata> : IAdditionalCssWriter
    where TMetadata : InlineMetadata
{
    /// <inheritdoc cref="IAdditionalCssWriter.WriteAdditionalCss"/>
    void IAdditionalCssWriter.WriteAdditionalCss(StringBuilder css) => WriteAdditionalCss(css);

    /// <summary>
    /// Appends writer-specific CSS rules. Override to add structural styles.
    /// Default implementation does nothing.
    /// </summary>
    internal virtual void WriteAdditionalCss(StringBuilder css)
    {
    }

    /// <summary>
    /// Writes the content associated with the metadata as HTML markup.
    /// </summary>
    /// <param name="metadata">The metadata whose inline tokens should be rendered.</param>
    /// <param name="writer">The text writer to write HTML to.</param>
    internal abstract Task WriteContentAsync(TMetadata metadata, TextWriter writer);
}
