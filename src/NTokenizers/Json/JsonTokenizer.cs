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
    internal protected override void Parse()
    {
        var stack = new Stack<ContainerType>();
        bool inString = false;
        bool inNumber = false;
        bool inKeyword = false;
        bool isExpectingKey = false;
        bool escape = false;
        string delimiter = _stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;

        if (delLength == 0)
        {
            while (true)
            {
                int ic = _reader.Read();
                if (ic == -1)
                {
                    EmitPending(ref inString, ref inNumber, ref inKeyword);
                    break;
                }
                char c = (char)ic;
                ProcessChar(c, ref inString, ref inNumber, ref inKeyword, ref escape, ref isExpectingKey, stack);
            }
        }
        else
        {
            var delQueue = new Queue<char>();
            bool stoppedByDelimiter = false;
            while (true)
            {
                int ic = _reader.Read();
                if (ic == -1)
                {
                    break;
                }
                char c = (char)ic;
                delQueue.Enqueue(c);
                if (delQueue.Count > delLength)
                {
                    char toProcess = delQueue.Dequeue();
                    ProcessChar(toProcess, ref inString, ref inNumber, ref inKeyword, ref escape, ref isExpectingKey, stack);
                }
                if (delQueue.Count == delLength && new string(delQueue.ToArray()) == delimiter)
                {
                    stoppedByDelimiter = true;
                    break;
                }
            }
            if (!stoppedByDelimiter)
            {
                while (delQueue.Count > 0)
                {
                    char toProcess = delQueue.Dequeue();
                    ProcessChar(toProcess, ref inString, ref inNumber, ref inKeyword, ref escape, ref isExpectingKey, stack);
                }
            }
            EmitPending(ref inString, ref inNumber, ref inKeyword);
        }
    }

    private void ProcessChar(char c, ref bool inString, ref bool inNumber, ref bool inKeyword, ref bool escape, ref bool isExpectingKey, Stack<ContainerType> stack)
    {
        if (inString)
        {
            _sb.Append(c);
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
                inString = false;
                string str = _sb.ToString();
                _sb.Clear();
                JsonTokenType type = (stack.Count > 0 && stack.Peek() == ContainerType.Object && isExpectingKey)
                    ? JsonTokenType.PropertyName
                    : JsonTokenType.StringValue;
                _onToken(new JsonToken(type, str));
                if (type == JsonTokenType.PropertyName)
                {
                    isExpectingKey = false;
                }
            }
            return;
        }

        if (inNumber)
        {
            if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '-' || c == '+')
            {
                _sb.Append(c);
                return;
            }
            else
            {
                string num = _sb.ToString();
                _sb.Clear();
                _onToken(new JsonToken(JsonTokenType.Number, num));
                inNumber = false;
                // Fall through to process current c
            }
        }

        if (inKeyword)
        {
            _sb.Append(c);
            string kw = _sb.ToString();
            if (kw == "true")
            {
                _onToken(new JsonToken(JsonTokenType.True, "true"));
                inKeyword = false;
                _sb.Clear();
                return;
            }
            else if (kw == "false")
            {
                _onToken(new JsonToken(JsonTokenType.False, "false"));
                inKeyword = false;
                _sb.Clear();
                return;
            }
            else if (kw == "null")
            {
                _onToken(new JsonToken(JsonTokenType.Null, "null"));
                inKeyword = false;
                _sb.Clear();
                return;
            }
            else if (!"true".StartsWith(kw) && !"false".StartsWith(kw) && !"null".StartsWith(kw))
            {
                // Invalid, reset
                inKeyword = false;
                _sb.Clear();
            }
            else
            {
                return;
            }
        }

        // Handle whitespace
        if (char.IsWhiteSpace(c))
        {
            _sb.Append(c);
            return;
        }
        else
        {
            // Emit any pending whitespace
            if (_sb.Length > 0 && !inNumber && !inKeyword && !inString)
            {
                _onToken(new JsonToken(JsonTokenType.Whitespace, _sb.ToString()));
                _sb.Clear();
            }
        }

        // Structural characters
        switch (c)
        {
            case '{':
                _onToken(new JsonToken(JsonTokenType.StartObject, "{"));
                stack.Push(ContainerType.Object);
                isExpectingKey = true;
                break;
            case '}':
                if (stack.Count > 0 && stack.Peek() == ContainerType.Object)
                {
                    stack.Pop();
                }
                _onToken(new JsonToken(JsonTokenType.EndObject, "}"));
                isExpectingKey = stack.Count > 0 && stack.Peek() == ContainerType.Object;
                break;
            case '[':
                _onToken(new JsonToken(JsonTokenType.StartArray, "["));
                stack.Push(ContainerType.Array);
                isExpectingKey = false;
                break;
            case ']':
                if (stack.Count > 0 && stack.Peek() == ContainerType.Array)
                {
                    stack.Pop();
                }
                _onToken(new JsonToken(JsonTokenType.EndArray, "]"));
                isExpectingKey = stack.Count > 0 && stack.Peek() == ContainerType.Object;
                break;
            case ':':
                _onToken(new JsonToken(JsonTokenType.Colon, ":"));
                isExpectingKey = false;
                break;
            case ',':
                _onToken(new JsonToken(JsonTokenType.Comma, ","));
                isExpectingKey = stack.Count > 0 && stack.Peek() == ContainerType.Object;
                break;
            case '"':
                inString = true;
                _sb.Append(c);
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
                inNumber = true;
                _sb.Append(c);
                break;
            case 't':
            case 'f':
            case 'n':
                inKeyword = true;
                _sb.Append(c);
                break;
            default:
                // Skip invalid characters
                break;
        }
    }

    private void EmitPending(ref bool inString, ref bool inNumber, ref bool inKeyword)
    {
        if (_sb.Length > 0)
        {
            if (inString)
            {
                // Incomplete string, skip or handle
            }
            else if (inNumber)
            {
                _onToken(new JsonToken(JsonTokenType.Number, _sb.ToString()));
            }
            else if (inKeyword)
            {
                // Incomplete, skip
            }
            else
            {
                _onToken(new JsonToken(JsonTokenType.Whitespace, _sb.ToString()));
            }
            _sb.Clear();
        }
        inString = false;
        inNumber = false;
        inKeyword = false;
    }

    private enum ContainerType
    {
        Object,
        Array
    }
}