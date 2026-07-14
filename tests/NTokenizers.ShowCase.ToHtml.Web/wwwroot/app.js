const DEFAULT_MARKDOWN = `- Item 1
  * Nested item
- Item 2

Here is some **bold** text and some *italic* text.

Star: \\* and Underscore: \\_

Here is some larger text: Lorem ipsum dolor sit amet, consectetur adipiscing elit.

# NTokenizers Showcase

## Code Examples

### JSON
\`\`\`json
{
  "name": "Laura Smith",
  "active": true
}
\`\`\`

### Rust
\`\`\`rust
let numbers = vec![1, 2, 3];
for n in &numbers {
    println!("{}", n);
}
\`\`\`

### TypeScript
\`\`\`typescript
const user = {
  name: "Laura Smith",
  active: true
};
\`\`\`

### Python
\`\`\`python
def greet(name: str) -> str:
    return f"Hello, {name}!"
\`\`\`

### C
\`\`\`c
int main(void) {
    printf("Hello, World!\\n");
    return 0;
}
\`\`\``;

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('markdown-input').value = DEFAULT_MARKDOWN;
});

async function streamHtml() {
    const markdown = document.getElementById('markdown-input').value;
    const statusIndicator = document.getElementById('streaming-status');
    const submitBtn = document.getElementById('submit-btn');
    const iframe = document.getElementById('output');

    statusIndicator.className = 'streaming-indicator active';
    statusIndicator.textContent = '⏳ Streaming HTML...';
    submitBtn.disabled = true;

    try {
        const response = await fetch('/stream-html', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Markdown: markdown })
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        // Get the iframe's document
        const iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
        iframeDoc.open();

        const reader = response.body.getReader();
        const decoder = new TextDecoder();

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            const chunk = decoder.decode(value, { stream: true });
            iframeDoc.write(chunk);
        }

        iframeDoc.close();

        statusIndicator.className = 'streaming-indicator complete';
        statusIndicator.textContent = '✅ Streaming complete!';
        setTimeout(() => {
            statusIndicator.className = 'streaming-indicator';
        }, 3000);

    } catch (error) {
        statusIndicator.className = 'streaming-indicator';
        const iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
        iframeDoc.open();
        iframeDoc.write(`<div style='color: #d32f2f; padding: 1rem; background: #ffebee; border-radius: 4px;'><strong>Error:</strong> ${error.message}</div>`);
        iframeDoc.close();
        console.error('Streaming error:', error);
    } finally {
        submitBtn.disabled = false;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('markdown-input').addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && e.ctrlKey) {
            streamHtml();
        }
    });
});
