using NTokenizers.Rust;
using System;

class Program
{
    static void Main(string[] args)
    {
        var code = """
            impl<T> Foo<T> {
                fn new(x: T) -> Self {
                    Self { x }
                }
                fn get(&self) -> &T {
                    &self.x
                }
            }
            """;
        var tokens = RustTokenizer.Create().Parse(code);
        foreach (var t in tokens)
        {
            Console.WriteLine($"[{t.TokenType,-20}] {t.Value.Replace("\n", "\\n")}");
        }
    }
}
