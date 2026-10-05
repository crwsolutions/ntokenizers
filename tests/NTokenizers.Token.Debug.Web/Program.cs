using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using NTokenizers.Token.Debug.Web;
using NTokenizers.ToHtml;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var injectedCss = MarkdownConverter.GetCss();

app.MapGet("/", () => Results.Text(HtmlBuilder.GenerateIndexHtml(injectedCss), "text/html; charset=utf-8"));

app.MapPost("/convert", async (ConvertRequest request, HttpContext context) =>
{
    var markdown = request?.Markdown ?? string.Empty;

    var tokens = await TokenCapture.CaptureAsync(markdown, context.RequestAborted);
    var htmlRaw = MarkdownConverter.ToHtml(markdown);

    var response = new ConvertResponse
    {
        Tokens = tokens,
        HtmlRaw = htmlRaw,
    };

    context.Response.ContentType = "application/json; charset=utf-8";
    await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions.Options));
});

app.Run();

/// <summary>
/// Request body of the /convert endpoint.
/// </summary>
public sealed class ConvertRequest
{
    public string? Markdown { get; set; }
}

/// <summary>
/// Response body of the /convert endpoint.
/// </summary>
public sealed class ConvertResponse
{
    public List<TokenEntry> Tokens { get; set; } = [];
    public string HtmlRaw { get; set; } = string.Empty;
}

/// <summary>
/// Shared JSON serializer options for the /convert endpoint.
/// </summary>
internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        IncludeFields = true,
    };
}
