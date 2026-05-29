using NTokenizers.Cpp;
using System;

class Program
{
    static void Main(string[] args)
    {
        var tokens = CppTokenizer.Create().Parse("// line comment\n/* block comment */");
        foreach (var t in tokens)
        {
            Console.WriteLine($"[{t.TokenType,-20}] {t.Value.Replace("\n", "\\n")}");
        }
    }
}
