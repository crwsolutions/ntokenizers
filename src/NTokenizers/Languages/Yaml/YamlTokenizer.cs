using NTokenizers.Core;

namespace NTokenizers.Yaml;

/// <summary>
/// Provides functionality for tokenizing YAML or YAML-like text sources.
/// </summary>
public sealed class YamlTokenizer : BaseSubTokenizer<YamlToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="YamlTokenizer"/>.
    /// </summary>
    public static YamlTokenizer Create() => new();

    /// <summary>
    /// Parses YAML or YAML-like content from the given <see cref="TextReader"/> and
    /// produces a sequence of <see cref="YamlToken"/> objects.
    /// </summary>
    protected internal override async Task ParseAsync(CancellationToken ct)
    {
        var stack = new Stack<ContainerType>();
        var state = new State();

        await TokenizeCharactersAsync(ct, async (c) => await ProcessChar(c, state, stack));

        await EmitPending(state);
    }

    private async Task ProcessChar(char c, State state, Stack<ContainerType> stack)
    {
        // Handle comments - comments consume everything until newline
        if (state.InComment)
        {
            _buffer.Append(c);
            if (c == '\n')
            {
                await _onToken(new YamlToken(YamlTokenType.Comment, _buffer.ToString()));
                _buffer.Clear();
                state.InComment = false;
                state.IsLineStart = true;
            }
            return;
        }

        // Handle quoted strings
        if (state.InQuotedString)
        {
            _buffer.Append(c);
            if (state.Escape)
            {
                state.Escape = false;
            }
            else if (c == '\\')
            {
                state.Escape = true;
            }
            else if (c == '"')
            {
                // Emit the string content (buffer now contains: opening quote + content + closing quote)
                string str = _buffer.ToString();
                _buffer.Clear();

                // Extract content between quotes
                if (str.Length > 1)
                {
                    await _onToken(new YamlToken(YamlTokenType.String, str.Substring(1, str.Length - 2)));
                }
                // Emit the closing quote
                await _onToken(new YamlToken(YamlTokenType.Quote, "\""));

                state.InQuotedString = false;
                state.InValue = false;
                state.IsAfterColon = false;
            }
            return;
        }

        if (state.IsDocumentStartOrBlockSeqEntry)
        {
            if (c == '-' && _buffer.Length == 2)
            {
                await _onToken(new YamlToken(YamlTokenType.DocumentStart, "---"));
                _buffer.Clear();
                state.IsDocumentStartOrBlockSeqEntry = false;
                return;
            }
            if (char.IsWhiteSpace(c))
            {
                // Block sequence entry
                await _onToken(new YamlToken(YamlTokenType.BlockSeqEntry, "-"));
                await _onToken(new YamlToken(YamlTokenType.Whitespace, c.ToString()));
                _buffer.Clear();
                state.IsDocumentStartOrBlockSeqEntry = false;
                state.InValue = true;
                return;
            }
            _buffer.Append(c);
            return;
        }

        if (c == '\r') //ignore returns
        {
            return;
        }

        // Handle newlines - they reset line state
        if (c == '\n')
        {
            // Emit pending content
            if (_buffer.Length > 0)
            {
                if (state.InKey)
                {
                    await _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                    state.InKey = false;
                    _buffer.Clear();
                }
                else if (state.InValue)
                {
                    await _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    state.InValue = false;
                    _buffer.Clear();
                }
            }

            // Append newline to buffer - it will be emitted as part of the next Whitespace token
            // along with any following indentation/spaces
            _buffer.Append(c);
            state.IsLineStart = true;
            state.IsAfterColon = false;
            return;
        }

        // Handle whitespace
        if (char.IsWhiteSpace(c))
        {
            // If we're after a colon or block sequence entry, we're starting a value (include leading whitespace)
            if ((state.IsAfterColon || state.IsAfterBlockSeqEntry) && !state.InValue)
            {
                state.InValue = true;
                state.IsAfterColon = false;
                state.IsAfterBlockSeqEntry = false;
            }

            // If we're in a key or value, we need to decide whether to include the whitespace
            if (state.InKey || state.InValue)
            {
                // For keys: never include trailing whitespace - emit immediately
                if (state.InKey)
                {
                    // Emit key without trailing whitespace
                    if (_buffer.Length > 0)
                    {
                        await _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                        _buffer.Clear();
                    }
                    state.InKey = false;
                    // Fall through to handle whitespace
                }
                else if (state.InValue)
                {
                    // For values: accumulate whitespace, but handle specially based on context
                    int nextChar = Peek();

                    if (nextChar == '#')
                    {
                        // Include whitespace before comment
                        _buffer.Append(c);
                        // Check if there's more whitespace
                        if (Peek() == '#')
                        {
                            // Immediately followed by #, emit value with trailing space
                            await _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                            _buffer.Clear();
                            state.InValue = false;
                        }
                        return;
                    }
                    else
                    {
                        // Accumulate whitespace in buffer
                        _buffer.Append(c);
                        // But if next is not whitespace and not part of value, emit
                        if (!char.IsWhiteSpace((char)nextChar) &&
                            (nextChar == ':' || nextChar == ',' || nextChar == ']' || nextChar == '}' || nextChar == '\n' || nextChar == -1))
                        {
                            // Emit value, trim trailing whitespace
                            string val = _buffer.ToString().TrimEnd();
                            if (val.Length > 0)
                            {
                                await _onToken(new YamlToken(YamlTokenType.Value, val));
                            }
                            _buffer.Clear();
                            state.InValue = false;
                            // Re-process just the current whitespace char
                            _buffer.Append(c);
                        }
                        return;
                    }
                }
            }

            _buffer.Append(c);

            return;
        }

        // Non-whitespace means line start is over
        bool wasLineStart = state.IsLineStart;
        if (!char.IsWhiteSpace(c))
        {
            state.IsLineStart = false;
        }

        // Emit any pending whitespace
        if (_buffer.Length > 0 && !state.InKey && !state.InValue)
        {
            await _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
            _buffer.Clear();
        }

        //Check for directives
        if (wasLineStart && c == '%')
        {
            await _onToken(new YamlToken(YamlTokenType.Directive, c.ToString()));
            var readKey = true;
            while (true)
            {
                var cc = Read();
                if (cc == -1)
                {
                    if (_buffer.Length > 0)
                    {
                        if (readKey)
                        {
                            await _onToken(new YamlToken(YamlTokenType.DirectiveKey, _buffer.ToString()));
                        }
                        else
                        {
                            await _onToken(new YamlToken(YamlTokenType.DirectiveValue, _buffer.ToString()));
                        }
                        _buffer.Clear();
                    }
                    return;
                }
                else
                {
                    var rc = (char)cc;
                    if (readKey && char.IsWhiteSpace(rc))
                    {
                        await _onToken(new YamlToken(YamlTokenType.DirectiveKey, _buffer.ToString()));
                        await _onToken(new YamlToken(YamlTokenType.Whitespace, rc.ToString()));
                        _buffer.Clear();
                        readKey = false;
                        continue;
                    }
                    else if (!readKey && (Peek() == '\n' || Peek() == '\r' || Peek() == '\0'))
                    {
                        _buffer.Append(rc);
                        await _onToken(new YamlToken(YamlTokenType.DirectiveValue, _buffer.ToString()));
                        _buffer.Clear();
                        return;
                    }
                    _buffer.Append(rc);
                }
            }
        }

        // Check for document markers at line start
        if (wasLineStart && c == '-')
        {
            // Check for --- or block sequence entry
            state.IsDocumentStartOrBlockSeqEntry = true;
            _buffer.Append(c);
            return;
        }

        if (wasLineStart && c == '.')
        {
            // Check for document end ...
            char next1 = PeekAhead(0);
            char next2 = PeekAhead(1);

            if (next1 == '.' && next2 == '.')
            {
                Read(); // consume second .
                Read(); // consume third .
                await _onToken(new YamlToken(YamlTokenType.DocumentEnd, "..."));
                return;
            }
        }

        // Handle structural characters in flow style
        switch (c)
        {
            case '[':
                if (state.InValue)
                {
                    await _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    state.InValue = false;
                }
                else if (_buffer.Length > 0)
                {
                    await _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
                    _buffer.Clear();
                }
                await _onToken(new YamlToken(YamlTokenType.FlowSeqStart, "["));
                stack.Push(ContainerType.FlowSeq);
                state.IsAfterColon = false;
                state.IsAfterBlockSeqEntry = false;
                return;

            case ']':
                if (state.InValue)
                {
                    await _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    state.InValue = false;
                }
                if (stack.Count > 0 && stack.Peek() == ContainerType.FlowSeq)
                {
                    stack.Pop();
                }
                await _onToken(new YamlToken(YamlTokenType.FlowSeqEnd, "]"));
                state.IsAfterColon = false;
                return;

            case '{':
                if (state.InValue)
                {
                    await _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    state.InValue = false;
                }
                await _onToken(new YamlToken(YamlTokenType.FlowMapStart, "{"));
                stack.Push(ContainerType.FlowMap);
                state.IsAfterColon = false;
                return;

            case '}':
                if (state.InValue)
                {
                    await _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    state.InValue = false;
                }
                if (stack.Count > 0 && stack.Peek() == ContainerType.FlowMap)
                {
                    stack.Pop();
                }
                await _onToken(new YamlToken(YamlTokenType.FlowMapEnd, "}"));
                state.IsAfterColon = false;
                return;

            case ',':
                if (state.InKey)
                {
                    await _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                    _buffer.Clear();
                    state.InKey = false;
                }
                else if (state.InValue)
                {
                    await _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    state.InValue = false;
                }
                await _onToken(new YamlToken(YamlTokenType.FlowEntry, ","));
                state.IsAfterColon = false;
                state.IsAfterBlockSeqEntry = false;
                return;

            case ':':
                // Emit key if we have one
                if (state.InKey)
                {
                    await _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                    _buffer.Clear();
                    state.InKey = false;
                }
                else if (_buffer.Length > 0)
                {
                    // We had accumulated something, it's a key
                    await _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                    _buffer.Clear();
                }
                await _onToken(new YamlToken(YamlTokenType.Colon, ":"));
                state.IsAfterColon = true;
                return;

            case '#':
                // Start of comment
                if (state.InKey)
                {
                    await _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                    _buffer.Clear();
                    state.InKey = false;
                }
                else if (state.InValue)
                {
                    await _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    state.InValue = false;
                }
                state.InComment = true;
                _buffer.Append(c);
                return;

            case '"':
                // Start of quoted string
                if (state.InValue)
                {
                    // If buffer is only whitespace, emit as Whitespace, not Value
                    string bufferContent = _buffer.ToString();
                    if (bufferContent.Length > 0)
                    {
                        if (bufferContent.Trim().Length == 0)
                        {
                            await _onToken(new YamlToken(YamlTokenType.Whitespace, bufferContent));
                        }
                        else
                        {
                            await _onToken(new YamlToken(YamlTokenType.Value, bufferContent));
                        }
                    }
                    _buffer.Clear();
                }
                else if (_buffer.Length > 0)
                {
                    // Emit any pending whitespace
                    await _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
                    _buffer.Clear();
                }
                await _onToken(new YamlToken(YamlTokenType.Quote, "\""));
                state.InQuotedString = true;
                state.InValue = false;
                _buffer.Append(c);
                return;

            case '&':
                // Anchor
                if (state.InValue)
                {
                    // If buffer is only whitespace, emit as Whitespace, not Value
                    string bufferContent = _buffer.ToString();
                    if (bufferContent.Length > 0)
                    {
                        if (bufferContent.Trim().Length == 0)
                        {
                            await _onToken(new YamlToken(YamlTokenType.Whitespace, bufferContent));
                        }
                        else
                        {
                            await _onToken(new YamlToken(YamlTokenType.Value, bufferContent));
                        }
                    }
                    _buffer.Clear();
                    state.InValue = false;
                }
                else if (_buffer.Length > 0)
                {
                    // Emit any pending whitespace
                    await _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
                    _buffer.Clear();
                }
                _buffer.Append(c);
                // Read anchor name
                while (true)
                {
                    int next = Peek();
                    if (next == -1 || char.IsWhiteSpace((char)next) || next == ':' || next == ',' || next == ']' || next == '}')
                        break;
                    _buffer.Append((char)Read());
                }
                await _onToken(new YamlToken(YamlTokenType.Anchor, _buffer.ToString()));
                _buffer.Clear();
                return;

            case '*':
                // Alias
                if (state.InValue)
                {
                    // If buffer is only whitespace, emit as Whitespace, not Value
                    string bufferContent = _buffer.ToString();
                    if (bufferContent.Length > 0)
                    {
                        if (bufferContent.Trim().Length == 0)
                        {
                            await _onToken(new YamlToken(YamlTokenType.Whitespace, bufferContent));
                        }
                        else
                        {
                            await _onToken(new YamlToken(YamlTokenType.Value, bufferContent));
                        }
                    }
                    _buffer.Clear();
                    state.InValue = false;
                }
                else if (_buffer.Length > 0)
                {
                    // Emit any pending whitespace
                    await _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
                    _buffer.Clear();
                }
                _buffer.Append(c);
                // Read alias name
                while (true)
                {
                    int next = Peek();
                    if (next == -1 || char.IsWhiteSpace((char)next) || next == ':' || next == ',' || next == ']' || next == '}')
                        break;
                    _buffer.Append((char)Read());
                }
                await _onToken(new YamlToken(YamlTokenType.Alias, _buffer.ToString()));
                _buffer.Clear();
                return;

            case '!':
                // Tag
                if (state.InValue)
                {
                    // If buffer is only whitespace, emit as Whitespace, not Value
                    string bufferContent = _buffer.ToString();
                    if (bufferContent.Length > 0)
                    {
                        if (bufferContent.Trim().Length == 0)
                        {
                            await _onToken(new YamlToken(YamlTokenType.Whitespace, bufferContent));
                        }
                        else
                        {
                            await _onToken(new YamlToken(YamlTokenType.Value, bufferContent));
                        }
                    }
                    _buffer.Clear();
                    state.InValue = false;
                }
                else if (_buffer.Length > 0)
                {
                    // Emit any pending whitespace
                    await _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
                    _buffer.Clear();
                }
                _buffer.Append(c);
                // Check for double !!
                if (Peek() == '!')
                {
                    _buffer.Append((char)Read());
                }
                // Read tag name
                while (true)
                {
                    int next = Peek();
                    if (next == -1 || char.IsWhiteSpace((char)next) || next == ':' || next == ',' || next == ']' || next == '}')
                        break;
                    _buffer.Append((char)Read());
                }
                await _onToken(new YamlToken(YamlTokenType.Tag, _buffer.ToString()));
                _buffer.Clear();
                return;

            default:
                // Regular character - part of key or value
                if (state.IsAfterColon || state.IsAfterBlockSeqEntry)
                {
                    // After colon or block seq entry, we're in a value
                    state.InValue = true;
                    state.IsAfterColon = false;
                    state.IsAfterBlockSeqEntry = false;
                }
                else if (!state.InValue)
                {
                    // Check if we're in a flow sequence - if so, treat as value
                    if (stack.Count > 0 && stack.Peek() == ContainerType.FlowSeq)
                    {
                        state.InValue = true;
                    }
                    else
                    {
                        // Before colon, we're in a key
                        state.InKey = true;
                    }
                }
                _buffer.Append(c);
                return;
        }
    }

    private async Task EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            if (state.InComment)
            {
                await _onToken(new YamlToken(YamlTokenType.Comment, _buffer.ToString()));
            }
            else if (state.InQuotedString)
            {
                // Incomplete string - emit as string
                string str = _buffer.ToString();
                if (str.Length > 1)
                {
                    await _onToken(new YamlToken(YamlTokenType.String, str.Substring(1)));
                }
            }
            else if (state.InKey)
            {
                await _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
            }
            else if (state.InValue)
            {
                await _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
            }
            else
            {
                await _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
            }
            _buffer.Clear();
        }

        state.InQuotedString = false;
        state.InKey = false;
        state.InValue = false;
        state.InComment = false;
    }

    private sealed class State
    {
        public bool InQuotedString;
        public bool InKey;
        public bool InValue;
        public bool InComment;
        public bool Escape;
        public bool IsLineStart = true;
        public bool IsAfterColon;
        public bool IsAfterBlockSeqEntry;
        public bool IsDocumentStartOrBlockSeqEntry;
    }

    private enum ContainerType
    {
        FlowSeq,
        FlowMap,
        BlockSeq,
        BlockMap
    }
}
