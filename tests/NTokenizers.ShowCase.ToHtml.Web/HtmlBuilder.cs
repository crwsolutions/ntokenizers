namespace NTokenizers.ShowCase.ToHtml.Web;

internal static class HtmlBuilder
{
    public static string GenerateIndexHtml(string injectedCss)
    {
        var html = @"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>NTokenizers — Streaming HTML Showcase</title>
    <style>" + injectedCss + @"
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 2rem;
        }
        .container {
            max-width: 1400px;
            margin: 0 auto;
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 2rem;
            height: calc(100vh - 4rem);
        }
        .panel {
            background: white;
            border-radius: 12px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            display: flex;
            flex-direction: column;
            overflow: hidden;
        }
        .panel-header {
            background: #f8f9fa;
            padding: 1.5rem;
            border-bottom: 1px solid #e9ecef;
        }
        .panel-header h2 {
            font-size: 1.25rem;
            color: #333;
            margin-bottom: 0.5rem;
        }
        .panel-header p {
            font-size: 0.875rem;
            color: #666;
        }
        .panel-content {
            flex: 1;
            overflow: auto;
            padding: 1.5rem;
        }
        textarea {
            width: 100%;
            height: 100%;
            border: none;
            outline: none;
            font-family: 'Courier New', monospace;
            font-size: 0.875rem;
            line-height: 1.6;
            resize: none;
            padding: 0;
        }
        .controls {
            padding: 1.5rem;
            background: #f8f9fa;
            border-top: 1px solid #e9ecef;
            display: flex;
        }
        button {
            flex: 1;
            padding: 0.75rem 1.5rem;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 6px;
            font-size: 1rem;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
        }
        button:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(102, 126, 234, 0.4);
        }
        button:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }
        #output { background: white; }
        #output-content {
            font-size: 0.95rem;
            line-height: 1.8;
            color: #333;
        }
        .streaming-indicator {
            display: none;
            padding: 1rem;
            background: #e7f3ff;
            border-left: 4px solid #667eea;
            color: #0c5aa0;
            font-size: 0.875rem;
            font-weight: 500;
            margin-bottom: 1rem;
        }
        .streaming-indicator.active { display: block; }
        .streaming-indicator.complete {
            background: #d4edda;
            border-left-color: #28a745;
            color: #155724;
        }
        pre {
            background: #f6f8fa;
            border: 1px solid #e1e4e8;
            border-radius: 6px;
            padding: 1rem;
            overflow-x: auto;
            margin: 1rem 0;
        }
        code {
            font-family: 'Courier New', monospace;
            font-size: 0.875rem;
            line-height: 1.6;
        }
        h1 { font-size: 2rem; margin-top: 1.5rem; }
        h2 { font-size: 1.5rem; margin-top: 1.5rem; }
        h3 { font-size: 1.25rem; margin-top: 1.5rem; }
        p { margin: 0.5rem 0; }
        ul, ol { margin: 0.5rem 0 0.5rem 2rem; }
        strong { font-weight: 600; color: #1a1a1a; }
        em { font-style: italic; color: #555; }
        blockquote {
            border-left: 4px solid #ddd;
            padding-left: 1rem;
            margin: 1rem 0;
            color: #666;
        }
        @media (max-width: 1024px) {
            .container { grid-template-columns: 1fr; height: auto; }
            .panel { min-height: 300px; }
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='panel'>
            <div class='panel-header'>
                <h2>Markdown Input</h2>
                <p>Edit your Markdown below or use the default example</p>
            </div>
            <div class='panel-content'>
                <textarea id='markdown-input' spellcheck='false'></textarea>
            </div>
            <div class='controls'>
                <button id='submit-btn' onclick='streamHtml()'>✨ Convert & Stream HTML</button>
            </div>
        </div>

        <div class='panel'>
            <div class='panel-header'>
                <h2>HTML Preview</h2>
                <p>Real-time streaming output</p>
            </div>
            <div id='streaming-status' class='streaming-indicator'></div>
            <iframe id='output' style='width: 100%; height: calc(100% - 60px); border: none; background: white;'></iframe>
        </div>
    </div>

    <script src='/app.js'></script>
</body>
</html>";
        return html;
    }
}
