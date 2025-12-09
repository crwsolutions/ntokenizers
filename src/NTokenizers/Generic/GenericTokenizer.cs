using NTokenizers.Core;
using NTokenizers.Markup;

namespace NTokenizers.Generic;

/// <summary>
/// A generic tokenizer that reads text until a specified stop delimiter is encountered.
/// </summary>
public sealed class GenericTokenizer : BaseSubTokenizer<MarkupToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="GenericTokenizer"/> class.
    /// </summary>
    public static GenericTokenizer Create() => new();

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found,
    /// </summary>
    internal protected override Task ParseAsync(CancellationToken ct)
    {
        TokenizeCharacters(ct, ProcessChar);

        EmitPending();

        return Task.CompletedTask;
    }

    private void ProcessChar(char toProcess)
    {
        _buffer.Append(toProcess);
        if (toProcess == ' ')
        {
            EmitPending();
        }
    }

    private void EmitPending()
    {
        if (_buffer.Length > 0)
        {
            _onToken(new MarkupToken(MarkupTokenType.Text, _buffer.ToString()));
            _buffer.Clear();
        }
    }
}
