namespace NTokenizers.ShowCase.ToHtml.AiChat;

/// <summary>
/// A StreamWriter that flushes every write for real-time streaming.
/// </summary>
public class FlushStreamWriter : StreamWriter
{
    public FlushStreamWriter(Stream stream) : base(stream, leaveOpen: true)
    {
    }

    public override void Write(string? value)
    {
        base.Write(value);
        Flush();
    }

    public override async Task WriteAsync(string? value)
    {
        await base.WriteAsync(value);
        await FlushAsync();
    }
}
