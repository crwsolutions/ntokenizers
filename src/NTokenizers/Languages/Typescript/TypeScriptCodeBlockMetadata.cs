using System.Text;
using NTokenizers.Core;

namespace NTokenizers.Typescript;

/// <summary>
/// Metadata for TypeScript code block tokens with syntax highlighting support.
/// </summary>
public sealed class TypeScriptCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<TypescriptToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<TypescriptToken> CreateTokenizer() => TypescriptTokenizer.Create();
}
