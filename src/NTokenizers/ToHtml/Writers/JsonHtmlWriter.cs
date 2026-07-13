using NTokenizers.Json;
using System.Text;

namespace NTokenizers.ToHtml.Writers;

/// <summary>
/// Writer for JSON tokens.
/// </summary>
internal sealed class JsonHtmlWriter : AbstractTokenToHtmlWriter<JsonToken>
{
    internal override void WriteAdditionalCss(StringBuilder css)
    {
        // JSON-specific token styles
        css.AppendLine(".tok-json-property { color: #800000; }");
        css.AppendLine();
    }

    internal override void WriteHtml(JsonToken token, TextWriter writer)
    {
        switch (token.TokenType)
        {
            case JsonTokenType.StartObject:
            case JsonTokenType.EndObject:
            case JsonTokenType.StartArray:
            case JsonTokenType.EndArray:
            case JsonTokenType.Colon:
            case JsonTokenType.Comma:
                WriteValue(writer, token.Value, "tok-punctuation", inPreBlock: true);
                break;

            case JsonTokenType.PropertyName:
                WriteValue(writer, token.Value, "tok-json-property", inPreBlock: true);
                break;

            case JsonTokenType.StringValue:
                WriteValue(writer, token.Value, "tok-string", inPreBlock: true);
                break;

            case JsonTokenType.Number:
                WriteValue(writer, token.Value, "tok-number", inPreBlock: true);
                break;

            case JsonTokenType.True:
            case JsonTokenType.False:
                WriteValue(writer, token.Value, "tok-boolean", inPreBlock: true);
                break;

            case JsonTokenType.Null:
                WriteValue(writer, token.Value, "tok-null", inPreBlock: true);
                break;

            case JsonTokenType.Whitespace:
                writer.Write(token.Value);
                break;

            default:
                WriteValue(writer, token.Value, null, inPreBlock: true);
                break;
        }
    }
}
