using System.Text;
using NTokenizers.Core;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.Markdown;

/// <summary>
/// Abstract base class for markdown tokenizers that provides common functionality for parsing markdown text streams.
/// </summary>
public abstract class BaseMarkdownTokenizer : BaseTokenizer<MarkdownToken>
{
    internal bool TryParseBoldOrItalic()
    {
        // Check for **
        if (PeekAhead(0) == '*' && PeekAhead(1) == '*')
        {
            EmitText();
            Read();
            Read();

            // Read bold text until closing **
            var boldText = new StringBuilder();
            while (Peek() != -1)
            {
                if (PeekAhead(0) == '*' && PeekAhead(1) == '*')
                {
                    Read();
                    Read();
                    _onToken(new MarkdownToken(MarkdownTokenType.Bold, boldText.ToString()));
                    return true;
                }
                AppendEscapedChar(boldText);
            }

            // No closing found, treat as text
            _buffer.Append("**").Append(boldText);
            return true;
        }

        // Handle __bold__ syntax
        if (PeekAhead(0) == '_' && PeekAhead(1) == '_')
        {
            EmitText();
            Read();
            Read();
            var boldText = new StringBuilder();
            while (Peek() != -1)
            {
                if (PeekAhead(0) == '_' && PeekAhead(1) == '_')
                {
                    Read(); Read();
                    _onToken(new MarkdownToken(MarkdownTokenType.Bold, boldText.ToString()));
                    return true;
                }
                AppendEscapedChar(boldText);
            }
            _buffer.Append("__").Append(boldText);
            // The leading "__" was already consumed above, so the caller must not append the
            // current character again; return true so the stream is left at the correct position.
            return true;
        }

        // Check for single *
        if (PeekAhead(0) == '*')
        {
            EmitText();
            Read();

            // Read italic text until closing *
            var italicText = new StringBuilder();
            while (Peek() != -1)
            {
                if (Peek() == '*')
                {
                    Read();
                    _onToken(new MarkdownToken(MarkdownTokenType.Italic, italicText.ToString()));
                    return true;
                }
                AppendEscapedChar(italicText);
            }

            // No closing found, treat as text
            _buffer.Append('*').Append(italicText);
            return true;
        }

        // Handle _italic_ syntax
        if (PeekAhead(0) == '_' && PeekAhead(1) != '_')
        {
            EmitText();
            Read();
            var italicText = new StringBuilder();
            while (Peek() != -1 && Peek() != '_')
            {
                AppendEscapedChar(italicText);
            }
            if (Peek() == '_')
            {
                Read();
                _onToken(new MarkdownToken(MarkdownTokenType.Italic, italicText.ToString()));
                return true;
            }
            _buffer.Append('_').Append(italicText);
            // The leading "_" was already consumed above, so the caller must not append the
            // current character again; return true so the stream is left at the correct position.
            return true;
        }

        return false;
    }

    /// <summary>
    /// Reads the next character of emphasis content and appends it to the given builder,
    /// resolving backslash escapes (a backslash before an ASCII punctuation character is
    /// dropped, keeping only the punctuation character).
    /// </summary>
    private void AppendEscapedChar(StringBuilder content)
    {
        if (Peek() == '\\' && AsciiPunctuation.Contains((char)PeekAhead(1)))
        {
            char escaped = (char)PeekAhead(1);
            Read(); // Consume the backslash.
            Read(); // Consume the escaped character.
            content.Append(escaped);
            return;
        }
        content.Append((char)Read());
    }

    internal bool TryParseInlineCode()
    {
        if (Peek() != '`') return false;

        // Count the opening backtick run. The closing delimiter must be a backtick run of the
        // same length; shorter runs inside the code span are content and must not close it.
        int openLength = 0;
        while (Peek() == '`')
        {
            Read();
            openLength++;
        }

        EmitText();

        // Keep reading (including whitespace and line breaks, which are preserved) until a
        // backtick run of exactly the opening length closes the span, or end of stream.
        var code = new StringBuilder();
        var foundClose = false;
        while (Peek() != -1)
        {
            char c = (char)Read();
            if (c != '`')
            {
                code.Append(c);
                continue;
            }

            int runLength = 1;
            while (Peek() == '`')
            {
                Read();
                runLength++;
            }

            if (runLength == openLength)
            {
                foundClose = true;
                break;
            }

            code.Append('`', runLength);
        }

        if (foundClose)
        {
            _onToken(new MarkdownToken(MarkdownTokenType.CodeInline, code.ToString()));
            return true;
        }

        // No closing found, treat as text
        _buffer.Append('`', openLength).Append(code);
        return true;
    }

    internal bool TryParseLink()
    {
        if (Peek() != '[') return false;

        // Look ahead for ]( pattern
        int pos = 1;
        while (PeekAhead(pos) != '\0' && PeekAhead(pos) != '\n' && PeekAhead(pos) != ']')
            pos++;

        if (PeekAhead(pos) != ']' || PeekAhead(pos + 1) != '(')
            return false;

        EmitText();
        Read(); // Consume [

        // Read link text
        var linkText = new StringBuilder();
        while (Peek() != -1 && Peek() != ']')
        {
            linkText.Append((char)Read());
        }

        if (Peek() != ']') return false;
        Read(); // Consume ]

        if (Peek() != '(') return false;
        Read(); // Consume (

        // Read URL until unescaped ) or "
        var url = new StringBuilder();
        while (Peek() != -1)
        {
            if (Peek() == '\\' && AsciiPunctuation.Contains((char)PeekAhead(1)))
            {
                Read(); // Consume backslash
                url.Append((char)Read()); // Keep escaped char
            }
            else if (Peek() == ')' || Peek() == '"')
            {
                break;
            }
            else
            {
                url.Append((char)Read());
            }
        }

        var rawUrl = url.ToString().Trim();
        var isBracketed = rawUrl.Length >= 2 && rawUrl[0] == '<' && rawUrl[rawUrl.Length - 1] == '>';
        var urlStr = isBracketed ? rawUrl.Substring(1, rawUrl.Length - 2) : rawUrl;
        string? title = null;

        // Check for optional title
        if (Peek() == '"')
        {
            Read(); // Consume "
            var titleBuilder = new StringBuilder();
            while (Peek() != -1)
            {
                if (Peek() == '\\' && AsciiPunctuation.Contains((char)PeekAhead(1)))
                {
                    Read(); // Consume backslash
                    titleBuilder.Append((char)Read());
                }
                else if (Peek() == '"')
                {
                    Read(); // Consume closing "
                    break;
                }
                else
                {
                    titleBuilder.Append((char)Read());
                }
            }
            title = titleBuilder.ToString().Trim();

            // Skip optional whitespace between title and closing )
            while (Peek() == ' ')
                Read();
        }

        if (Peek() == ')')
            Read(); // Consume )

        var value = string.IsNullOrEmpty(title)
            ? $"[{linkText}]({rawUrl})"
            : $"[{linkText}]({rawUrl} \"{title}\")";

        _onToken(new MarkdownToken(
            MarkdownTokenType.Link,
            value,
            new LinkMetadata(urlStr, linkText.Length > 0 ? linkText.ToString().Trim() : null, title, isBracketed)
        ));

        return true;
    }

    internal bool TryParseImage()
    {
        if (PeekAhead(0) != '!' || PeekAhead(1) != '[')
            return false;

        EmitText();
        Read(); // Consume !
        Read(); // Consume [

        // Read alt text
        var altText = new StringBuilder();
        while (Peek() != -1 && Peek() != ']')
        {
            altText.Append((char)Read());
        }

        if (Peek() != ']') return false;
        Read(); // Consume ]

        if (Peek() != '(') return false;
        Read(); // Consume (

        // Read URL until unescaped ) or "
        var url = new StringBuilder();
        while (Peek() != -1)
        {
            if (Peek() == '\\' && AsciiPunctuation.Contains((char)PeekAhead(1)))
            {
                Read(); // Consume backslash
                url.Append((char)Read()); // Keep escaped char
            }
            else if (Peek() == ')' || Peek() == '"')
            {
                break;
            }
            else
            {
                url.Append((char)Read());
            }
        }

        var rawUrl = url.ToString().Trim();
        var isBracketed = rawUrl.Length >= 2 && rawUrl[0] == '<' && rawUrl[rawUrl.Length - 1] == '>';
        var urlStr = isBracketed ? rawUrl.Substring(1, rawUrl.Length - 2) : rawUrl;
        string? title = null;

        // Check for optional title
        if (Peek() == '"')
        {
            Read(); // Consume "
            var titleBuilder = new StringBuilder();
            while (Peek() != -1)
            {
                if (Peek() == '\\' && AsciiPunctuation.Contains((char)PeekAhead(1)))
                {
                    Read(); // Consume backslash
                    titleBuilder.Append((char)Read());
                }
                else if (Peek() == '"')
                {
                    Read(); // Consume closing "
                    break;
                }
                else
                {
                    titleBuilder.Append((char)Read());
                }
            }
            title = titleBuilder.ToString().Trim();

            // Skip optional whitespace between title and closing )
            while (Peek() == ' ')
                Read();
        }

        if (Peek() == ')')
            Read(); // Consume )

        var value = string.IsNullOrEmpty(title)
            ? $"![{altText}]({rawUrl})"
            : $"![{altText}]({rawUrl} \"{title}\")";

        _onToken(new MarkdownToken(
            MarkdownTokenType.Image,
            value,
            new LinkMetadata(urlStr, altText.Length > 0 ? altText.ToString().Trim() : null, title, isBracketed)
        ));

        return true;
    }

    internal bool TryParseEmoji()
    {
        if (Peek() != ':') return false;

        // Look ahead for closing :
        int pos = 1;
        while (PeekAhead(pos) != '\0' && PeekAhead(pos) != '\n' && PeekAhead(pos) != ':' && pos < 50)
        {
            if (!char.IsLetterOrDigit(PeekAhead(pos)) && PeekAhead(pos) != '_' && PeekAhead(pos) != '-')
                return false;
            pos++;
        }

        if (PeekAhead(pos) != ':' || pos == 1)
            return false;

        EmitText();
        Read(); // Consume opening :

        // Read emoji name
        var emojiName = new StringBuilder();
        while (Peek() != -1 && Peek() != ':')
        {
            emojiName.Append((char)Read());
        }

        if (Peek() == ':')
            Read(); // Consume closing :

        _onToken(new MarkdownToken(
            MarkdownTokenType.Emoji,
            emojiName.ToString(),
            new EmojiMetadata(emojiName.ToString())
        ));

        return true;
    }

    internal bool TryParseSubscript()
    {
        if (Peek() != '^') return false;

        EmitText();
        Read(); // Consume opening ^

        // Read subscript text until closing ^
        var subText = new StringBuilder();
        while (Peek() != -1 && Peek() != '\n')
        {
            char c = (char)Read();
            if (c == '^')
            {
                _onToken(new MarkdownToken(MarkdownTokenType.Subscript, subText.ToString()));
                return true;
            }
            subText.Append(c);
        }

        // No closing found, treat as text
        _buffer.Append('^').Append(subText);
        return true;
    }

    internal bool TryParseSuperscript()
    {
        if (Peek() != '~') return false;

        EmitText();
        Read(); // Consume opening ~

        // Read superscript text until closing ~
        var supText = new StringBuilder();
        while (Peek() != -1 && Peek() != '\n')
        {
            char c = (char)Read();
            if (c == '~')
            {
                _onToken(new MarkdownToken(MarkdownTokenType.Superscript, supText.ToString()));
                return true;
            }
            supText.Append(c);
        }

        // No closing found, treat as text
        _buffer.Append('~').Append(supText);
        return true;
    }

    internal bool TryParseInsertedText()
    {
        if (PeekAhead(0) != '+' || PeekAhead(1) != '+')
            return false;

        EmitText();
        Read(); // Consume first +
        Read(); // Consume second +

        // Read inserted text until closing ++
        var insText = new StringBuilder();
        while (true)
        {
            var ch = Peek();
            if (ch == -1 || ch == '\n')
            {
                break;
            }

            if (ch == '+' && PeekAhead(1) == '+')
            {
                Read();
                Read();
                _onToken(new MarkdownToken(MarkdownTokenType.InsertedText, insText.ToString()));
                return true;
            }
            insText.Append((char)Read());
        }

        // No closing found, treat as text
        _buffer.Append("++").Append(insText);
        return true;
    }

    internal bool TryParseMarkedText()
    {
        if (PeekAhead(0) != '=' || PeekAhead(1) != '=')
            return false;

        EmitText();
        Read(); // Consume first =
        Read(); // Consume second =

        // Read marked text until closing ==
        var markedText = new StringBuilder();
        while (Peek() != -1)
        {
            if (PeekAhead(0) == '=' && PeekAhead(1) == '=')
            {
                Read();
                Read();
                _onToken(new MarkdownToken(MarkdownTokenType.MarkedText, markedText.ToString()));
                return true;
            }
            markedText.Append((char)Read());
        }

        // No closing found, treat as text
        _buffer.Append("==").Append(markedText);
        return true;
    }

    internal void EmitText()
    {
        if (_buffer.Length > 0)
        {
            _onToken(new MarkdownToken(MarkdownTokenType.Text, _buffer.ToString()));
            _buffer.Clear();
        }
    }

    /// <summary>
    /// CommonMark ASCII punctuation characters that may be backslash-escaped.
    /// </summary>
    private static readonly HashSet<char> AsciiPunctuation = new()
    {
        '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
        ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~'
    };

    /// <summary>
    /// Handles backslash-escaped ASCII punctuation. Consumes the backslash and the punctuation character,
    /// emitting only the punctuation character as plain text.
    /// </summary>
    private bool TryParseBackslashEscape()
    {
        int next = PeekAhead(1);
        if (next == -1) return false;

        char nextCh = (char)next;
        if (!AsciiPunctuation.Contains(nextCh)) return false;

        Read(); // Consume backslash
        Read(); // Consume punctuation
        _buffer.Append(nextCh);
        return true;
    }

    /// <summary>
    /// Parses inline constructs such as bold, italic
    /// </summary>
    internal protected bool TryParseInlineConstruct(char ch) => ch switch
    {
        '\\' when TryParseBackslashEscape() => true,
        '*' when TryParseBoldOrItalic() => true,
        '_' when TryParseBoldOrItalic() => true,
        '`' when TryParseInlineCode() => true,
        '[' when TryParseLink() => true,
        '!' when PeekAhead(1) == '[' && TryParseImage() => true,
        ':' when TryParseEmoji() => true,
        '^' when TryParseSubscript() => true,
        '~' when TryParseSuperscript() => true,
        '+' when PeekAhead(1) == '+' && TryParseInsertedText() => true,
        '=' when PeekAhead(1) == '=' && TryParseMarkedText() => true,
        '<' when TryParseHtmlTag() => true,
        _ => false
    };

    private bool TryParseHtmlTag()
    {
        if (Peek() != '<') return false;

        // Check if it looks like an HTML tag
        char next = PeekAhead(1);

        // Must start with letter or / for closing tags
        if (!char.IsLetter(next) && next != '/')
            return false;

        EmitText();
        Read(); // Consume <

        // Read tag content until >
        var tagContent = new StringBuilder();
        tagContent.Append('<');

        while (Peek() != -1)
        {
            char c = (char)Read();
            tagContent.Append(c);

            if (c == '>')
            {
                _onToken(new MarkdownToken(MarkdownTokenType.HtmlTag, tagContent.ToString()));
                return true;
            }
        }

        // No closing found, treat as text
        _buffer.Append(tagContent);
        return true;
    }
}
