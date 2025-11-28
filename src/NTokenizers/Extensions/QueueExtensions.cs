using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NTokenizers.Extensions;

internal static class QueueExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsEqualTo(this Queue<char> queue, string str)
    {
        if (queue.Count != str.Length) return false;

        int i = 0;
        foreach (var c in queue)
        {
            if (c != str[i])
            {
                return false;
            }

            i++;
        }

        return true;
    }
}

