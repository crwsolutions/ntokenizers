using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Rust;

[DebuggerDisplay("RustToken: {TokenType} '{Value}'")]
public class RustToken : IToken<RustTokenType>
{
    public RustTokenType TokenType { get; }
    public string Value { get; }

    public RustToken(RustTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
