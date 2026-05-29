using NTokenizers.Swift;
using System;

class Program
{
    static void Main(string[] args)
    {
        var swiftCode = """
            struct User {
                let id: Int
                var name: String
                init(id: Int, name: String) {
                    self.id = id
                    self.name = name
                }
            }
            """;

        var tokenizer = SwiftTokenizer.Create();
        var tokens = tokenizer.Parse(swiftCode);

        foreach (var token in tokens)
        {
            Console.WriteLine($"[{token.TokenType,-20}] {token.Value.Replace("\n", "\\n")}");
        }
    }
}
