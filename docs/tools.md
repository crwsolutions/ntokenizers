---
layout: default
title: "Tools"
---

# Tools

The repository ships a collection of sample applications and tools that showcase the library in action:

- **[NTokenizers.ShowCase.ToHtml.AiChat](https://github.com/crwsolutions/ntokenizers/tree/main/tests/NTokenizers.ShowCase.ToHtml.AiChat)** — Streaming AI chat showcase (web)
- **[NTokenizers.ShowCase.ToHtml.Web](https://github.com/crwsolutions/ntokenizers/tree/main/tests/NTokenizers.ShowCase.ToHtml.Web)** — Streaming Markdown to HTML showcase (web)
- **[NTokenizers.Token.Debug.Web](https://github.com/crwsolutions/ntokenizers/tree/main/tests/NTokenizers.Token.Debug.Web)** — Web app with four side-by-side columns — Input, Tokens, ToHtml raw and ToHtml rendered — that shows the token stream of a Markdown document in real time.
- **[NTokenizers.Tools.MarkdownToHtml](https://github.com/crwsolutions/ntokenizers/tree/main/tools/NTokenizers.Tools.MarkdownToHtml)** — CLI tool that reads a Markdown file as a stream and writes a styled HTML file, with encoding detection and timing output.
- **[NTokenizers.Tools.MarkdownViewer](https://github.com/crwsolutions/ntokenizers/tree/main/tools/NTokenizers.Tools.MarkdownViewer)** — Windows WinUI 3 desktop app for reading `.md` files with ToHtml rendering.

## NTokenizers.ShowCase.ToHtml.AiChat

A web app that shows the [ToHtml API](/ntokenizers/tohtml) in a live AI chat: the response of an AI model is streamed through `MarkdownConverter.WriteHtmlAsync` and rendered in the browser as the tokens arrive — no buffering of the full response.

The app talks to any OpenAI-compatible endpoint through `Microsoft.Extensions.AI`. Out of the box it points at a local [llama.cpp](https://github.com/ggml-org/llama.cpp) server:

```
http://localhost:8080/unsloth/Qwen3-Coder-30B-A3B-Instruct-GGUF:Q6_K
```

```powershell
dotnet run --project tests/NTokenizers.ShowCase.ToHtml.AiChat
```

![NTokenizers.ShowCase.ToHtml.AiChat](/assets/NTokenizers.ShowCase.ToHtml.AiChat.png)

## NTokenizers.ShowCase.ToHtml.Web

A streaming Markdown showcase: edit Markdown in one pane and press Convert — the HTML fragment is streamed back and rendered live in the other pane, showing the ToHtml API in action.

```powershell
dotnet run --project tests/NTokenizers.ShowCase.ToHtml.Web
```

![NTokenizers.ShowCase.ToHtml.Web](/assets/NTokenizers.ShowCase.ToHtml.Web.png)

## NTokenizers.Token.Debug.Web

A web app with four side-by-side columns — Input, Tokens, ToHtml raw and ToHtml rendered — that shows the token stream of a Markdown document in real time.

```powershell
dotnet run --project tests/NTokenizers.Token.Debug.Web
```

![NTokenizers.Token.Debug.Web](/assets/NTokenizers.Token.Debug.Web.png)

## NTokenizers.Tools.MarkdownToHtml

A CLI tool that reads a Markdown file as a stream and writes a styled HTML file, with encoding detection and timing output.

```powershell
dotnet run --project tools/NTokenizers.Tools.MarkdownToHtml -- input.md output.html
```

![NTokenizers.Tools.MarkdownToHtml](/assets/NTokenizers.Tools.MarkdownToHtml.png)

## NTokenizers.Tools.MarkdownViewer

A Windows WinUI 3 desktop app for reading `.md` files with ToHtml rendering.

```powershell
dotnet run --project tools/NTokenizers.Tools.MarkdownViewer
```

![NTokenizers.Tools.MarkdownViewer](/assets/NTokenizers.Tools.MarkdownViewer.png)
