using NTokenizers.Cpp;
using Spectre.Console;

var cppCode = """
    #include <iostream>
    #include <vector>
    #include <string>

    template<typename T>
    class Stack {
    private:
        std::vector<T> elements;

    public:
        void push(const T& element) {
            elements.push_back(element);
        }

        T pop() {
            T element = elements.back();
            elements.pop_back();
            return element;
        }

        bool empty() const {
            return elements.empty();
        }
    };

    int main() {
        Stack<int> intStack;
        intStack.push(42);
        intStack.push(17);
        intStack.push(99);

        while (!intStack.empty()) {
            std::cout << intStack.pop() << std::endl;
        }

        return 0;
    }
    """;

var tokenizer = CppTokenizer.Create();
var tokens = tokenizer.Parse(cppCode).ToList();

foreach (var token in tokens)
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        CppTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        CppTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        CppTokenType.StringValue => new Markup($"[green]{value}[/]"),
        CppTokenType.Number => new Markup($"[magenta]{value}[/]"),
        CppTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        CppTokenType.Comment => new Markup($"[grey]{value}[/]"),
        CppTokenType.Boolean => new Markup($"[magenta]{value}[/]"),
        CppTokenType.Null => new Markup($"[magenta]{value}[/]"),
        CppTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
}

Console.WriteLine();
Console.WriteLine("Done.");
