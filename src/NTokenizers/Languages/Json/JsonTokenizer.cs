using NTokenizers.Core;

namespace NTokenizers.Json;

/// <summary>
/// Provides functionality for tokenizing JSON or JSON-like text sources.
/// </summary>
public sealed class JsonTokenizer : BaseSubTokenizer<JsonToken>
{
    /// <summary>
    /// Creates a new instance of the <see cref="JsonTokenizer"/>.
    /// </summary>
    public static JsonTokenizer Create() => new();

    /// <summary>
    /// Parses JSON or JSON-like content from the given <see cref="TextReader"/> and
    /// produces a sequence of <see cref="JsonToken"/> objects.
    /// </summary>
    internal protected override async Task ParseAsync(CancellationToken ct)
    {
        var stack = new Stack<ContainerType>();
        var state = new State();

        await TokenizeCharactersAsync(ct, async (c) => await ProcessChar(c, state, stack));

        await EmitPending(state);
    }

    private async Task ProcessChar(char c, State state, Stack<ContainerType> stack)
    {
        if (state.InString)
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
                state.InString = false;
                string str = _buffer.ToString();
                _buffer.Clear();
                JsonTokenType type = (stack.Count > 0 && stack.Peek() == ContainerType.Object && state.IsExpectingKey)
                    ? JsonTokenType.PropertyName
                    : JsonTokenType.StringValue;
                await _onToken(new JsonToken(type, str));
                if (type == JsonTokenType.PropertyName)
                {
                    state.IsExpectingKey = false;
                }
            }
            return;
        }

        if (state.InNumber)
        {
            if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '-' || c == '+')
            {
                _buffer.Append(c);
                return;
            }
            else
            {
                string num = _buffer.ToString();
                _buffer.Clear();
                await _onToken(new JsonToken(JsonTokenType.Number, num));
                state.InNumber = false;
                // Fall through to process current c
            }
        }

        if (state.InKeyword)
        {
            _buffer.Append(c);
            string kw = _buffer.ToString();
            if (kw == "true")
            {
                await _onToken(new JsonToken(JsonTokenType.True, "true"));
                state.InKeyword = false;
                _buffer.Clear();
                return;
            }
            else if (kw == "false")
            {
                await _onToken(new JsonToken(JsonTokenType.False, "false"));
                state.InKeyword = false;
                _buffer.Clear();
                return;
            }
            else if (kw == "null")
            {
                await _onToken(new JsonToken(JsonTokenType.Null, "null"));
                state.InKeyword = false;
                _buffer.Clear();
                return;
            }
            else if (!"true".StartsWith(kw) && !"false".StartsWith(kw) && !"null".StartsWith(kw))
            {
                // Invalid, reset
                state.InKeyword = false;
                _buffer.Clear();
            }
            else
            {
                return;
            }
        }

        // Handle whitespace
        if (char.IsWhiteSpace(c))
        {
            _buffer.Append(c);
            return;
        }
        else
        {
            // Emit any pending whitespace
            if (_buffer.Length > 0 && !state.InNumber && !state.InKeyword && !state.InString)
            {
                await _onToken(new JsonToken(JsonTokenType.Whitespace, _buffer.ToString()));
                _buffer.Clear();
            }
        }

        // Structural characters
        switch (c)
        {
            case '{':
                await _onToken(new JsonToken(JsonTokenType.StartObject, "{"));
                stack.Push(ContainerType.Object);
                state.IsExpectingKey = true;
                break;
            case '}':
                if (stack.Count > 0 && stack.Peek() == ContainerType.Object)
                {
                    stack.Pop();
                }
                await _onToken(new JsonToken(JsonTokenType.EndObject, "}"));
                state.IsExpectingKey = stack.Count > 0 && stack.Peek() == ContainerType.Object;
                break;
            case '[':
                await _onToken(new JsonToken(JsonTokenType.StartArray, "["));
                stack.Push(ContainerType.Array);
                state.IsExpectingKey = false;
                break;
            case ']':
                if (stack.Count > 0 && stack.Peek() == ContainerType.Array)
                {
                    stack.Pop();
                }
                await _onToken(new JsonToken(JsonTokenType.EndArray, "]"));
                state.IsExpectingKey = stack.Count > 0 && stack.Peek() == ContainerType.Object;
                break;
            case ':':
                await _onToken(new JsonToken(JsonTokenType.Colon, ":"));
                state.IsExpectingKey = false;
                break;
            case ',':
                await _onToken(new JsonToken(JsonTokenType.Comma, ","));
                state.IsExpectingKey = stack.Count > 0 && stack.Peek() == ContainerType.Object;
                break;
            case '"':
                state.InString = true;
                _buffer.Append(c);
                break;
            case '-':
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                state.InNumber = true;
                _buffer.Append(c);
                break;
            case 't':
            case 'f':
            case 'n':
                state.InKeyword = true;
                _buffer.Append(c);
                break;
            default:
                // Skip invalid characters
                break;
        }
    }

    private async Task EmitPending(State state)
    {
        if (_buffer.Length > 0)
        {
            if (state.InString)
            {
                // Incomplete string, skip or handle
            }
            else if (state.InNumber)
            {
                await _onToken(new JsonToken(JsonTokenType.Number, _buffer.ToString()));
            }
            else if (state.InKeyword)
            {
                // Incomplete, skip
            }
            else
            {
                await _onToken(new JsonToken(JsonTokenType.Whitespace, _buffer.ToString()));
            }
            _buffer.Clear();
        }
        state.InString = false;
        state.InNumber = false;
        state.InKeyword = false;
    }

    private sealed class State
    {
        public bool InString;
        public bool InNumber;
        public bool InKeyword;
        public bool IsExpectingKey;
        public bool Escape;
    }

    private enum ContainerType
    {
        Object,
        Array
    }
}