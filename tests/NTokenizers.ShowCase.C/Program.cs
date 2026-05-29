using NTokenizers.C;
using Spectre.Console;

var cCode = """
    #include <stdio.h>
    #include <stdlib.h>

    struct Node {
        int data;
        struct Node *next;
    };

    int main(void) {
        struct Node *head = NULL;
        int values[] = {1, 2, 3, 4, 5};

        for (int i = 0; i < 5; i++) {
            struct Node *newNode = malloc(sizeof(struct Node));
            newNode->data = values[i];
            newNode->next = head;
            head = newNode;
        }

        printf("Linked list: ");
        struct Node *current = head;
        while (current != NULL) {
            printf("%d ", current->data);
            current = current->next;
        }
        printf("\\n");

        return 0;
    }
    """;

var tokenizer = CTokenizer.Create();
var tokens = tokenizer.Parse(cCode).ToList();

foreach (var token in tokens)
{
    var value = Markup.Escape(token.Value);
    var colored = token.TokenType switch
    {
        CTokenType.Keyword => new Markup($"[blue]{value}[/]"),
        CTokenType.Identifier => new Markup($"[cyan]{value}[/]"),
        CTokenType.StringValue => new Markup($"[green]{value}[/]"),
        CTokenType.Number => new Markup($"[magenta]{value}[/]"),
        CTokenType.Operator => new Markup($"[yellow]{value}[/]"),
        CTokenType.Comment => new Markup($"[grey]{value}[/]"),
        CTokenType.Preprocessor => new Markup($"[yellow]{value}[/]"),
        CTokenType.Whitespace => new Markup($"[grey]{value}[/]"),
        _ => new Markup(value)
    };
    AnsiConsole.Write(colored);
}

Console.WriteLine();
Console.WriteLine("Done.");
