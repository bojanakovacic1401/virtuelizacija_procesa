using System.Collections.Generic;
using System.Text;

namespace EVCharging.Client.Parsing
{
    public static class CsvLineSplitter
    {
        public static string[] Split(string line)
        {
            char delimiter = DetectDelimiter(line);
            var result = new List<string>();
            var current = new StringBuilder();
            bool insideQuotes = false;

            for (int i = 0; i < (line == null ? 0 : line.Length); i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }
                }
                else if (c == delimiter && !insideQuotes)
                {
                    result.Add(current.ToString());
                    current.Length = 0;
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        private static char DetectDelimiter(string line)
        {
            if (line == null)
            {
                return ',';
            }

            int commas = Count(line, ',');
            int tabs = Count(line, '\t');
            int semicolons = Count(line, ';');

            if (tabs > commas && tabs > semicolons)
            {
                return '\t';
            }

            if (semicolons > commas && semicolons > tabs)
            {
                return ';';
            }

            return ',';
        }

        private static int Count(string text, char searched)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (c == searched)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
