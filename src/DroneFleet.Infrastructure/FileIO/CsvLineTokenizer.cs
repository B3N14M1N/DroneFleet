using System.Text;

namespace DroneFleet.Infrastructure.FileIO;

/// <summary>
/// Provides CSV tokenisation utilities.
/// </summary>
internal static class CsvLineTokenizer
{
    /// <summary>
    /// Splits a CSV line into fields supporting quoted values.
    /// </summary>
    public static IReadOnlyList<string> Tokenize(string line)
    {
        ArgumentNullException.ThrowIfNull(line);

        var builder = new StringBuilder();
        var values = new List<string>();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    builder.Append('\"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                values.Add(builder.ToString().Trim());
                builder.Clear();
                continue;
            }

            builder.Append(ch);
        }

        values.Add(builder.ToString().Trim());
        return values;
    }
}
