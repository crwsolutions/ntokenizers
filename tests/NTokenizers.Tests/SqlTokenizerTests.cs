using NTokenizers.Sql;
using System.Text;

namespace Sql;

public class SqlTokenizerTests
{
    [Fact]
    public void TestSimpleSelect()
    {
        var tokens = Tokenize("SELECT * FROM users");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[2].TokenType);
        Assert.Equal("*", tokens[2].Value);
        Assert.Equal(SqlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[4].TokenType);
        Assert.Equal("FROM", tokens[4].Value);
        Assert.Equal(SqlTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[6].TokenType);
        Assert.Equal("users", tokens[6].Value);
    }

    [Fact]
    public void TestSelectWithWhere()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT id, name FROM users WHERE age > 18");
        Assert.Equal(10, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[1].TokenType);
        Assert.Equal("id", tokens[1].Value);
        Assert.Equal(SqlTokenType.Comma, tokens[2].TokenType);
        Assert.Equal(SqlTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal("name", tokens[3].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[4].TokenType);
        Assert.Equal("FROM", tokens[4].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[5].TokenType);
        Assert.Equal("users", tokens[5].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[6].TokenType);
        Assert.Equal("WHERE", tokens[6].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[7].TokenType);
        Assert.Equal("age", tokens[7].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[8].TokenType);
        Assert.Equal(">", tokens[8].Value);
        Assert.Equal(SqlTokenType.Number, tokens[9].TokenType);
        Assert.Equal("18", tokens[9].Value);
    }

    [Fact]
    public void TestInsertStatement()
    {
        var tokens = TokenizeWithoutWhitespace("INSERT INTO users (id, name) VALUES (1, 'John')");
        Assert.Equal(14, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("INSERT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[1].TokenType);
        Assert.Equal("INTO", tokens[1].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("users", tokens[2].Value);
        Assert.Equal(SqlTokenType.OpenParenthesis, tokens[3].TokenType);
        Assert.Equal(SqlTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("id", tokens[4].Value);
        Assert.Equal(SqlTokenType.Comma, tokens[5].TokenType);
        Assert.Equal(SqlTokenType.Identifier, tokens[6].TokenType);
        Assert.Equal("name", tokens[6].Value);
        Assert.Equal(SqlTokenType.CloseParenthesis, tokens[7].TokenType);
        Assert.Equal(SqlTokenType.Keyword, tokens[8].TokenType);
        Assert.Equal("VALUES", tokens[8].Value);
        Assert.Equal(SqlTokenType.OpenParenthesis, tokens[9].TokenType);
        Assert.Equal(SqlTokenType.Number, tokens[10].TokenType);
        Assert.Equal("1", tokens[10].Value);
        Assert.Equal(SqlTokenType.Comma, tokens[11].TokenType);
        Assert.Equal(SqlTokenType.StringValue, tokens[12].TokenType);
        Assert.Equal("'John'", tokens[12].Value);
        Assert.Equal(SqlTokenType.CloseParenthesis, tokens[13].TokenType);
    }

    [Fact]
    public void TestUpdateStatement()
    {
        var tokens = TokenizeWithoutWhitespace("UPDATE users SET name = 'Jane' WHERE id = 1");
        Assert.Equal(10, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("UPDATE", tokens[0].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[1].TokenType);
        Assert.Equal("users", tokens[1].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("SET", tokens[2].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal("name", tokens[3].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[4].TokenType);
        Assert.Equal("=", tokens[4].Value);
        Assert.Equal(SqlTokenType.StringValue, tokens[5].TokenType);
        Assert.Equal("'Jane'", tokens[5].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[6].TokenType);
        Assert.Equal("WHERE", tokens[6].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[7].TokenType);
        Assert.Equal("id", tokens[7].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[8].TokenType);
        Assert.Equal("=", tokens[8].Value);
        Assert.Equal(SqlTokenType.Number, tokens[9].TokenType);
        Assert.Equal("1", tokens[9].Value);
    }

    [Fact]
    public void TestDeleteStatement()
    {
        var tokens = TokenizeWithoutWhitespace("DELETE FROM users WHERE id = 1");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("DELETE", tokens[0].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[1].TokenType);
        Assert.Equal("FROM", tokens[1].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("users", tokens[2].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[3].TokenType);
        Assert.Equal("WHERE", tokens[3].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("id", tokens[4].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[5].TokenType);
        Assert.Equal("=", tokens[5].Value);
        Assert.Equal(SqlTokenType.Number, tokens[6].TokenType);
        Assert.Equal("1", tokens[6].Value);
    }

    [Fact]
    public void TestStringLiteralsSingleQuotes()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT 'Hello World' FROM dual");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.StringValue, tokens[1].TokenType);
        Assert.Equal("'Hello World'", tokens[1].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("FROM", tokens[2].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal("dual", tokens[3].Value);
    }

    [Fact]
    public void TestStringLiteralsDoubleQuotes()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT \"Hello World\" FROM dual");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.StringValue, tokens[1].TokenType);
        Assert.Equal("\"Hello World\"", tokens[1].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("FROM", tokens[2].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal("dual", tokens[3].Value);
    }

    [Fact]
    public void TestNumbers()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT 123, 45.67, 0.5 FROM dual");
        Assert.Equal(8, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Number, tokens[1].TokenType);
        Assert.Equal("123", tokens[1].Value);
        Assert.Equal(SqlTokenType.Comma, tokens[2].TokenType);
        Assert.Equal(SqlTokenType.Number, tokens[3].TokenType);
        Assert.Equal("45.67", tokens[3].Value);
        Assert.Equal(SqlTokenType.Comma, tokens[4].TokenType);
        Assert.Equal(SqlTokenType.Number, tokens[5].TokenType);
        Assert.Equal("0.5", tokens[5].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[6].TokenType);
        Assert.Equal("FROM", tokens[6].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[7].TokenType);
        Assert.Equal("dual", tokens[7].Value);
    }

    [Fact]
    public void TestOperators()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT * FROM t WHERE a = 1 AND b <> 2 AND c < 3 AND d > 4 AND e <= 5 AND f >= 6");
        Assert.Equal(28, tokens.Count);
        // Checking key operators
        Assert.Equal(SqlTokenType.Operator, tokens[6].TokenType);
        Assert.Equal("=", tokens[6].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[8].TokenType);
        Assert.Equal("AND", tokens[8].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[10].TokenType);
        Assert.Equal("<>", tokens[10].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[14].TokenType);
        Assert.Equal("<", tokens[14].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[18].TokenType);
        Assert.Equal(">", tokens[18].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[22].TokenType);
        Assert.Equal("<=", tokens[22].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[26].TokenType);
        Assert.Equal(">=", tokens[26].Value);
    }

    [Fact]
    public void TestArithmeticOperators()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT a + b - c * d / e % f FROM t");
        Assert.Equal(14, tokens.Count);
        Assert.Equal(SqlTokenType.Identifier, tokens[1].TokenType);
        Assert.Equal("a", tokens[1].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[2].TokenType);
        Assert.Equal("+", tokens[2].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal("b", tokens[3].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[4].TokenType);
        Assert.Equal("-", tokens[4].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[6].TokenType);
        Assert.Equal("*", tokens[6].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[8].TokenType);
        Assert.Equal("/", tokens[8].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[10].TokenType);
        Assert.Equal("%", tokens[10].Value);
    }

    [Fact]
    public void TestLineComment()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT * FROM users -- This is a comment\nWHERE id = 1");
        Assert.Equal(9, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[1].TokenType);
        Assert.Equal("*", tokens[1].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("FROM", tokens[2].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal("users", tokens[3].Value);
        Assert.Equal(SqlTokenType.Comment, tokens[4].TokenType);
        Assert.Equal("-- This is a comment", tokens[4].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[5].TokenType);
        Assert.Equal("WHERE", tokens[5].Value);
    }

    [Fact]
    public void TestBlockComment()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT * /* This is a block comment */ FROM users");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[1].TokenType);
        Assert.Equal("*", tokens[1].Value);
        Assert.Equal(SqlTokenType.Comment, tokens[2].TokenType);
        Assert.Equal("/* This is a block comment */", tokens[2].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[3].TokenType);
        Assert.Equal("FROM", tokens[3].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("users", tokens[4].Value);
    }

    [Fact]
    public void TestPunctuation()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT id, name FROM users.accounts WHERE id = 1;");
        Assert.Equal(13, tokens.Count);
        Assert.Equal(SqlTokenType.Comma, tokens[2].TokenType);
        Assert.Equal(",", tokens[2].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[5].TokenType);
        Assert.Equal("users", tokens[5].Value);
        Assert.Equal(SqlTokenType.Dot, tokens[6].TokenType);
        Assert.Equal(".", tokens[6].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[7].TokenType);
        Assert.Equal("accounts", tokens[7].Value);
        Assert.Equal(SqlTokenType.SequenceTerminator, tokens[12].TokenType);
        Assert.Equal(";", tokens[12].Value);
    }

    [Fact]
    public void TestJoinQuery()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT u.name, o.amount FROM users u INNER JOIN orders o ON u.id = o.user_id");
        Assert.Equal(23, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[1].TokenType);
        Assert.Equal("u", tokens[1].Value);
        Assert.Equal(SqlTokenType.Dot, tokens[2].TokenType);
        Assert.Equal(SqlTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal("name", tokens[3].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[11].TokenType);
        Assert.Equal("INNER", tokens[11].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[12].TokenType);
        Assert.Equal("JOIN", tokens[12].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[15].TokenType);
        Assert.Equal("ON", tokens[15].Value);
    }

    [Fact]
    public void TestSubquery()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT * FROM users WHERE id IN (SELECT user_id FROM orders)");
        Assert.Equal(13, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[6].TokenType);
        Assert.Equal("IN", tokens[6].Value);
        Assert.Equal(SqlTokenType.OpenParenthesis, tokens[7].TokenType);
        Assert.Equal(SqlTokenType.Keyword, tokens[8].TokenType);
        Assert.Equal("SELECT", tokens[8].Value);
        Assert.Equal(SqlTokenType.CloseParenthesis, tokens[12].TokenType);
    }

    [Fact]
    public void TestComplexQueryWithMultipleJoins()
    {
        var sql = @"SELECT u.name, o.amount, p.product_name 
                    FROM users u 
                    INNER JOIN orders o ON u.id = o.user_id 
                    LEFT JOIN products p ON o.product_id = p.id 
                    WHERE u.age > 18 AND o.amount > 100 
                    ORDER BY u.name ASC";
        var tokens = TokenizeWithoutWhitespace(sql);
        
        // Check for key elements
        var keywords = tokens.Where(t => t.TokenType == SqlTokenType.Keyword).Select(t => t.Value.ToUpper()).ToList();
        Assert.Contains("SELECT", keywords);
        Assert.Contains("FROM", keywords);
        Assert.Contains("INNER", keywords);
        Assert.Contains("JOIN", keywords);
        Assert.Contains("LEFT", keywords);
        Assert.Contains("ON", keywords);
        Assert.Contains("WHERE", keywords);
        Assert.Contains("AND", keywords);
        Assert.Contains("ORDER", keywords);
        Assert.Contains("BY", keywords);
        Assert.Contains("ASC", keywords);
    }

    [Fact]
    public void TestStopDelimiter()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT * FROM users```MORE CONTENT", "```");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[1].TokenType);
        Assert.Equal("*", tokens[1].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("FROM", tokens[2].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal("users", tokens[3].Value);
    }

    [Fact]
    public void TestStopDelimiterWithScript()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT * FROM users</script>MORE CONTENT", "</script>");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[1].TokenType);
        Assert.Equal("*", tokens[1].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("FROM", tokens[2].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal("users", tokens[3].Value);
    }

    [Fact]
    public void TestCaseInsensitiveKeywords()
    {
        var tokens = TokenizeWithoutWhitespace("select * from users where id = 1");
        Assert.Equal(8, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("select", tokens[0].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("from", tokens[2].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[4].TokenType);
        Assert.Equal("where", tokens[4].Value);
    }

    [Fact]
    public void TestIdentifierVsKeyword()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT my_select FROM users");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[1].TokenType);
        Assert.Equal("my_select", tokens[1].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[2].TokenType);
        Assert.Equal("FROM", tokens[2].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[3].TokenType);
        Assert.Equal("users", tokens[3].Value);
    }

    [Fact]
    public void TestGroupByAndOrderBy()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT name, COUNT(*) FROM users GROUP BY name ORDER BY name DESC");
        Assert.Equal(16, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[9].TokenType);
        Assert.Equal("GROUP", tokens[9].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[10].TokenType);
        Assert.Equal("BY", tokens[10].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[12].TokenType);
        Assert.Equal("ORDER", tokens[12].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[13].TokenType);
        Assert.Equal("BY", tokens[13].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[15].TokenType);
        Assert.Equal("DESC", tokens[15].Value);
    }

    [Fact]
    public void TestCreateTable()
    {
        var sql = "CREATE TABLE users (id INT, name VARCHAR(100))";
        var tokens = TokenizeWithoutWhitespace(sql);
        
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("CREATE", tokens[0].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[1].TokenType);
        Assert.Equal("TABLE", tokens[1].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[2].TokenType);
        Assert.Equal("users", tokens[2].Value);
        Assert.Equal(SqlTokenType.OpenParenthesis, tokens[3].TokenType);
        Assert.Equal(SqlTokenType.Identifier, tokens[4].TokenType);
        Assert.Equal("id", tokens[4].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[5].TokenType);
        Assert.Equal("INT", tokens[5].Value);
    }

    [Fact]
    public void TestMultilineComment()
    {
        var sql = @"SELECT * 
        /* This is a 
        multiline comment */
        FROM users";
        var tokens = TokenizeWithoutWhitespace(sql);
        
        var comments = tokens.Where(t => t.TokenType == SqlTokenType.Comment).ToList();
        Assert.Single(comments);
        Assert.Contains("multiline", comments[0].Value);
    }

    [Fact]
    public void TestNullAndIs()
    {
        var tokens = TokenizeWithoutWhitespace("SELECT * FROM users WHERE name IS NULL");
        Assert.Equal(8, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[6].TokenType);
        Assert.Equal("IS", tokens[6].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[7].TokenType);
        Assert.Equal("NULL", tokens[7].Value);
    }

    [Fact]
    public void TestWhitespaceTokens()
    {
        var tokens = Tokenize("SELECT * FROM users");
        
        // Should have whitespace tokens between keywords/operators
        var whitespaceTokens = tokens.Where(t => t.TokenType == SqlTokenType.Whitespace).ToList();
        Assert.Equal(3, whitespaceTokens.Count);
        
        // Verify structure: SELECT <ws> * <ws> FROM <ws> users
        Assert.Equal(7, tokens.Count);
        Assert.Equal(SqlTokenType.Keyword, tokens[0].TokenType);
        Assert.Equal("SELECT", tokens[0].Value);
        Assert.Equal(SqlTokenType.Whitespace, tokens[1].TokenType);
        Assert.Equal(" ", tokens[1].Value);
        Assert.Equal(SqlTokenType.Operator, tokens[2].TokenType);
        Assert.Equal("*", tokens[2].Value);
        Assert.Equal(SqlTokenType.Whitespace, tokens[3].TokenType);
        Assert.Equal(" ", tokens[3].Value);
        Assert.Equal(SqlTokenType.Keyword, tokens[4].TokenType);
        Assert.Equal("FROM", tokens[4].Value);
        Assert.Equal(SqlTokenType.Whitespace, tokens[5].TokenType);
        Assert.Equal(" ", tokens[5].Value);
        Assert.Equal(SqlTokenType.Identifier, tokens[6].TokenType);
        Assert.Equal("users", tokens[6].Value);
    }

    [Fact]
    public void TestMultipleWhitespaceCharacters()
    {
        var tokens = Tokenize("SELECT  \t\n id");
        
        // Should have one whitespace token with multiple chars
        var whitespaceTokens = tokens.Where(t => t.TokenType == SqlTokenType.Whitespace).ToList();
        Assert.Single(whitespaceTokens);
        Assert.Equal("  \t\n ", whitespaceTokens[0].Value);
    }

    private static List<SqlToken> Tokenize(string input) => SqlTokenizer.Create().Parse(input);

    private static List<SqlToken> TokenizeWithoutWhitespace(string input)
    {
        var tokens = SqlTokenizer.Create().Parse(input);
        return tokens.Where(t => t.TokenType != SqlTokenType.Whitespace).ToList();
    }

    private static List<SqlToken> Tokenize(string input, string stopDelimiter)
    {
        var tokens = new List<SqlToken>();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream, Encoding.UTF8);
        SqlTokenizer.Create().ParseAsync(reader, stopDelimiter, token => tokens.Add(token)).GetAwaiter().GetResult();
        return tokens;
    }

    private static List<SqlToken> TokenizeWithoutWhitespace(string input, string stopDelimiter)
    {
        var tokens = Tokenize(input, stopDelimiter);
        return tokens.Where(t => t.TokenType != SqlTokenType.Whitespace).ToList();
    }

    [Fact]
    public async Task TestCancellation()
    {
        // Create a large SQL to parse
        var largeSql = "SELECT * FROM users WHERE " + string.Join(" OR ", Enumerable.Range(1, 1000).Select(i => $"id = {i}"));
        
        using var cts = new CancellationTokenSource();
        var tokens = new List<SqlToken>();
        int tokenCount = 0;
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(largeSql));
        
        // Cancel after a few tokens
        var parseTask = Task.Run(async () =>
        {
            await SqlTokenizer.Create().ParseAsync(stream, cts.Token, token =>
            {
                tokens.Add(token);
                tokenCount++;
                if (tokenCount == 20)
                {
                    cts.Cancel();
                }
            });
        }, TestContext.Current.CancellationToken);
        
        await parseTask;
        
        // Should have stopped early
        Assert.True(tokenCount < 1000, "Tokenization should have been cancelled");
    }
}
