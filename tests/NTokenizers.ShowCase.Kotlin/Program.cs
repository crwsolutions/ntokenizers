using NTokenizers.Kotlin;
using System;

class Program
{
    static void Main(string[] args)
    {
        var kotlinCode = """
            data class Config(
                val host: String,
                val port: Int,
                val enabled: Boolean = true
            ) {
                companion object {
                    val DEFAULT = Config("localhost", 8080, true)
                }
            }
            """;

        var tokenizer = KotlinTokenizer.Create();
        var tokens = tokenizer.Parse(kotlinCode);

        foreach (var token in tokens)
        {
            Console.WriteLine($"[{token.TokenType,-20}] {token.Value.Replace("\n", "\\n")}");
        }
    }
}
