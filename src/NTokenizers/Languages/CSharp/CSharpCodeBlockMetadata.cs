using System.Text;
using NTokenizers.Core;

namespace NTokenizers.CSharp;

/// <summary>
/// Metadata for C# code block tokens with syntax highlighting support.
/// </summary>
public sealed class CSharpCodeBlockMetadata(string language, TextReader reader, StringBuilder capture, Queue<char> lookahead, string stopDelimiter) : CodeBlockMetadata<CSharpToken>(language, reader, capture, lookahead, stopDelimiter)
{
    internal override BaseSubTokenizer<CSharpToken> CreateTokenizer() => CSharpTokenizer.Create();
}
