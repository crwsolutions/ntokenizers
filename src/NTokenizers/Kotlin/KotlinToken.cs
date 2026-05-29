using NTokenizers.Core;
using System.Diagnostics;

namespace NTokenizers.Kotlin;

[DebuggerDisplay("KotlinToken: {TokenType} '{Value}'")]
public class KotlinToken : IToken<KotlinTokenType>
{
    public KotlinTokenType TokenType { get; }
    public string Value { get; }

    public KotlinToken(KotlinTokenType tokenType, string value)
    {
        TokenType = tokenType;
        Value = value;
    }
}
