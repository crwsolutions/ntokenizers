using NTokenizers.Go;
using System;

class Program
{
    static void Main(string[] args)
    {
        var goCode = """
            func fibonacci(n int) []int {
                fibs := make([]int, n)
                for i := 2; i < n; i++ {
                    fibs[i] = fibs[i-1] + fibs[i-2]
                }
                return fibs
            }
            """;

        var tokenizer = GoTokenizer.Create();
        var tokens = tokenizer.Parse(goCode);

        foreach (var token in tokens)
        {
            Console.WriteLine($"[{token.TokenType,-20}] {token.Value.Replace("\n", "\\n")}");
        }
    }
}
