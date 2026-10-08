using System.Text;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Contract for HTML writers that can contribute additional CSS rules.
/// </summary>
internal interface IAdditionalCssWriter
{
    /// <summary>
    /// Appends writer-specific CSS rules to the provided builder.
    /// </summary>
    void WriteAdditionalCss(StringBuilder css);
}
