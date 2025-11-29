using NTokenizers.Core;
using NTokenizers.Extensions;

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
                if (delQueue.IsEqualTo(delimiter))
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

            if (stoppedByDelimiter)
            {
                StripFinalLineFeed();
            }

            EmitPending(ref inString, ref inNumber, ref inKeyword);
        }
    }

    private void ProcessChar(char c, ref bool inString, ref bool inNumber, ref bool inKeyword, ref bool escape, ref bool isExpectingKey, Stack<ContainerType> stack)
    {
        if (inString)
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
                inString = false;
                string str = _buffer.ToString();
                _buffer.Clear();
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
                _buffer.Append(c);
                return;
            }
            else
            {
                string num = _buffer.ToString();
                _buffer.Clear();
                _onToken(new JsonToken(JsonTokenType.Number, num));
                inNumber = false;
                // Fall through to process current c
            }
        }

        if (inKeyword)
        {
            _buffer.Append(c);
            string kw = _buffer.ToString();
            if (kw == "true")
            {
                _onToken(new JsonToken(JsonTokenType.True, "true"));
                inKeyword = false;
                _buffer.Clear();
                return;
            }
            else if (kw == "false")
            {
                _onToken(new JsonToken(JsonTokenType.False, "false"));
                inKeyword = false;
                _buffer.Clear();
                return;
            }
            else if (kw == "null")
            {
                _onToken(new JsonToken(JsonTokenType.Null, "null"));
                inKeyword = false;
                _buffer.Clear();
                return;
            }
            else if (!"true".StartsWith(kw) && !"false".StartsWith(kw) && !"null".StartsWith(kw))
            {
                // Invalid, reset
                inKeyword = false;
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
            if (_buffer.Length > 0 && !inNumber && !inKeyword && !inString)
            {
                _onToken(new JsonToken(JsonTokenType.Whitespace, _buffer.ToString()));
                _buffer.Clear();
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
                inNumber = true;
                _buffer.Append(c);
                break;
            case 't':
            case 'f':
            case 'n':
                inKeyword = true;
                _buffer.Append(c);
                break;
            default:
                // Skip invalid characters
                break;
        }
    }

    private void EmitPending(ref bool inString, ref bool inNumber, ref bool inKeyword)
    {
        if (_buffer.Length > 0)
        {
            if (inString)
            {
                // Incomplete string, skip or handle
            }
            else if (inNumber)
            {
                _onToken(new JsonToken(JsonTokenType.Number, _buffer.ToString()));
            }
            else if (inKeyword)
            {
                // Incomplete, skip
            }
            else
            {
                _onToken(new JsonToken(JsonTokenType.Whitespace, _buffer.ToString()));
            }
            _buffer.Clear();
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