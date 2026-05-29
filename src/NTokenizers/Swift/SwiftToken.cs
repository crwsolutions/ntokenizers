using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Swift;

[DebuggerDisplay("SwiftToken: {TokenType} '{Value}'")]
public class SwiftToken : IToken<SwiftTokenType>
{
    public SwiftTokenType TokenType { get; }
    public string Value { get; }

    public SwiftToken(SwiftTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
