using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Cpp;

/// <summary>
/// Represents a C++ token with its type and value.
/// </summary>
[DebuggerDisplay("CppToken: {TokenType} '{Value}'")]
public class CppToken : IToken<CppTokenType>
{
    public CppTokenType TokenType { get; }
    public string Value { get; }

    public CppToken(CppTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
