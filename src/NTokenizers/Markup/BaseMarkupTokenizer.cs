using NTokenizers.Core;
using NTokenizers.Markup.Metadata;
using System.Text;

namespace NTokenizers.Markup;

/// <summary>
/// Abstract base class for markup tokenizers that provides common functionality for parsing markup text streams.
/// </summary>
public abstract class BaseMarkupTokenizer : BaseTokenizer<MarkupToken>
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
                    _onToken(new MarkupToken(MarkupTokenType.Bold, boldText.ToString()));
                    return true;
                }
                boldText.Append((char)Read());
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
                    _onToken(new MarkupToken(MarkupTokenType.Bold, boldText.ToString()));
                    return true;
                }
                boldText.Append((char)Read());
            }
            _buffer.Append("__").Append(boldText);
            return false;
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
                    _onToken(new MarkupToken(MarkupTokenType.Italic, italicText.ToString()));
                    return true;
                }
                italicText.Append((char)Read());
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
                italicText.Append((char)Read());
            }
            if (Peek() == '_')
            {
                Read();
                _onToken(new MarkupToken(MarkupTokenType.Italic, italicText.ToString()));
                return true;
            }
            _buffer.Append('_').Append(italicText);
            return false;
        }

        return false;
    }

    internal bool TryParseInlineCode()
    {
        if (Peek() != '`') return false;

        EmitText();
        Read(); // Consume opening `

        // Read code until closing `
        var code = new StringBuilder();
        while (Peek() != -1 && Peek() != '\n')
        {
            char c = (char)Read();
            if (c == '`')
            {
                _onToken(new MarkupToken(MarkupTokenType.CodeInline, code.ToString()));
                return true;
            }
            code.Append(c);
        }

        // No closing found, treat as text
        _buffer.Append('`').Append(code);
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

        // Read URL
        var url = new StringBuilder();
        string? title = null;
        bool inQuote = false;
        var titleBuilder = new StringBuilder();

        while (Peek() != -1 && Peek() != ')')
        {
            char c = (char)Read();

            if (c == '"' && !inQuote)
            {
                inQuote = true;
            }
            else if (c == '"' && inQuote)
            {
                title = titleBuilder.ToString();
                inQuote = false;
            }
            else if (inQuote)
            {
                titleBuilder.Append(c);
            }
            else if (c == ' ' && url.Length > 0 && Peek() == '"')
            {
                // Space before title
            }
            else
            {
                url.Append(c);
            }
        }

        if (Peek() == ')')
            Read(); // Consume )

        _onToken(new MarkupToken(
            MarkupTokenType.Link,
            linkText.ToString(),
            new LinkMetadata(url.ToString().Trim(), title)
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

        // Read URL
        var url = new StringBuilder();
        string? title = null;
        bool inQuote = false;
        var titleBuilder = new StringBuilder();

        while (Peek() != -1 && Peek() != ')')
        {
            char c = (char)Read();

            if (c == '"' && !inQuote)
            {
                inQuote = true;
            }
            else if (c == '"' && inQuote)
            {
                title = titleBuilder.ToString();
                inQuote = false;
            }
            else if (inQuote)
            {
                titleBuilder.Append(c);
            }
            else if (c == ' ' && url.Length > 0 && Peek() == '"')
            {
                // Space before title
            }
            else
            {
                url.Append(c);
            }
        }

        if (Peek() == ')')
            Read(); // Consume )

        _onToken(new MarkupToken(
            MarkupTokenType.Image,
            altText.ToString(),
            new LinkMetadata(url.ToString().Trim(), title)
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

        _onToken(new MarkupToken(
            MarkupTokenType.Emoji,
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
                _onToken(new MarkupToken(MarkupTokenType.Subscript, subText.ToString()));
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
                _onToken(new MarkupToken(MarkupTokenType.Superscript, supText.ToString()));
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
                _onToken(new MarkupToken(MarkupTokenType.InsertedText, insText.ToString()));
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
                _onToken(new MarkupToken(MarkupTokenType.MarkedText, markedText.ToString()));
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
            _onToken(new MarkupToken(MarkupTokenType.Text, _buffer.ToString()));
            _buffer.Clear();
        }
    }

    /// <summary>
    /// Parses inline constructs such as bold, italic
    /// </summary>
    internal protected bool TryParseInlineConstruct(char ch) => ch switch
    {
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
                _onToken(new MarkupToken(MarkupTokenType.HtmlTag, tagContent.ToString()));
                return true;
            }
        }

        // No closing found, treat as text
        _buffer.Append(tagContent);
        return true;
    }
}
