namespace NTokenizers.Yaml;

/// <summary>
/// Represents the type of a YAML token.
/// </summary>
public enum YamlTokenType
{
    /// <summary>
    /// Represents a % directive
    /// </summary>
    Directive,

    /// <summary>
    /// Directive key, YAML or TAG
    /// </summary>
    DirectiveKey,

    /// <summary>
    /// Directive value
    /// </summary>
    DirectiveValue,

    /// <summary>
    /// Represents the document start marker (---).
    /// </summary>
    DocumentStart,

    /// <summary>
    /// Represents the document end marker (...).
    /// </summary>
    DocumentEnd,

    /// <summary>
    /// Represents a comment (# ...).
    /// </summary>
    Comment,

    /// <summary>
    /// Represents a key before a colon.
    /// </summary>
    Key,

    /// <summary>
    /// Represents a colon (:) separator.
    /// </summary>
    Colon,

    /// <summary>
    /// Represents a plain value or quoted string value.
    /// </summary>
    Value,

    /// <summary>
    /// Represents a quote character (").
    /// </summary>
    Quote,

    /// <summary>
    /// Represents the content between quotes.
    /// </summary>
    String,

    /// <summary>
    /// Represents an anchor (&amp;anchor).
    /// </summary>
    Anchor,

    /// <summary>
    /// Represents an alias (*alias).
    /// </summary>
    Alias,

    /// <summary>
    /// Represents a tag (!tag or !!type).
    /// </summary>
    Tag,

    /// <summary>
    /// Represents the start of a flow sequence ([).
    /// </summary>
    FlowSeqStart,

    /// <summary>
    /// Represents the end of a flow sequence (]).
    /// </summary>
    FlowSeqEnd,

    /// <summary>
    /// Represents the start of a flow mapping ({).
    /// </summary>
    FlowMapStart,

    /// <summary>
    /// Represents the end of a flow mapping (}).
    /// </summary>
    FlowMapEnd,

    /// <summary>
    /// Represents a comma (,) separator in flow collections.
    /// </summary>
    FlowEntry,

    /// <summary>
    /// Represents a block sequence entry marker (-).
    /// </summary>
    BlockSeqEntry,

    /// <summary>
    /// Represents whitespace characters (spaces, tabs, newlines).
    /// </summary>
    Whitespace
}
