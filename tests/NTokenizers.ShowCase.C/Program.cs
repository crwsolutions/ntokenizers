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

Console.WriteLine("=== C Tokenizer Showcase ===");
Console.WriteLine();

foreach (var token in tokens)
{
    switch (token.TokenType)
    {
        case CTokenType.Keyword:
            Console.Write($"{token.Value,-20}", ConsoleColor.Red);
            break;
        case CTokenType.StringValue:
            Console.Write($"{token.Value,-20}", ConsoleColor.Green);
            break;
        case CTokenType.Number:
            Console.Write($"{token.Value,-20}", ConsoleColor.Cyan);
            break;
        case CTokenType.Comment:
            Console.Write($"{token.Value,-20}", ConsoleColor.DarkGray);
            break;
        case CTokenType.Preprocessor:
            Console.Write($"{token.Value,-20}", ConsoleColor.Magenta);
            break;
        case CTokenType.Operator:
            Console.Write($"{token.Value,-20}", ConsoleColor.Yellow);
            break;
        case CTokenType.Identifier:
            Console.Write($"{token.Value,-20}", ConsoleColor.White);
            break;
        default:
            Console.Write($"{token.Value,-20}");
            break;
    }
}

Console.WriteLine();
Console.WriteLine();
Console.WriteLine($"Total tokens: {tokens.Count}");
