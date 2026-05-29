using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Go;

/// <summary>
/// Represents a Go token with its type and value.
/// </summary>
[DebuggerDisplay("GoToken: {TokenType} '{Value}'")]
public class GoToken : IToken<GoTokenType>
{
    public GoTokenType TokenType { get; }
    public string Value { get; }

    public GoToken(GoTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
