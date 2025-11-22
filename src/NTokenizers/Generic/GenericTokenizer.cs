using NTokenizers.Markup;
using System.Text;

namespace NTokenizers.Generic;
internal class GenericTokenizer
{
    internal static void Parse(TextReader reader, string stopDelimiter, Action<MarkupToken> onToken)
    {
        var sb = new StringBuilder();
        int delLength = stopDelimiter.Length;

        var delQueue = new Queue<char>();
        bool stoppedByDelimiter = false;
        while (true)
        {
            int ic = reader.Read();
            if (ic == -1)
            {
                break;
            }
            char c = (char)ic;
            delQueue.Enqueue(c);

            if (delQueue.Count > delLength)
            {
                char toProcess = delQueue.Dequeue();
                sb.Append(toProcess);
                if (char.IsWhiteSpace(toProcess))
                {
                    EmitPending(sb, onToken);
                }
            }

            if (delQueue.Count == delLength && new string(delQueue.ToArray()) == stopDelimiter)
            {
                stoppedByDelimiter = true;
                break;
            }
        }
        if (!stoppedByDelimiter)
        {
            while (delQueue.Count > 0)
            {
                char toProcess = delQueue.Dequeue();
                sb.Append(toProcess);
            }
        }
        EmitPending(sb, onToken);
    }

    private static void EmitPending(StringBuilder sb, Action<MarkupToken> onToken)
    {
        if (sb.Length > 0)
        {
            onToken(new MarkupToken(MarkupTokenType.Text, sb.ToString()));
            sb.Clear();
        }
    }
}
