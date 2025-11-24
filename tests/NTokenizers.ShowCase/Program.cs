using NTokenizers.Markup;
using NTokenizers.ShowCase;
using System.IO.Pipes;
using System.Text;

using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

var writerTask = Task.Run(async () =>
{
    var rng = new Random();
    byte[] bytes = Encoding.UTF8.GetBytes(MarkupExample.GetSampleText());
    foreach (var b in bytes)
    {
        await pipe.WriteAsync(new[] { b }.AsMemory(0, 1));
        await pipe.FlushAsync();
        await Task.Delay(rng.Next(1, 5));
    }

    pipe.Close();
});

MarkupTokenizer.Parse(reader, MarkupWriter.Write);

await writerTask;

Console.WriteLine("\nDone.");