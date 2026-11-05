using Microsoft.Extensions.AI;
using NTokenizers.ToHtml;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;

namespace NTokenizers.ShowCase.ToHtml.AiChat;

public static class AiChatEndpoints
{
    public static IEndpointRouteBuilder MapAiChatEndpoints(this IEndpointRouteBuilder endpoints, string injectedCss)
    {
        endpoints.MapGet("/", (HttpContext context) =>
        {
            var html = HtmlBuilder.GenerateIndexHtml(injectedCss);
            context.Response.ContentType = "text/html; charset=utf-8";
            return Results.Text(html);
        });

        endpoints.MapPost("/render-markdown", async (HttpContext context) =>
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            try
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                var request = JsonSerializer.Deserialize<RenderRequestDto>(body) ??
                    throw new ArgumentException("Invalid request");

                using var markdownStream = new MemoryStream(Encoding.UTF8.GetBytes(request.Content));
                using var writer = new StringWriter();
                await MarkdownConverter.WriteHtmlAsync(markdownStream, writer);

                var response = new { Html = writer.ToString() };
                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in /render-markdown: {ex.Message}");
                throw;
            }
        });

        endpoints.MapPost("/stream-chat", async (HttpContext context, IChatClient chatClient) =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";

            try
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                var request = JsonSerializer.Deserialize<ChatRequestDto>(body) ??
                    throw new ArgumentException("Invalid request");

                // Streaming writer that flushes every write for real-time streaming
                using var streamingWriter = new FlushStreamWriter(context.Response.Body);

                // Convert AI chunks to a Stream via Pipe, then feed to MarkdownConverter
                var pipe = new Pipe();
                var aiStreamTask = Task.Run(() => FeedAiChunks(pipe, chatClient, request.Messages));
                var aiStream = pipe.Reader.AsStream();

                // Stream the AI response through ToHtml — real-time, no delays
                var markdown = await MarkdownConverter.WriteHtmlAsync(aiStream, streamingWriter);
                await aiStreamTask;

                // Send the consumed Markdown as an inert JSON trailer on the same response,
                // so the client can store the real assistant reply in its chat history.
                // The \\/ replacement keeps a literal </script> in the content from terminating the tag.
                var payload = JsonSerializer.Serialize(markdown).Replace("</", "<\\/");
                await streamingWriter.WriteAsync($"<script id=\"assistant-markdown\" type=\"application/json\">{payload}</script>");
            }
            catch (Exception ex)
            {
                // Response may have already started, so we cannot set status code
                // Just log the error
                Console.WriteLine($"Error in /stream-chat: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        });

        return endpoints;
    }

    private static async Task FeedAiChunks(Pipe pipe, IChatClient chatClient, List<ChatMessageDto> messages)
    {
        var chatMessages = messages.Select(m => new ChatMessage(
            m.Role == "user" ? ChatRole.User : ChatRole.Assistant,
            m.Content)).ToList();

        var stream = chatClient.GetStreamingResponseAsync(chatMessages, null);
        await foreach (var chunk in stream)
        {
            if (!string.IsNullOrEmpty(chunk.Text))
            {
                var bytes = Encoding.UTF8.GetBytes(chunk.Text);
                await pipe.Writer.WriteAsync(bytes);
            }
        }
        await pipe.Writer.CompleteAsync();
    }
}
