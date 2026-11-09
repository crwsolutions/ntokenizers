using System.ClientModel;
using Microsoft.Extensions.AI;
using NTokenizers.ShowCase.ToHtml.AiChat;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MinRequestBodyDataRate = null;
    serverOptions.Limits.MinResponseDataRate = null;
});

// Configure ChatClient via DI with hardcoded endpoint and model
var endpoint = "http://localhost:8080/";
var modelId = "dummy";
var key = "dummy"; // llama.cpp requires one, value ignored

builder.Services.AddChatClient(new OpenAIClient(
    new ApiKeyCredential(key),
    new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
    .GetChatClient(modelId)
    .AsIChatClient()
    .AsBuilder()
    .UseFunctionInvocation()
    .Build());

var app = builder.Build();
app.MapAiChatEndpoints();
app.Run();
