namespace NTokenizers.ShowCase.ToHtml.AiChat;

internal static class HtmlBuilder
{
    public static string GenerateIndexHtml(string injectedCss)
    {
        var html = @"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>CRW | NTokenizers | AI Chat Showcase</title>
    <style>" + injectedCss + @"
        * { margin: 0; padding: 0; box-sizing: border-box; }
        html, body {
            min-height: 100%;
        }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: #f6f9f5;
            display: flex;
            flex-direction: column;
        }
        .header {
            background: #2E803E;
            color: white;
            padding: 1.25rem 1.5rem;
            /* text-align: center; */
            font-family: Calibri, sans-serif;
            //font-weight: 600;
            font-size: 2rem;
            //box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .chat-container {
            flex: 1;
            padding: 1.5rem;
            padding-bottom: 120px;
            position: relative;
            display: flex;
            flex-direction: column;
            gap: 1rem;
            max-width: 900px;
            width: 100%;
            margin: 0 auto;
            overflow-y: auto;
        }
        .welcome-message {
            flex: 1;
            display: flex;
            align-items: center;
            justify-content: center;
            text-align: center;
        }
        body.initial-state .welcome-message {
            position: absolute;
            top: 30%;
            left: 0;
            right: 0;
        }
        .welcome-message h2 {
            color: #224529;
            font-size: 1.5rem;
        }
        .message {
            max-width: 80%;
            padding: 0.875rem 1.25rem;
            border-radius: 16px;
            font-size: 0.95rem;
            line-height: 1.6;
            word-wrap: break-word;
            overflow-wrap: break-word;
        }
        .message.user {
            align-self: flex-end;
            background: #2E803E;
            color: white;
            border-bottom-right-radius: 4px;
        }
        .message.assistant {
            align-self: flex-start;
            background: white;
            color: #0D1B0B;
            border-bottom-left-radius: 4px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }
        .message p { margin: 0.4rem 0; }
        .message p:first-child { margin-top: 0; }
        .message p:last-child { margin-bottom: 0; }
        .message ul, .message ol {
            margin: 0.4rem 0;
            padding-left: 1.5rem;
        }
        .message li {
            margin: 0.2rem 0;
        }
        /* Code block container from ToHtml */
        .input-container {
            position: fixed;
            bottom: 1.5rem;
            left: 50%;
            transform: translateX(-50%);
            background: white;
            border-top: 1px solid #e0e0e0;
            border-radius: 16px;
            padding: 1rem 1.5rem;
            display: flex;
            gap: 0.75rem;
            max-width: 900px;
            width: 100%;
            box-shadow: 0 -2px 10px rgba(0,0,0,0.05);
        }
        .input-container.centered {
            position: absolute;
            bottom: 45%;
            left: 50%;
            transform: translateX(-50%);
            box-shadow: 0 4px 20px rgba(0,0,0,0.1);
        }
        textarea {
            flex: 1;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 0.75rem 1rem;
            font-size: 0.95rem;
            font-family: inherit;
            resize: none;
            outline: none;
            transition: border-color 0.2s;
        }
        textarea:focus {
            border-color: #2E803E;
        }
        button {
            padding: 0.75rem 1.5rem;
            background: #2E803E;
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 1rem;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
        }
        button:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(46, 128, 62, 0.4);
        }
        button:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }
        @media (max-width: 768px) {
            .chat-container { padding: 1rem; }
            .message { max-width: 90%; }
        }
        .typing-indicator {
            color: #2E803E;
            font-size: 1.5rem;
            animation: pulse 1s infinite;
        }
        @keyframes pulse {
            0%, 100% { opacity: 0.2; }
            50% { opacity: 1; }
        }
    </style>
</head>
<body>
    <div class='header'>
        <strong>CRW</strong> | NTokenizers | AI Chat Showcase
    </div>
    <div class='chat-container' id='chat'>
        <div class='welcome-message' id='welcome'>
            <h2>What are we going to do today?</h2>
        </div>
        <!-- Messages will be appended here -->
    </div>
    <div class='input-container'>
        <textarea id='chat-input' rows='1' placeholder='Type your message...'></textarea>
        <button id='send-btn' onclick='sendChat()'>Send</button>
    </div>

    <script src='/app.js'></script>
</body>
</html>";
        return html;
    }
}
