using System.Text;
using NTokenizers.Core;
using NTokenizers.Markdown.Metadata;

namespace NTokenizers.Markdown;

/// <summary>
/// Abstract base class for markdown tokenizers that provides common functionality for parsing markdown text streams.
/// </summary>
public abstract class BaseMarkdownTokenizer : BaseTokenizer<MarkdownToken>
{
    /// <summary>
    /// Checks whether the buffered text ends with a word character (letter, digit, or underscore).
    /// Used to determine if an emphasis delimiter is left-flanking.
    /// </summary>
    private bool PrecededByWordCharacter()
    {
        if (_buffer.Length == 0)
        {
            return false;
        }
        return char.IsLetterOrDigit(_buffer[_buffer.Length - 1]) || _buffer[_buffer.Length - 1] == '_';
    }

    internal async Task<bool> TryParseBoldOrItalicAsync()
    {
        // Check for *** (bold italic with asterisks), but not four or more
        // (which should be handled as nested bold). Only opens bold italic when
        // not preceded by a word character (left-flanking rule). If the closing
        // delimiter is ** instead of ***, fall back to bold with the extra * as text.
        if (PeekAhead(0) == '*' && PeekAhead(1) == '*' && PeekAhead(2) == '*' && PeekAhead(3) != '*'
            && !PrecededByWordCharacter())
        {
            await EmitTextAsync();
            Read();
            Read();
            Read();

            var content = new StringBuilder();
            while (Peek() != -1)
            {
                if (PeekAhead(0) == '*' && PeekAhead(1) == '*' && PeekAhead(2) == '*')
                {
                    // Closing ***, emit bold italic
                    Read();
                    Read();
                    Read();
                    await _onToken(new MarkdownToken(MarkdownTokenType.BoldItalic, content.ToString()));
                    return true;
                }
                if (PeekAhead(0) == '*' && PeekAhead(1) == '*' && PeekAhead(2) != '*')
                {
                    // Closing **, fall back to bold with leading * as text
                    Read();
                    Read();
                    await _onToken(new MarkdownToken(MarkdownTokenType.Bold, "*" + content.ToString()));
                    return true;
                }
                AppendEscapedChar(content);
            }

            // No closing found, treat as text
            _buffer.Append("***").Append(content);
            return true;
        }

        // Check for ___ (bold italic with underscores), but not four or more.
        // Only opens bold italic when not preceded by a word character.
        if (PeekAhead(0) == '_' && PeekAhead(1) == '_' && PeekAhead(2) == '_' && PeekAhead(3) != '_'
            && !PrecededByWordCharacter())
        {
            await EmitTextAsync();
            Read();
            Read();
            Read();

            var content = new StringBuilder();
            while (Peek() != -1)
            {
                if (PeekAhead(0) == '_' && PeekAhead(1) == '_' && PeekAhead(2) == '_')
                {
                    Read();
                    Read();
                    Read();
                    await _onToken(new MarkdownToken(MarkdownTokenType.BoldItalic, content.ToString()));
                    return true;
                }
                if (PeekAhead(0) == '_' && PeekAhead(1) == '_' && PeekAhead(2) != '_')
                {
                    Read();
                    Read();
                    await _onToken(new MarkdownToken(MarkdownTokenType.Bold, "_" + content.ToString()));
                    return true;
                }
                AppendEscapedChar(content);
            }

            // No closing found, treat as text
            _buffer.Append("___").Append(content);
            return true;
        }

        // Check for **
        if (PeekAhead(0) == '*' && PeekAhead(1) == '*')
        {
            await EmitTextAsync();
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
                    await _onToken(new MarkdownToken(MarkdownTokenType.Bold, boldText.ToString()));
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
            await EmitTextAsync();
            Read();
            Read();
            var boldText = new StringBuilder();
            while (Peek() != -1)
            {
                if (PeekAhead(0) == '_' && PeekAhead(1) == '_')
                {
                    Read(); Read();
                    await _onToken(new MarkdownToken(MarkdownTokenType.Bold, boldText.ToString()));
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
            await EmitTextAsync();
            Read();

            // Read italic text until closing *
            var italicText = new StringBuilder();
            while (Peek() != -1)
            {
                if (Peek() == '*')
                {
                    Read();
                    await _onToken(new MarkdownToken(MarkdownTokenType.Italic, italicText.ToString()));
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
            await EmitTextAsync();
            Read();
            var italicText = new StringBuilder();
            while (Peek() != -1 && Peek() != '_')
            {
                AppendEscapedChar(italicText);
            }
            if (Peek() == '_')
            {
                Read();
                await _onToken(new MarkdownToken(MarkdownTokenType.Italic, italicText.ToString()));
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

    internal async Task<bool> TryParseInlineCodeAsync()
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

        await EmitTextAsync();

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
            await _onToken(new MarkdownToken(MarkdownTokenType.CodeInline, code.ToString()));
            return true;
        }

        // No closing run was found, so this is not a real code span (the opening backtick(s)
        // are unpaired). The backtick(s) are literal text and the scanned content must go
        // through the normal inline pipeline (backslash escapes, raw HTML, etc.) rather than
        // being swallowed raw to end of stream. Emit the backtick(s) and re-queue the content
        // so the caller processes it character by character.
        for (int i = 0; i < openLength; i++)
        {
            _buffer.Append('`');
        }

        for (int i = 0; i < code.Length; i++)
        {
            _lookaheadBuffer.Enqueue(code[i]);
        }

        return true;
    }

    internal async Task<bool> TryParseLinkAsync()
    {
        if (Peek() != '[') return false;

        // Look ahead for ]( pattern
        int pos = 1;
        while (PeekAhead(pos) != '\0' && PeekAhead(pos) != '\n' && PeekAhead(pos) != ']')
            pos++;

        if (PeekAhead(pos) != ']' || PeekAhead(pos + 1) != '(')
            return false;

        await EmitTextAsync();
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

        await _onToken(new MarkdownToken(
            MarkdownTokenType.Link,
            value,
            new LinkMetadata(urlStr, linkText.Length > 0 ? linkText.ToString().Trim() : null, title, isBracketed)
        ));

        return true;
    }

    internal async Task<bool> TryParseImageAsync()
    {
        if (PeekAhead(0) != '!' || PeekAhead(1) != '[')
            return false;

        await EmitTextAsync();
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

        await _onToken(new MarkdownToken(
            MarkdownTokenType.Image,
            value,
            new LinkMetadata(urlStr, altText.Length > 0 ? altText.ToString().Trim() : null, title, isBracketed)
        ));

        return true;
    }

    internal async Task<bool> TryParseEmojiAsync()
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

        await EmitTextAsync();
        Read(); // Consume opening :

        // Read emoji name
        var emojiName = new StringBuilder();
        while (Peek() != -1 && Peek() != ':')
        {
            emojiName.Append((char)Read());
        }

        if (Peek() == ':')
            Read(); // Consume closing :

        await _onToken(new MarkdownToken(
            MarkdownTokenType.Emoji,
            emojiName.ToString(),
            new EmojiMetadata(emojiName.ToString())
        ));

        return true;
    }

    internal async Task<bool> TryParseSubscriptAsync()
    {
        if (Peek() != '~') return false;

        await EmitTextAsync();
        Read(); // Consume opening ~

        // Read subscript text until closing ~
        var subText = new StringBuilder();
        while (Peek() != -1 && Peek() != '\n')
        {
            char c = (char)Read();
            if (c == '~')
            {
                await _onToken(new MarkdownToken(MarkdownTokenType.Subscript, subText.ToString()));
                return true;
            }
            subText.Append(c);
        }

        // No closing found, treat as text
        _buffer.Append('~').Append(subText);
        return true;
    }

    internal async Task<bool> TryParseSuperscriptAsync()
    {
        if (Peek() != '^') return false;

        await EmitTextAsync();
        Read(); // Consume opening ^

        // Read superscript text until closing ^
        var supText = new StringBuilder();
        while (Peek() != -1 && Peek() != '\n')
        {
            char c = (char)Read();
            if (c == '^')
            {
                await _onToken(new MarkdownToken(MarkdownTokenType.Superscript, supText.ToString()));
                return true;
            }
            supText.Append(c);
        }

        // No closing found, treat as text
        _buffer.Append('^').Append(supText);
        return true;
    }

    internal async Task<bool> TryParseStrikethroughAsync()
    {
        if (PeekAhead(0) != '~' || PeekAhead(1) != '~')
            return false;

        await EmitTextAsync();
        Read(); // Consume first ~
        Read(); // Consume second ~

        // Read strikethrough text until closing ~~
        var delText = new StringBuilder();
        while (Peek() != -1)
        {
            if (PeekAhead(0) == '~' && PeekAhead(1) == '~')
            {
                Read();
                Read();
                await _onToken(new MarkdownToken(MarkdownTokenType.Strikethrough, delText.ToString()));
                return true;
            }
            delText.Append((char)Read());
        }

        // No closing found, treat as text
        _buffer.Append("~~").Append(delText);
        return true;
    }

    internal async Task<bool> TryParseInsertedTextAsync()
    {
        if (PeekAhead(0) != '+' || PeekAhead(1) != '+')
            return false;

        await EmitTextAsync();
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
                await _onToken(new MarkdownToken(MarkdownTokenType.InsertedText, insText.ToString()));
                return true;
            }
            insText.Append((char)Read());
        }

        // No closing found, treat as text
        _buffer.Append("++").Append(insText);
        return true;
    }

    internal async Task<bool> TryParseMarkedTextAsync()
    {
        if (PeekAhead(0) != '=' ||
            PeekAhead(1) != '=' ||
            char.IsWhiteSpace(PeekAhead(2)))
        {
            return false;
        }

        await EmitTextAsync();
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
                await _onToken(new MarkdownToken(MarkdownTokenType.MarkedText, markedText.ToString()));
                return true;
            }
            markedText.Append((char)Read());
        }

        // No closing found, treat as text
        _buffer.Append("==").Append(markedText);
        return true;
    }

    internal async Task EmitTextAsync()
    {
        if (_buffer.Length > 0)
        {
            await _onToken(new MarkdownToken(MarkdownTokenType.Text, _buffer.ToString()));
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
    /// emitting only the punctuation character as plain text. A backslash before any other
    /// character (letters, digits, whitespace) is not an escape and is left for the caller to
    /// emit as a literal character.
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
    internal protected async Task<bool> TryParseInlineConstructAsync(char ch)
    {
        switch (ch)
        {
            case '\\' when TryParseBackslashEscape():
                return true;
            case '*':
                return await TryParseBoldOrItalicAsync();
            case '_':
                return await TryParseBoldOrItalicAsync();
            case '`':
                return await TryParseInlineCodeAsync();
            case '[':
                return await TryParseLinkAsync();
            case '!' when PeekAhead(1) == '[':
                return await TryParseImageAsync();
            case ':':
                return await TryParseEmojiAsync();
            case '^':
                return await TryParseSuperscriptAsync();
            case '~' when PeekAhead(1) == '~':
                return await TryParseStrikethroughAsync();
            case '~':
                return await TryParseSubscriptAsync();
            case '+' when PeekAhead(1) == '+':
                return await TryParseInsertedTextAsync();
            case '=' when PeekAhead(1) == '=':
                return await TryParseMarkedTextAsync();
            case '<':
                return await TryParseHtmlTagAsync();
            default:
                return false;
        }
    }

    /// <summary>
    /// Parses an autolink or an inline HTML tag at an opening angle bracket.
    /// </summary>
    /// <remarks>
    /// A '&lt;' is treated as an HTML span only when it is followed by a letter, a '/'
    /// (closing tag), or '!' or '?' (declaration/comment/processing instruction); anything
    /// else (e.g. '&lt;33&gt;', '&lt; a&gt;') is plain text. The span is read up to the
    /// first closing '&gt;'. When it is a URI or email autolink a Link token is emitted;
    /// otherwise the span is emitted verbatim as an HtmlTag token (inline raw HTML
    /// pass-through: no tag structure is validated and no markdown is parsed inside the tag).
    /// A run without a closing '&gt;' is plain text. Newlines may occur inside the tag.
    /// </remarks>
    private async Task<bool> TryParseHtmlTagAsync()
    {
        if (Peek() != '<') return false;

        char next = PeekAhead(1);
        if (!char.IsLetter(next) && next != '/' && next != '!' && next != '?')
        {
            return false;
        }

        await EmitTextAsync();
        Read(); // Consume <

        // Read the span up to the first closing angle bracket (or end of stream when absent).
        var content = new StringBuilder();
        while (Peek() != -1 && Peek() != '>')
        {
            content.Append((char)Read());
        }

        // An autolink is a bracketed URI or email address; consume the closing '>' as well.
        if (await TryParseAutolinkAsync(content.ToString()))
        {
            if (Peek() == '>')
            {
                Read();
            }
            return true;
        }

        if (Peek() != '>')
        {
            // No closing found: treat the whole span as plain text.
            _buffer.Append('<').Append(content);
            return true;
        }

        Read(); // Consume >
        await _onToken(new MarkdownToken(MarkdownTokenType.HtmlTag, "<" + content + ">"));
        return true;
    }

    /// <summary>
    /// Checks whether the given span (the characters between the angle brackets of an
    /// autolink candidate) is a URI or email autolink. When it is, a Link token is emitted
    /// (the email gets a mailto: prefix) and true is returned.
    /// </summary>
    /// <remarks>
    /// URI autolinks: a ':' whose scheme starts with a letter, contains only
    /// [a-zA-Z0-9+.-], is at least two characters long, and whose part after the ':' is
    /// non-empty. Email autolinks: exactly one '@' with a non-empty local part and a domain
    /// part containing a '.'. Neither may contain spaces; a backslash disqualifies an
    /// email but is allowed (and percent-encoded by the writer) in a URI.
    /// </remarks>
    private async Task<bool> TryParseAutolinkAsync(string content)
    {
        if (content.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < content.Length; i++)
        {
            if (char.IsWhiteSpace(content[i]) || content[i] == '<' || content[i] == '>')
            {
                return false;
            }
        }

        int at = content.IndexOf('@');
        int colon = content.IndexOf(':');

        // A ':' and no '@' is a URI autolink; an '@' and no ':' is an email autolink.
        // (Both present means a URI autolink, as in MAILTO:FOO@BAR.BAZ.)
        if (colon >= 0 && (at < 0 || colon < at))
        {
            bool schemeValid = colon >= 2
                && char.IsLetter(content[0])
                && content[colon + 1] != '\0';
            for (int i = 1; schemeValid && i < colon; i++)
            {
                char ch = content[i];
                if (!char.IsLetterOrDigit(ch) && ch is not ('+' or '.' or '-'))
                {
                    schemeValid = false;
                }
            }

            if (!schemeValid)
            {
                return false;
            }

            await EmitLinkTokenAsync($"<{content}>", content, content, true);
            return true;
        }

        if (at > 0 && content.Substring(at + 1).Contains('.'))
        {
            if (content.Contains('\\'))
            {
                return false;
            }

            string email = content;
            await EmitLinkTokenAsync($"<{email}>", "mailto:" + email, email, true);
            return true;
        }

        return false;
    }

    private async Task EmitLinkTokenAsync(string value, string url, string text, bool isAutolink)
    {
        await _onToken(new MarkdownToken(MarkdownTokenType.Link, value,
            new LinkMetadata(url, text, null, IsBracketed: false, IsAutolink: isAutolink)));
    }
}
