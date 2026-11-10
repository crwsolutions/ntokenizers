using NTokenizers.Core;
using NTokenizers.Markdown;

namespace NTokenizers.Generic;

/// <summary>
/// A generic tokenizer that reads text until a specified stop delimiter is encountered.
/// </summary>
public sealed class GenericTokenizer : BaseSubTokenizer<MarkdownToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="GenericTokenizer"/> class.
    /// </summary>
    public static GenericTokenizer Create() => new();

    /// <summary>
    /// Parses the input text reader and invokes the onToken action for each token found,
    /// </summary>
    internal protected override async Task ParseAsync(CancellationToken ct)
    {
        await TokenizeCharactersAsync(ct, ProcessChar);

        await EmitPending();
    }

    private async Task ProcessChar(char toProcess)
    {
        _buffer.Append(toProcess);
        if (toProcess == ' ')
        {
            await EmitPending();
        }
    }

    private async Task EmitPending()
    {
        if (_buffer.Length > 0)
        {
            await _onToken(new MarkdownToken(MarkdownTokenType.Text, _buffer.ToString()));
            _buffer.Clear();
        }
    }
}
