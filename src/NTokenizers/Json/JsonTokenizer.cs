using System.Text;

namespace NTokenizers.Json;

/// <summary>
/// Provides functionality for tokenizing JSON or JSON-like text sources.
/// </summary>
public static class JsonTokenizer
{
    /// <summary>
    /// Parses JSON or JSON-like content from the given <see cref="Stream"/> and
    /// produces a sequence of <see cref="JsonToken"/> objects.
    /// </summary>
    /// <param name="stream">
    /// The input stream containing the text to tokenize.  
    /// The stream is read as UTF-8.
    /// </param>
    /// <param name="onToken">
    /// A callback invoked for each <see cref="JsonToken"/> produced during parsing.
    /// This delegate must not be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="stream"/> or <paramref name="onToken"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// The following example demonstrates how to parse a simple JSON snippet:
    /// <code>
    /// var json = "{ \"name\": \"Alice\", \"age\": 30 }";
    /// using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
    ///
    /// JsonTokenizer.Parse(ms, token =>
    /// {
    ///     Console.WriteLine($"{token.Kind}: {token.Value}");
    /// });
    /// </code>
    ///
    /// The next example shows parsing JSON that is embedded inside Markdown.
    /// The tokenizer stops when the code fence delimiter is reached:
    /// <code>
    /// var markdown = "```json\n{ \"value\": 123 }\n``` extra";
    ///
    /// using var ms = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
    ///
    /// JsonTokenizer.Parse(ms, "```", token =>
    /// {
    ///     Console.WriteLine($"{token.Kind}: {token.Value}");
    /// });
    /// </code>
    /// </example>
    public static void Parse(Stream stream, Action<JsonToken> onToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        Parse(reader, null, onToken);
    }

    /// <summary>
    /// Parses JSON or JSON-like content from the given <see cref="TextReader"/> and
    /// produces a sequence of <see cref="JsonToken"/> objects.
    /// </summary>
    /// <param name="reader">
    /// The input reader containing the text to tokenize.
    /// </param>
    /// <param name="stopDelimiter">
    /// An optional string that, when encountered in the input, instructs the tokenizer
    /// to stop parsing and return control to the caller.  
    /// This is typically used when the tokenizer is operating as a sub-tokenizer
    /// inside another language (for example, JSON inside Markdown or another format),
    /// where parsing should stop when reaching a delimiter such as a Markdown code
    /// fence (<c>```</c>).
    /// If <c>null</c>, the tokenizer parses until the end of the stream.
    /// </param>
    /// <param name="onToken">
    /// A callback invoked for each <see cref="JsonToken"/> produced during parsing.
    /// This delegate must not be <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="reader"/> or <paramref name="onToken"/> is <c>null</c>.
    /// </exception>
    public static void Parse(TextReader reader, string? stopDelimiter, Action<JsonToken> onToken)
    {
        var sb = new StringBuilder();
        var stack = new Stack<ContainerType>();
        bool inString = false;
        bool inNumber = false;
        bool inKeyword = false;
        bool isExpectingKey = false;
        bool escape = false;
        string delimiter = stopDelimiter ?? string.Empty;
        int delLength = delimiter.Length;

        if (delLength == 0)
        {
            while (true)
            {
                int ic = reader.Read();
                if (ic == -1)
                {
                    EmitPending(sb, ref inString, ref inNumber, ref inKeyword, onToken);
                    break;
                }
                char c = (char)ic;
                ProcessChar(c, ref inString, ref inNumber, ref inKeyword, ref escape, ref isExpectingKey, sb, onToken, stack);
            }
        }
        else
        {
            var delQueue = new Queue<char>();
            bool stoppedByDelimiter = false;
            while (true)
            {
                int ic = reader.Read();
                if (ic == -1)
                {
                    break;
                }
                char c = (char)ic;
                delQueue.Enqueue(c);
                if (delQueue.Count > delLength)
                {
                    char toProcess = delQueue.Dequeue();
                    ProcessChar(toProcess, ref inString, ref inNumber, ref inKeyword, ref escape, ref isExpectingKey, sb, onToken, stack);
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
                    ProcessChar(toProcess, ref inString, ref inNumber, ref inKeyword, ref escape, ref isExpectingKey, sb, onToken, stack);
                }
            }
            EmitPending(sb, ref inString, ref inNumber, ref inKeyword, onToken);
        }
    }

    private static void ProcessChar(char c, ref bool inString, ref bool inNumber, ref bool inKeyword, ref bool escape, ref bool isExpectingKey, StringBuilder sb, Action<JsonToken> onToken, Stack<ContainerType> stack)
    {
        if (inString)
        {
            sb.Append(c);
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
                string str = sb.ToString();
                sb.Clear();
                JsonTokenType type = (stack.Count > 0 && stack.Peek() == ContainerType.Object && isExpectingKey)
                    ? JsonTokenType.PropertyName
                    : JsonTokenType.StringValue;
                onToken(new JsonToken(type, str));
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
                sb.Append(c);
                return;
            }
            else
            {
                string num = sb.ToString();
                sb.Clear();
                onToken(new JsonToken(JsonTokenType.Number, num));
                inNumber = false;
                // Fall through to process current c
            }
        }

        if (inKeyword)
        {
            sb.Append(c);
            string kw = sb.ToString();
            if (kw == "true")
            {
                onToken(new JsonToken(JsonTokenType.True, "true"));
                inKeyword = false;
                sb.Clear();
                return;
            }
            else if (kw == "false")
            {
                onToken(new JsonToken(JsonTokenType.False, "false"));
                inKeyword = false;
                sb.Clear();
                return;
            }
            else if (kw == "null")
            {
                onToken(new JsonToken(JsonTokenType.Null, "null"));
                inKeyword = false;
                sb.Clear();
                return;
            }
            else if (!"true".StartsWith(kw) && !"false".StartsWith(kw) && !"null".StartsWith(kw))
            {
                // Invalid, reset
                inKeyword = false;
                sb.Clear();
            }
            else
            {
                return;
            }
        }

        // Handle whitespace
        if (char.IsWhiteSpace(c))
        {
            sb.Append(c);
            return;
        }
        else
        {
            // Emit any pending whitespace
            if (sb.Length > 0 && !inNumber && !inKeyword && !inString)
            {
                onToken(new JsonToken(JsonTokenType.Whitespace, sb.ToString()));
                sb.Clear();
            }
        }

        // Structural characters
        switch (c)
        {
            case '{':
                onToken(new JsonToken(JsonTokenType.StartObject, "{"));
                stack.Push(ContainerType.Object);
                isExpectingKey = true;
                break;
            case '}':
                if (stack.Count > 0 && stack.Peek() == ContainerType.Object)
                {
                    stack.Pop();
                }
                onToken(new JsonToken(JsonTokenType.EndObject, "}"));
                isExpectingKey = stack.Count > 0 && stack.Peek() == ContainerType.Object;
                break;
            case '[':
                onToken(new JsonToken(JsonTokenType.StartArray, "["));
                stack.Push(ContainerType.Array);
                isExpectingKey = false;
                break;
            case ']':
                if (stack.Count > 0 && stack.Peek() == ContainerType.Array)
                {
                    stack.Pop();
                }
                onToken(new JsonToken(JsonTokenType.EndArray, "]"));
                isExpectingKey = stack.Count > 0 && stack.Peek() == ContainerType.Object;
                break;
            case ':':
                onToken(new JsonToken(JsonTokenType.Colon, ":"));
                isExpectingKey = false;
                break;
            case ',':
                onToken(new JsonToken(JsonTokenType.Comma, ","));
                isExpectingKey = stack.Count > 0 && stack.Peek() == ContainerType.Object;
                break;
            case '"':
                inString = true;
                sb.Append(c);
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
                sb.Append(c);
                break;
            case 't':
            case 'f':
            case 'n':
                inKeyword = true;
                sb.Append(c);
                break;
            default:
                // Skip invalid characters
                break;
        }
    }

    private static void EmitPending(StringBuilder sb, ref bool inString, ref bool inNumber, ref bool inKeyword, Action<JsonToken> onToken)
    {
        if (sb.Length > 0)
        {
            if (inString)
            {
                // Incomplete string, skip or handle
            }
            else if (inNumber)
            {
                onToken(new JsonToken(JsonTokenType.Number, sb.ToString()));
            }
            else if (inKeyword)
            {
                // Incomplete, skip
            }
            else
            {
                onToken(new JsonToken(JsonTokenType.Whitespace, sb.ToString()));
            }
            sb.Clear();
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