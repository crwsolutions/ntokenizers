namespace NTokenizers.Token.Debug.Web;

/// <summary>
/// Builds the single page of the token debug web app: a 4 column grid with
/// Input, Tokens, ToHtml raw and ToHtml rendered.
/// </summary>
internal static class HtmlBuilder
{
    private const string SampleMarkdown =
        """
        # Token Debug

        This is a **bold** and *italic* paragraph with a [link](https://example.com "title").

        - first item
        - second item

        1. one
        2. two

        > A quoted paragraph with `inline code`.

        ```csharp
        var number = 42;
        // line comment
        Console.WriteLine("hello {0}", number);
        ```

        | Name | Value |
        |------|-------|
        | a    | 1     |
        | b    | 2     |

        ```wat
        this is an unknown language
        ```
        """;

    public static string GenerateIndexHtml(string injectedCss) =>
        PageTemplate
            .Replace("__INJECTED_CSS__", injectedCss)
            .Replace("__SAMPLE_MARKDOWN__", SampleMarkdown);

    private const string PageTemplate =
        """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8" />
            <title>NTokenizers Token Debug</title>
            <script type="text/plain" id="tohtml-css">__INJECTED_CSS__</script>
            <style>
                * { box-sizing: border-box; margin: 0; padding: 0; }
                html, body { height: 100%; }
                body { background: #0d1117; color: #c9d1d9; font-family: 'Segoe UI', sans-serif; }

                .grid {
                    display: grid;
                    grid-template-columns: 1fr 0.5fr 1fr 1.5fr;
                    grid-template-rows: auto 1fr;
                    gap: 6px;
                    height: 100vh;
                    padding: 6px;
                }
                .grid > * { min-width: 0; min-height: 0; }

                .col-header {
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                    background: #161b22;
                    border: 1px solid #30363d;
                    border-bottom: none;
                    padding: 8px 12px;
                    font-size: 12px;
                    font-weight: 600;
                    text-transform: uppercase;
                    letter-spacing: 0.05em;
                    color: #8b949e;
                }
                .copy-btn {
                    display: inline-flex;
                    align-items: center;
                    background: none;
                    border: none;
                    padding: 2px;
                    margin: 0;
                    cursor: pointer;
                    color: #8b949e;
                    opacity: 0;
                    transition: opacity 0.15s ease, color 0.15s ease;
                }
                .col-header:hover .copy-btn { opacity: 1; }
                .copy-btn:hover { color: #c9d1d9; }
                .copy-btn:active { color: #58a6ff; }
                .copy-btn svg { display: block; }
                .copy-btn svg path { fill: currentColor; }
                .col {
                    background: #0d1117;
                    border: 1px solid #30363d;
                    overflow: auto;
                    padding: 10px;
                }
                .col-rendered { position: relative; padding: 0; overflow: hidden; }
                #rendered {
                    position: absolute;
                    inset: 0;
                    width: 100%;
                    height: 100%;
                    border: 0;
                    background: #ffffff;
                }

                .col-input { display: flex; flex-direction: column; overflow: hidden; padding: 0; }
                #input {
                    flex: 1;
                    width: 100%;
                    background: #0d1117;
                    color: #c9d1d9;
                    border: none;
                    outline: none;
                    resize: none;
                    padding: 10px;
                    font-family: Consolas, monospace;
                    font-size: 13px;
                    line-height: 1.5;
                }
                .input-footer {
                    flex: none;
                    display: flex;
                    gap: 8px;
                    align-items: center;
                    padding: 8px;
                    border-top: 1px solid #30363d;
                    background: #161b22;
                }
                #convert {
                    flex: none;
                    background: #238636;
                    color: #ffffff;
                    border: none;
                    padding: 7px 14px;
                    border-radius: 4px;
                    font-size: 13px;
                    font-weight: 600;
                    cursor: pointer;
                }
                #convert:disabled { opacity: 0.5; cursor: default; }
                #status { font-size: 12px; color: #8b949e; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

                #html-raw {
                    white-space: pre-wrap;
                    word-break: break-word;
                    font-family: Consolas, monospace;
                    font-size: 12px;
                    line-height: 1.5;
                    color: #c9d1d9;
                }
                /* Token colors for the ToHtml raw column (HtmlTokenizer output). */
                #html-raw .raw-elementname { color: #7ee787; }
                #html-raw .raw-attributename { color: #79c0ff; }
                #html-raw .raw-attributevalue { color: #ffa657; }
                #html-raw .raw-attributequote { color: #d29922; }
                #html-raw .raw-attributeequals { color: #8b949e; }
                #html-raw .raw-openinganglebracket, #html-raw .raw-closinganglebracket, #html-raw .raw-selfclosingslash { color: #8b949e; }
                #html-raw .raw-comment { color: #8b949e; font-style: italic; }
                #html-raw .raw-documenttypedeclaration { color: #ff7b72; }
                #html-raw .raw-scriptelement, #html-raw .raw-styleelement { color: #ff7b72; }
                #html-raw .raw-whitespace { color: #6e7681; }
                #html-raw .raw-text { color: #c9d1d9; }
                #html-raw .raw-keyword { color: #ff7b72; }
                #html-raw .raw-string { color: #a5d6ff; }
                #html-raw .raw-number { color: #ffa657; }
                #html-raw .raw-operator, #html-raw .raw-punctuation { color: #8b949e; }

                #tokens, #tokens ul { list-style: none; }
                #tokens ul ul { margin-left: 2px; padding-left: 14px; border-left: 1px solid #2d3340; }
                #tokens li { padding: 1px 0; font-family: Consolas, monospace; font-size: 12px; line-height: 1.5; }
                .tok-type { color: #ff7b72; font-weight: 600; }
                .tok-value { color: #a5d6ff; }
                .tok-meta { color: #8b949e; }
            </style>
        </head>
        <body>
            <div class="grid">
                <div class="col-header">
                    <span>Input</span>
                    <button class="copy-btn" data-copy-target="input" title="Copy input to clipboard" type="button">
                        <svg aria-hidden="true" height="16" viewBox="0 0 16 16" width="16"><path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"></path><path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"></path></svg>
                    </button>
                </div>
                <div class="col-header">
                    <span>Tokens</span>
                    <button class="copy-btn" data-copy-target="tokens" title="Copy tokens to clipboard" type="button">
                        <svg aria-hidden="true" height="16" viewBox="0 0 16 16" width="16"><path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"></path><path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"></path></svg>
                    </button>
                </div>
                <div class="col-header">
                    <span>ToHtml (raw)</span>
                    <button class="copy-btn" data-copy-target="html-raw" title="Copy raw HTML to clipboard" type="button">
                        <svg aria-hidden="true" height="16" viewBox="0 0 16 16" width="16"><path d="M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z"></path><path d="M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z"></path></svg>
                    </button>
                </div>
                <div class="col-header"><span>ToHtml (rendered)</span></div>

                <div class="col col-input">
                    <textarea id="input" spellcheck="false">__SAMPLE_MARKDOWN__</textarea>
                    <div class="input-footer">
                        <button id="convert">Convert</button>
                        <span id="status">Enter or Convert to tokenize</span>
                    </div>
                </div>

                <div class="col" id="tokens-col"><div id="tokens"></div></div>
                <div class="col"><pre id="html-raw"></pre></div>
                <div class="col col-rendered"><iframe id="rendered"></iframe></div>
            </div>

            <script>
                const input = document.getElementById('input');
                const convertButton = document.getElementById('convert');
                const statusEl = document.getElementById('status');
                const tokensEl = document.getElementById('tokens');
                const rawEl = document.getElementById('html-raw');
                const renderedEl = document.getElementById('rendered');
                const tohtmlCss = document.getElementById('tohtml-css').textContent;

                const copyCodeScript =
                    'function copyCode(button) {' +
                    "  var container = button.closest('.code-block-container');" +
                    "  var code = container.querySelector('code');" +
                    "  var text = code.textContent || code.innerText;" +
                    "  navigator.clipboard.writeText(text.trim()).then(function() {" +
                    "    button.textContent = 'Copied!';" +
                    "    button.classList.add('code-block-copied');" +
                    "    setTimeout(function() {" +
                    "      button.textContent = 'Copy';" +
                    "      button.classList.remove('code-block-copied');" +
                    '    }, 1500);' +
                    '  });' +
                    '}';

                let busy = false;

                convertButton.addEventListener('click', convert);
                attachCopyButtons();
                input.addEventListener('keydown', function (event) {
                    // Enter converts; Shift+Enter inserts a newline.
                    if (event.key === 'Enter' && !event.shiftKey) {
                        event.preventDefault();
                        convert();
                    }
                });

                async function convert() {
                    if (busy) return;
                    busy = true;
                    convertButton.disabled = true;
                    setStatus('Converting...');
                    try {
                        const response = await fetch('/convert', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ markdown: input.value })
                        });
                        if (!response.ok) {
                            throw new Error('HTTP ' + response.status);
                        }
                        const result = await response.json();
                        tokensEl.innerHTML = renderTokenTree(result.tokens);
                        if (result.htmlTokens && result.htmlTokens.length > 0) {
                            rawEl.textContent = '';
                            rawEl.appendChild(renderRawHtml(result.htmlTokens));
                        } else {
                            rawEl.textContent = result.htmlRaw;
                        }
                        renderedEl.srcdoc = buildRenderedDocument(result.htmlRaw);
                        setStatus(countTokens(result.tokens) + ' token(s) captured');
                    } catch (error) {
                        setStatus('Error: ' + error.message);
                    } finally {
                        busy = false;
                        convertButton.disabled = false;
                    }
                }

                // Copies the content of one of the first three columns to the clipboard.
                // The token tree is serialized as plain text (type + formatted value + metadata,
                // nested children indented) so the copy is readable and round-trips visually.
                function attachCopyButtons() {
                    const copyButtons = document.querySelectorAll('.copy-btn');
                    for (const button of copyButtons) {
                        button.addEventListener('click', async function () {
                            const text = getColumnText(button.getAttribute('data-copy-target'));
                            try {
                                await navigator.clipboard.writeText(text);
                                const svg = button.querySelector('svg');
                                const previous = svg.outerHTML;
                                button.innerHTML =
                                    '<svg aria-hidden="true" height="16" viewBox="0 0 16 16" width="16" class="copy-check">' +
                                    '<path fill="#58a6ff" d="M13.78 4.22a.75.75 0 0 1 0 1.06l-7.25 7.25a.75.75 0 0 1-1.06 0L2.22 9.28a.751.751 0 0 1 .018-1.042.751.751 0 0 1 1.042-.018L6 10.94l6.72-6.72a.75.75 0 0 1 1.06 0Z"></svg>';
                                setTimeout(function () {
                                    button.innerHTML = previous;
                                }, 1500);
                            } catch (error) {
                                console.error('Copy failed:', error);
                            }
                        });
                    }
                }

                function getColumnText(id) {
                    const el = document.getElementById(id);
                    if (!el) return '';
                    if (id === 'input') {
                        return el.value || '';
                    }
                    if (id === 'tokens') {
                        return renderTokensToText(el);
                    }
                    return el.innerText || '';
                }

                // Walks the #tokens <ul> tree and produces an indented plain-text
                // representation so nested block tokens stay attached to their block
                // when copied (e.g. for pasting back to debug a problem).
                function renderTokensToText(root) {
                    const topUl = root.querySelector(':scope > ul');
                    if (!topUl) return '';
                    const lines = [];
                    (function walk(ul, depth) {
                        for (const li of ul.children) {
                            if (li.tagName !== 'LI') continue;
                            const indent = '  '.repeat(depth);
                            const typeSpan = li.querySelector(':scope > .tok-type');
                            const valueSpan = li.querySelector(':scope > .tok-value');
                            const metaSpan = li.querySelector(':scope > .tok-meta');
                            const parts = [];
                            if (typeSpan) parts.push(typeSpan.textContent);
                            if (valueSpan) parts.push(valueSpan.textContent);
                            if (metaSpan) parts.push(metaSpan.textContent.trim());
                            lines.push(indent + parts.join(' '));
                            const childUl = li.querySelector(':scope > ul');
                            if (childUl) walk(childUl, depth + 1);
                        }
                    })(topUl, 0);
                    return lines.join('\n');
                }

                function countTokens(tokens) {
                    let total = 0;
                    for (const token of tokens) {
                        total += 1;
                        if (token.children && token.children.length > 0) {
                            total += countTokens(token.children);
                        }
                    }
                    return total;
                }

                function setStatus(text) {
                    statusEl.textContent = text;
                }

                function renderTokenTree(tokens) {
                    const list = document.createElement('ul');
                    for (const token of tokens) {
                        list.appendChild(renderTokenNode(token));
                    }
                    return list.outerHTML;
                }

                // Renders the ToHtml fragment tokens (HtmlTokenizer output) as colored spans.
                // Nested script/style tokens are inlined, keeping the raw output readable.
                function renderRawHtml(tokens) {
                    const frag = document.createDocumentFragment();
                    (function walk(list) {
                        for (const token of list) {
                            const span = document.createElement('span');
                            span.className = 'raw-' + token.type.toLowerCase();
                            span.textContent = token.value;
                            frag.appendChild(span);
                            if (token.children && token.children.length > 0) {
                                walk(token.children);
                            }
                        }
                    })(tokens);
                    return frag;
                }

                function renderTokenNode(token) {
                    const li = document.createElement('li');

                    const type = document.createElement('span');
                    type.className = 'tok-type';
                    type.textContent = token.type;
                    li.appendChild(type);

                    const value = document.createElement('span');
                    value.className = 'tok-value';
                    value.textContent = formatValue(token.value);
                    li.appendChild(value);

                    if (token.metadata) {
                        const meta = document.createElement('span');
                        meta.className = 'tok-meta';
                        meta.textContent = '  ' + token.metadata;
                        li.appendChild(meta);
                    }

                    if (token.children && token.children.length > 0) {
                        const childList = document.createElement('ul');
                        for (const child of token.children) {
                            childList.appendChild(renderTokenNode(child));
                        }
                        li.appendChild(childList);
                    }
                    return li;
                }

                function formatValue(value) {
                    if (value === null || value === undefined || value === '') {
                        return "''";
                    }
                    const escaped = value.replace(/\\/g, '\\\\').replace(/\n/g, '\\n').replace(/\r/g, '\\r');
                    return "'" + escaped + "'";
                }

                function buildRenderedDocument(html) {
                    return '<!DOCTYPE html>\n' +
                        '<html><head><meta charset="utf-8">' +
                        '<style>' + tohtmlCss + '</style>' +
                        '<script>' + copyCodeScript + '</' + 'script>' +
                        '</head><body>' + html + '</' + 'body></html>';
                }
            </script>
        </body>
        </html>
        """;
}
