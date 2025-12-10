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
    protected internal override Task ParseAsync(CancellationToken ct)
    {
        var stack = new Stack<ContainerType>();
        bool inQuotedString = false;
        bool inKey = false;
        bool inValue = false;
        bool inComment = false;
        bool escape = false;
        bool isLineStart = true;
        bool isAfterColon = false;
        bool isAfterBlockSeqEntry = false;

        TokenizeCharacters(ct, (c) => ProcessChar(c, ref inQuotedString, ref inKey, ref inValue, ref inComment, ref escape, ref isLineStart, ref isAfterColon, ref isAfterBlockSeqEntry, stack));

        EmitPending(ref inQuotedString, ref inKey, ref inValue, ref inComment);

        return Task.CompletedTask;
    }

    private void ProcessChar(char c, ref bool inQuotedString, ref bool inKey, ref bool inValue, ref bool inComment, ref bool escape, ref bool isLineStart, ref bool isAfterColon, ref bool isAfterBlockSeqEntry, Stack<ContainerType> stack)
    {
        // Handle comments - comments consume everything until newline
        if (inComment)
        {
            _buffer.Append(c);
            if (c == '\n')
            {
                _onToken(new YamlToken(YamlTokenType.Comment, _buffer.ToString()));
                _buffer.Clear();
                inComment = false;
                isLineStart = true;
            }
            return;
        }

        // Handle quoted strings
        if (inQuotedString)
        {
            _buffer.Append(c);
            if (escape)
            {
                escape = false;
            }
            else if (c == '\\')
            {
                escape = true;
            }
            else if (c == '"')
            {
                // Emit the string content (buffer now contains: opening quote + content + closing quote)
                string str = _buffer.ToString();
                _buffer.Clear();
                
                // Extract content between quotes
                if (str.Length > 1)
                {
                    _onToken(new YamlToken(YamlTokenType.String, str.Substring(1, str.Length - 2)));
                }
                // Emit the closing quote
                _onToken(new YamlToken(YamlTokenType.Quote, "\""));
                
                inQuotedString = false;
                inValue = false;
                isAfterColon = false;
            }
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
                if (inKey)
                {
                    _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                    inKey = false;
                    _buffer.Clear();
                }
                else if (inValue)
                {
                    _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    inValue = false;
                    _buffer.Clear();
                }
            }

            // Append newline to buffer - it will be emitted as part of the next Whitespace token
            // along with any following indentation/spaces
            _buffer.Append(c);
            isLineStart = true;
            isAfterColon = false;
            return;
        }

        // Handle whitespace
        if (char.IsWhiteSpace(c))
        {
            // If we're after a colon or block sequence entry, we're starting a value (include leading whitespace)
            if ((isAfterColon || isAfterBlockSeqEntry) && !inValue)
            {
                inValue = true;
                isAfterColon = false;
                isAfterBlockSeqEntry = false;
            }
            
            // If we're in a key or value, we need to decide whether to include the whitespace
            if (inKey || inValue)
            {
                // For keys: never include trailing whitespace - emit immediately
                if (inKey)
                {
                    // Emit key without trailing whitespace
                    if (_buffer.Length > 0)
                    {
                        _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                        _buffer.Clear();
                    }
                    inKey = false;
                    // Fall through to handle whitespace
                }
                else if (inValue)
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
                            _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                            _buffer.Clear();
                            inValue = false;
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
                                _onToken(new YamlToken(YamlTokenType.Value, val));
                            }
                            _buffer.Clear();
                            inValue = false;
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
        bool wasLineStart = isLineStart;
        if (!char.IsWhiteSpace(c))
        {
            isLineStart = false;
        }

        // Emit any pending whitespace
        if (_buffer.Length > 0 && !inKey && !inValue)
        {
            _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
            _buffer.Clear();
        }

        //Check for directives
        if (wasLineStart && c == '%')
        {
            _onToken(new YamlToken(YamlTokenType.Directive, c.ToString()));
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
                            _onToken(new YamlToken(YamlTokenType.DirectiveKey, _buffer.ToString()));
                        }
                        else
                        {
                            _onToken(new YamlToken(YamlTokenType.DirectiveValue, _buffer.ToString()));
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
                        _onToken(new YamlToken(YamlTokenType.DirectiveKey, _buffer.ToString()));
                        _onToken(new YamlToken(YamlTokenType.Whitespace, rc.ToString()));
                        _buffer.Clear();
                        readKey = false;
                        continue;
                    }
                    else if (!readKey && (Peek() == '\n' || Peek() == '\r' || Peek() == '\0'))
                    {
                        _buffer.Append(rc);
                        _onToken(new YamlToken(YamlTokenType.DirectiveValue, _buffer.ToString()));
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
            char next1 = PeekAhead(0);
            char next2 = PeekAhead(1);
            
            if (next1 == '-' && next2 == '-')
            {
                // Document start
                Read(); // consume second -
                Read(); // consume third -
                _onToken(new YamlToken(YamlTokenType.DocumentStart, "---"));
                return;
            }
            else if (char.IsWhiteSpace(next1) || next1 == '\0')
            {
                // Block sequence entry
                _onToken(new YamlToken(YamlTokenType.BlockSeqEntry, "-"));
                isAfterColon = false;
                isAfterBlockSeqEntry = true;
                if (char.IsWhiteSpace(next1))
                {
                    var whitespace = (char)Read();
                    _onToken(new YamlToken(YamlTokenType.Whitespace, $"{whitespace}"));
                }
                return;
            }
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
                _onToken(new YamlToken(YamlTokenType.DocumentEnd, "..."));
                return;
            }
        }

        // Handle structural characters in flow style
        switch (c)
        {
            case '[':
                if (inValue)
                {
                    _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    inValue = false;
                }
                else if (_buffer.Length > 0)
                {
                    _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
                    _buffer.Clear();
                }
                _onToken(new YamlToken(YamlTokenType.FlowSeqStart, "["));
                stack.Push(ContainerType.FlowSeq);
                isAfterColon = false;
                isAfterBlockSeqEntry = false;
                return;

            case ']':
                if (inValue)
                {
                    _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    inValue = false;
                }
                if (stack.Count > 0 && stack.Peek() == ContainerType.FlowSeq)
                {
                    stack.Pop();
                }
                _onToken(new YamlToken(YamlTokenType.FlowSeqEnd, "]"));
                isAfterColon = false;
                return;

            case '{':
                if (inValue)
                {
                    _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    inValue = false;
                }
                _onToken(new YamlToken(YamlTokenType.FlowMapStart, "{"));
                stack.Push(ContainerType.FlowMap);
                isAfterColon = false;
                return;

            case '}':
                if (inValue)
                {
                    _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    inValue = false;
                }
                if (stack.Count > 0 && stack.Peek() == ContainerType.FlowMap)
                {
                    stack.Pop();
                }
                _onToken(new YamlToken(YamlTokenType.FlowMapEnd, "}"));
                isAfterColon = false;
                return;

            case ',':
                if (inKey)
                {
                    _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                    _buffer.Clear();
                    inKey = false;
                }
                else if (inValue)
                {
                    _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    inValue = false;
                }
                _onToken(new YamlToken(YamlTokenType.FlowEntry, ","));
                isAfterColon = false;
                isAfterBlockSeqEntry = false;
                return;

            case ':':
                // Emit key if we have one
                if (inKey)
                {
                    _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                    _buffer.Clear();
                    inKey = false;
                }
                else if (_buffer.Length > 0)
                {
                    // We had accumulated something, it's a key
                    _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                    _buffer.Clear();
                }
                _onToken(new YamlToken(YamlTokenType.Colon, ":"));
                isAfterColon = true;
                return;

            case '#':
                // Start of comment
                if (inKey)
                {
                    _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
                    _buffer.Clear();
                    inKey = false;
                }
                else if (inValue)
                {
                    _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
                    _buffer.Clear();
                    inValue = false;
                }
                inComment = true;
                _buffer.Append(c);
                return;

            case '"':
                // Start of quoted string
                if (inValue)
                {
                    // If buffer is only whitespace, emit as Whitespace, not Value
                    string bufferContent = _buffer.ToString();
                    if (bufferContent.Length > 0)
                    {
                        if (bufferContent.Trim().Length == 0)
                        {
                            _onToken(new YamlToken(YamlTokenType.Whitespace, bufferContent));
                        }
                        else
                        {
                            _onToken(new YamlToken(YamlTokenType.Value, bufferContent));
                        }
                    }
                    _buffer.Clear();
                }
                else if (_buffer.Length > 0)
                {
                    // Emit any pending whitespace
                    _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
                    _buffer.Clear();
                }
                _onToken(new YamlToken(YamlTokenType.Quote, "\""));
                inQuotedString = true;
                inValue = false;
                _buffer.Append(c);
                return;

            case '&':
                // Anchor
                if (inValue)
                {
                    // If buffer is only whitespace, emit as Whitespace, not Value
                    string bufferContent = _buffer.ToString();
                    if (bufferContent.Length > 0)
                    {
                        if (bufferContent.Trim().Length == 0)
                        {
                            _onToken(new YamlToken(YamlTokenType.Whitespace, bufferContent));
                        }
                        else
                        {
                            _onToken(new YamlToken(YamlTokenType.Value, bufferContent));
                        }
                    }
                    _buffer.Clear();
                    inValue = false;
                }
                else if (_buffer.Length > 0)
                {
                    // Emit any pending whitespace
                    _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
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
                _onToken(new YamlToken(YamlTokenType.Anchor, _buffer.ToString()));
                _buffer.Clear();
                return;

            case '*':
                // Alias
                if (inValue)
                {
                    // If buffer is only whitespace, emit as Whitespace, not Value
                    string bufferContent = _buffer.ToString();
                    if (bufferContent.Length > 0)
                    {
                        if (bufferContent.Trim().Length == 0)
                        {
                            _onToken(new YamlToken(YamlTokenType.Whitespace, bufferContent));
                        }
                        else
                        {
                            _onToken(new YamlToken(YamlTokenType.Value, bufferContent));
                        }
                    }
                    _buffer.Clear();
                    inValue = false;
                }
                else if (_buffer.Length > 0)
                {
                    // Emit any pending whitespace
                    _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
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
                _onToken(new YamlToken(YamlTokenType.Alias, _buffer.ToString()));
                _buffer.Clear();
                return;

            case '!':
                // Tag
                if (inValue)
                {
                    // If buffer is only whitespace, emit as Whitespace, not Value
                    string bufferContent = _buffer.ToString();
                    if (bufferContent.Length > 0)
                    {
                        if (bufferContent.Trim().Length == 0)
                        {
                            _onToken(new YamlToken(YamlTokenType.Whitespace, bufferContent));
                        }
                        else
                        {
                            _onToken(new YamlToken(YamlTokenType.Value, bufferContent));
                        }
                    }
                    _buffer.Clear();
                    inValue = false;
                }
                else if (_buffer.Length > 0)
                {
                    // Emit any pending whitespace
                    _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
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
                _onToken(new YamlToken(YamlTokenType.Tag, _buffer.ToString()));
                _buffer.Clear();
                return;

            default:
                // Regular character - part of key or value
                if (isAfterColon || isAfterBlockSeqEntry)
                {
                    // After colon or block seq entry, we're in a value
                    inValue = true;
                    isAfterColon = false;
                    isAfterBlockSeqEntry = false;
                }
                else if (!inValue)
                {
                    // Check if we're in a flow sequence - if so, treat as value
                    if (stack.Count > 0 && stack.Peek() == ContainerType.FlowSeq)
                    {
                        inValue = true;
                    }
                    else
                    {
                        // Before colon, we're in a key
                        inKey = true;
                    }
                }
                _buffer.Append(c);
                return;
        }
    }

    private void EmitPending(ref bool inQuotedString, ref bool inKey, ref bool inValue, ref bool inComment)
    {
        if (_buffer.Length > 0)
        {
            if (inComment)
            {
                _onToken(new YamlToken(YamlTokenType.Comment, _buffer.ToString()));
            }
            else if (inQuotedString)
            {
                // Incomplete string - emit as string
                string str = _buffer.ToString();
                if (str.Length > 1)
                {
                    _onToken(new YamlToken(YamlTokenType.String, str.Substring(1)));
                }
            }
            else if (inKey)
            {
                _onToken(new YamlToken(YamlTokenType.Key, _buffer.ToString()));
            }
            else if (inValue)
            {
                _onToken(new YamlToken(YamlTokenType.Value, _buffer.ToString()));
            }
            else
            {
                _onToken(new YamlToken(YamlTokenType.Whitespace, _buffer.ToString()));
            }
            _buffer.Clear();
        }
        
        inQuotedString = false;
        inKey = false;
        inValue = false;
        inComment = false;
    }

    private enum ContainerType
    {
        FlowSeq,
        FlowMap,
        BlockSeq,
        BlockMap
    }
}
