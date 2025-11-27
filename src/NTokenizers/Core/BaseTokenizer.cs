using NTokenizers.CSharp;
using NTokenizers.Markup;
using System;
using System.Collections.Generic;
using System.Text;

namespace NTokenizers.Core;
public abstract class BaseTokenizer<TToken> where TToken : IToken
{
    protected TextReader _reader = default!;
    protected Action<TToken> _onToken = default!;

    /// <summary>
    /// Parses the input stream and invokes the onToken action for each token found.
    /// </summary>
    public void Parse(Stream stream, Action<TToken> onToken)
    {
        _reader = new StreamReader(stream);
        _onToken = onToken;
        Parse();
    }

    internal void Parse(TextReader reader, Action<TToken> onToken)
    {
        _reader = reader;
        _onToken = onToken;
        Parse();
    }

    internal protected abstract void Parse();
}

public abstract class  BaseSubTokenizer<TToken> : BaseTokenizer<TToken> where TToken : IToken
{
    protected string? _stopDelimiter;

    public void Parse(TextReader reader, string? stopDelimiter, Action<TToken> onToken)
    {
        _reader = reader;
        _stopDelimiter = stopDelimiter;
        _onToken = onToken;
    }
}
