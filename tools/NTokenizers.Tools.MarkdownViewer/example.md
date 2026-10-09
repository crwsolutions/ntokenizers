# NTokenizers Showcase

Welcome to the MarkdownViewer! This file showcases every Markdown feature that the ToHtml converter supports.

## Headings

# Heading 1
## Heading 2
### Heading 3
#### Heading 4
##### Heading 5
###### Heading 6

## Emphasis

You can write **bold text** or *italic text*. You can also use __double underscores__ for bold and _single underscores_ for italic.

To combine both: **bold with _italic_ inside** or _italic with **bold** inside_.

~~Strikethrough~~ text is supported too.

## Lists

### Unordered

- Apples
- Bananas
- Cherries
    - Nested item A
    - Nested item B
        - Deeply nested

### Ordered

1. First step
2. Second step
3. Third step
    a. Sub-step A
    b. Sub-step B
4. Fourth step

### Mixed

1. Build the app
   - Install dependencies
   - Write code
   - Run tests
2. Deploy
3. Monitor

## Code

### Inline

Use `Console.WriteLine("Hello")` to print to the console. You can also use backticks in code spans: `` `single` `` and `` `double` ``.

### Fenced code blocks

```csharp
using System;

namespace Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var items = new List<string> { "alpha", "beta", "gamma" };
            foreach (var item in items)
            {
                Console.WriteLine($"Processing: {item}");
            }
        }
    }
}
```

```python
def fibonacci(n: int) -> list[int]:
    """Generate the first n Fibonacci numbers."""
    seq = [0, 1]
    for _ in range(n - 2):
        seq.append(seq[-1] + seq[-2])
    return seq

print(fibonacci(10))
```

```javascript
const users = [
    { name: "Alice", role: "admin" },
    { name: "Bob", role: "user" },
    { name: "Charlie", role: "moderator" }
];

const admins = users.filter(u => u.role === "admin");
console.log(`Found ${admins.length} admin(s)`);
```

```sql
SELECT
    u.id,
    u.email,
    COUNT(o.id) AS order_count,
    SUM(o.total) AS lifetime_value
FROM users u
LEFT JOIN orders o ON o.user_id = u.id
WHERE u.created_at >= '2025-01-01'
GROUP BY u.id, u.email
HAVING COUNT(o.id) > 5
ORDER BY lifetime_value DESC;
```

```rust
fn main() {
    let numbers: Vec<i32> = vec![1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    let evens: Vec<i32> = numbers.iter().filter(|x| x % 2 == 0).copied().collect();
    let sum: i32 = evens.iter().sum();
    println!("Even numbers: {:?}", evens);
    println!("Sum: {}", sum);
}
```

## Blockquotes

> "The best way to predict the future is to invent it."
> — Alan Kay

> Nested quote:
>
> > "Simplicity is the ultimate sophistication."
> > — Leonardo da Vinci
>
> Back to outer quote.

## Links

Here is a [link to GitHub](https://github.com/crwsolutions/ntokenizers).

Email links: [colin@example.com](mailto:colin@example.com)

Reference-style links are **not supported** (streaming limitation) but inline links work perfectly.

## Tables

| Feature          | Status  | Notes                          |
| ---------------- | ------- | ------------------------------ |
| Streaming parser | ✅ Done | Character-by-character         |
| ToHtml API       | ✅ Done | Full document + fragment       |
| Link references  | ❌ No   | Requires full document buffer  |
| HTML blocks      | ❌ No   | Inline pass-through only       |

## Horizontal rules

---

***

___

## Nested structures

> A quote that contains a list:
>
> 1. First item
> 2. Second item
>    - Nested bullet
>    - Another bullet
> 3. Third item

> A quote with code:
>
> ```
> this is code inside a blockquote
> ```

## Special characters

Backslash escapes: \*not italic\* \_not italic\_ \\ backslash

Entity-like text (not decoded by design): &amp; &lt; &gt; &#38; &#x26;

Autolinks: https://example.com and https://github.com/crwsolutions/ntokenizers/tree/main

## Fenced code with tilde

~~~python
# Tilde fences work too
x = ~3  # bitwise NOT
print(x)
~~~

## Long paragraphs with inline formatting

This is a long paragraph that mixes **bold**, *italic*, and `code` throughout the text. It also contains a [link](https://docs.microsoft.com/dotnet/) and some ~~struck through~~ text. The paragraph should wrap naturally in the viewer, and all inline formatting should render correctly even when the text spans multiple lines in the source.

> This blockquote contains **bold text** and a [link](https://example.com) and `inline code`.
