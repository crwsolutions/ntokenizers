namespace NTokenizers.Css;

/// <summary>
/// Represents the type of a CSS token.
/// </summary>
public enum CssTokenType
{
    /// <summary>
    /// Represents the start of a CSS rule set ({).
    /// </summary>
    StartRuleSet,  // {
    /// <summary>
    /// Represents the end of a CSS rule set (}).
    /// </summary>
    EndRuleSet,    // }
    /// <summary>
    /// Represents a CSS selector.
    /// </summary>
    Selector,      // .class, #id, element, etc.
    /// <summary>
    /// Represents a Pseudo element
    /// </summary>
    PseudoElement,      // ::before, ::after
    /// <summary>
    /// Represents a CSS property name.
    /// </summary>
    PropertyName,  // --color
    /// <summary>
    /// Represents a string value in CSS (quoted strings).
    /// </summary>
    StringValue,   // "string", 'string'
    /// <summary>
    /// Represents a quote character in CSS (single or double quotes).
    /// </summary>
    Quote,         // ", '
    /// <summary>
    /// Represents a number value in CSS (integer, float).
    /// </summary>
    Number,        // 10, 10.5, -5
    /// <summary>
    /// Represents a CSS unit (px, em, %, etc.).
    /// </summary>
    Unit,          // px, em, %, rem, pt, etc.
    /// <summary>
    /// Represents a CSS function (like url(), calc(), etc.).
    /// </summary>
    Function,      // url(), calc(), var()
    /// <summary>
    /// Represents the opening parenthesis of a function.
    /// </summary>
    OpenParen,     // (
    /// <summary>
    /// Represents the closing parenthesis of a function.
    /// </summary>
    CloseParen,    // )
    /// <summary>
    /// Represents a CSS comment.
    /// </summary>
    Comment,       // /* comment */
    /// <summary>
    /// Represents a colon (:) used to separate property names and values.
    /// </summary>
    Colon,         // :
    /// <summary>
    /// Represents a semicolon (;) used to separate CSS declarations.
    /// </summary>
    Semicolon,     // ;
    /// <summary>
    /// Represents a comma (,) used to separate multiple values or selectors.
    /// </summary>
    Comma,         // ,
    /// <summary>
    /// Represents whitespace characters between tokens.
    /// </summary>
    Whitespace,    // spaces, tabs, newlines
    /// <summary>
    /// Represents an at-rule (like @media, @import, etc.).
    /// </summary>
    AtRule,        // @media, @import, @keyframes, etc.
    /// <summary>
    /// Represents a CSS identifier (including selectors and property names).
    /// </summary>
    Identifier,    // .class-name, #id, property-name
    /// <summary>
    /// Represents a CSS operator (+, -, *, /).
    /// </summary>
    Operator,      // +, -, *, /
    /// <summary>
    /// Represents a dot class selector (.class).
    /// </summary>
    DotClass,      // .class
    /// <summary>
    /// Represents an attribute selector opening bracket ([).
    /// </summary>
    LeftBracket,   // [
    /// <summary>
    /// Represents an attribute selector closing bracket (]).
    /// </summary>
    RightBracket,  // ]
    /// <summary>
    /// Represents the equality operator (=).
    /// </summary>
    Equals
}