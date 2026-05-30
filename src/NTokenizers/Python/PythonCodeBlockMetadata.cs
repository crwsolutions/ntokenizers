using NTokenizers.Core;

namespace NTokenizers.Python;

/// <summary>
/// Metadata for Python code block tokens with syntax highlighting support.
/// </summary>
public sealed class PythonCodeBlockMetadata(string language) : CodeBlockMetadata<PythonToken>(language)
{
    internal override BaseSubTokenizer<PythonToken> CreateTokenizer() => PythonTokenizer.Create();
}
