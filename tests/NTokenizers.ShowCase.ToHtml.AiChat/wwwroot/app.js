let chatHistory = [];
let isStreaming = false;

async function renderMarkdown(content) {
    const response = await fetch('/render-markdown', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Content: content })
    });
    const result = await response.json();
    return result.Html;
}

function addMessage(role, html) {
    // Remove welcome message on first chat message
    const welcome = document.getElementById('welcome');
    if (welcome) {
        welcome.remove();
    }

    // Un-center input box on first message
    const inputContainer = document.querySelector('.input-container');
    if (inputContainer && inputContainer.classList.contains('centered')) {
        inputContainer.classList.remove('centered');
    }
    // Reset initial state on first message
    document.body.classList.remove('initial-state');

    const chat = document.getElementById('chat');
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${role}`;
    if (html) {
        messageDiv.innerHTML = html;
    }
    chat.appendChild(messageDiv);
    scrollToBottom();
    return messageDiv;
}

function scrollToBottom() {
    window.scrollTo(0, document.documentElement.scrollHeight);
}

function showTypingIndicator() {
    const indicator = addMessage('assistant', '<span class="typing-indicator">...</span>');
    indicator.id = 'typing-indicator';
    return indicator;
}

function removeTypingIndicator() {
    const indicator = document.getElementById('typing-indicator');
    if (indicator) {
        indicator.remove();
    }
}

async function sendChat() {
    const input = document.getElementById('chat-input');
    const prompt = input.value.trim();
    if (!prompt || isStreaming) return;

    isStreaming = true;

    chatHistory.push({ Role: 'user', Content: prompt });
    input.value = '';

    const sendBtn = document.getElementById('send-btn');
    sendBtn.disabled = true;

    try {
        // Render user bubble immediately
        const userHtml = await renderMarkdown(prompt);
        addMessage('user', userHtml);

        // Show typing indicator
        showTypingIndicator();

        // Stream AI response
        const response = await fetch('/stream-chat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Messages: chatHistory })
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        // Remove typing indicator and create assistant bubble
        removeTypingIndicator();
        const assistantMsg = addMessage('assistant', '');

        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let accumulated = '';

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            const chunk = decoder.decode(value, { stream: true });
            accumulated += chunk;
            // Update bubble with accumulated HTML — browser parses complete HTML each time
            assistantMsg.innerHTML = accumulated;
            scrollToBottom();
        }

        chatHistory.push({ Role: 'assistant', Content: '' });
    } catch (error) {
        console.error('Error streaming chat:', error);
        removeTypingIndicator();
        addMessage('assistant', `<p>Error: ${error.message}</p>`);
    } finally {
        isStreaming = false;
        sendBtn.disabled = false;
    }
}

function resetChat() {
    chatHistory = [];
    document.getElementById('chat-input').value = '';
    document.getElementById('chat').innerHTML = '';
}

// Center input box on page load
document.addEventListener('DOMContentLoaded', () => {
    document.body.classList.add('initial-state');
    const inputContainer = document.querySelector('.input-container');
    inputContainer.classList.add('centered');
});

// Handle Enter key in textarea (without Shift)
document.getElementById('chat-input').addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        sendChat();
    }
});
