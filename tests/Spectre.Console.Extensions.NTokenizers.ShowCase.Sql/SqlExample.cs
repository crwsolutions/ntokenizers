namespace Spectre.Console.Extensions.NTokenizers.ShowCase.Sql;

internal static class SqlExample
{
    internal static string GetSampleSql() =>
        """
        SELECT name, active FROM users
        WHERE id = 4821;
        """;
}
