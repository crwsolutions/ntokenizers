using Microsoft.Extensions.AI;
using NTokenizers.ToHtml;
using NTokenizers.ShowCase.ToHtml.AiChat;
using OpenAI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MinRequestBodyDataRate = null;
    serverOptions.Limits.MinResponseDataRate = null;
    serverOptions.AllowSynchronousIO = true;
});

// Configure ChatClient via DI with hardcoded endpoint and model
var endpoint = "http://localhost:8080/";
var modelId = "unsloth/Qwen3-Coder-30B-A3B-Instruct-GGUF:Q6_K";

builder.Services.AddChatClient(new OpenAIClient(
    new ApiKeyCredential("dummy"), // llama.cpp requires one, value ignored
    new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
    .GetChatClient(modelId)
    .AsIChatClient()
    .AsBuilder()
    .UseFunctionInvocation()
    .Build());

var app = builder.Build();

// Enable static files serving
app.UseDefaultFiles();
app.UseStaticFiles();

var injectedCss = MarkdownConverter.GetCss();

app.MapAiChatEndpoints(injectedCss);

app.Run();
