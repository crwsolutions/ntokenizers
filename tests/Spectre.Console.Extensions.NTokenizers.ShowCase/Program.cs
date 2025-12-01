using NTokenizers.Markup;
using NTokenizers.ShowCase;
using Spectre.Console.Extensions.NTokenizers.Writers;
using System.IO.Pipes;
using System.Text;

using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
using var reader = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);

//Console.ReadLine();

var writerTask = Task.Run(async () =>
{
    var rng = new Random();
    byte[] bytes = Encoding.UTF8.GetBytes(MarkupExample.GetSampleText());
    foreach (var b in bytes)
    {
        await pipe.WriteAsync(new[] { b }.AsMemory(0, 1));
        await pipe.FlushAsync();
        await Task.Delay(rng.Next(0, 2));
    }

    pipe.Close();
});

//AnsiConsole.Console.WriteMarkup(reader);

await MarkupTokenizer.Create().ParseAsync(
    reader,
    async token => await MarkupWriter.WriteAsync(token)
);

await writerTask;

Console.WriteLine("\nDone.");