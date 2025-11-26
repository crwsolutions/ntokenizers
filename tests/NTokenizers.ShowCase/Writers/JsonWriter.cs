using NTokenizers.Json;
using Spectre.Console;

namespace NTokenizers.ShowCase.Writers;

internal sealed class JsonWriter : BaseInlineWriter<JsonToken, JsonTokenType>
{
    protected override Style GetStyle(JsonTokenType token) => token switch
    {
        JsonTokenType.StartObject => new Style(Color.DeepSkyBlue4_1),
        JsonTokenType.EndObject => new Style(Color.DeepSkyBlue4_1),
        JsonTokenType.StartArray => new Style(Color.DeepSkyBlue4_1),
        JsonTokenType.EndArray => new Style(Color.DeepSkyBlue4_1),
        JsonTokenType.PropertyName => new Style(Color.DeepSkyBlue3_1),
        JsonTokenType.Colon => new Style(Color.Yellow),
        JsonTokenType.Comma => new Style(Color.Yellow),
        JsonTokenType.StringValue => new Style(Color.DarkSlateGray1),
        JsonTokenType.Number => new Style(Color.Blue),
        JsonTokenType.True => new Style(Color.Blue),
        JsonTokenType.False => new Style(Color.Blue),
        JsonTokenType.Null => new Style(Color.Blue),
        _ => new Style()
    };
}
