using System.Text;

namespace DroneFleet.App.ConsoleApp;

/// <summary>
/// Tokenises command lines entered by the user.
/// </summary>
internal static class CommandTokenizer
{
    /// <summary>
    /// Splits a command line into tokens, respecting quotes.
    /// </summary>
    public static IReadOnlyList<string> Tokenize(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var result = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        for (int index = 0; index < input.Length; index++)
        {
            var ch = input[index];

            if (ch == '\"')
            {
                if (inQuotes && index + 1 < input.Length && input[index + 1] == '\"')
                {
                    builder.Append('\"');
                    index++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (builder.Length > 0)
                {
                    result.Add(builder.ToString());
                    builder.Clear();
                }

                continue;
            }

            builder.Append(ch);
        }

        if (builder.Length > 0)
        {
            result.Add(builder.ToString());
        }

        return result;
    }
}
