using NTokenizers.Markup;
using NTokenizers.ShowCase;
using System.IO.Pipes;
using System.Text;

class Program
{
    static async Task Main()
    {
        var markup = MarkupExample.GetSampleText();

        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

        var writerTask = EmitSlowlyAsync(markup, pipe);

        MarkupTokenizer.Parse(reader, MarkupWriter.Write);

        await writerTask;
        Console.WriteLine("\nDone.");
    }

    static async Task EmitSlowlyAsync(string text, Stream output)
    {
        var rng = new Random();
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        foreach (var b in bytes)
        {
            await output.WriteAsync(new[] { b }.AsMemory(0, 1));
            await output.FlushAsync();
            await Task.Delay(rng.Next(1, 5));
        }
        output.Close();
    }
}
