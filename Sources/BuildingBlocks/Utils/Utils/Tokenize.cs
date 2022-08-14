namespace Pulsar.BuildingBlocks.Utils;

public static class Tokenize
{
    static HashSet<string> stopWords = new HashSet<string>()
    {
        "de", "da", "para", "com", "a", "o", "e", "à", "que", "em", "um", "não", "uma", "os", "as", "no", "se", "na", "por", "das", "dos", "ao", "ou", "pelo", "pela", "sem"
    };

    static string from = "áéíóúàèìòùâêîôûãõüç";
    static string to = "aeiouaeiouaeiouaouc";
    static Dictionary<char, char> fromTo = new Dictionary<char, char>();

    static Tokenize()
    {
        for (int i = 0; i < from.Length; i++)
        {
            fromTo[from[i]] = to[i];
        }
    }

    public static string? Perform(string? s)
    {
        if (s == null)
            return null;
        s = s.Trim();
        if (s == string.Empty)
            return string.Empty;
        var n = Normalize(s);
        var words = n.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(x => !stopWords.Contains(x)).ToList();
        var joined = string.Join(';', words.SelectMany(w => Break(w)));
        return joined;
    }

    private static string Normalize(string s)
    {
        return new string(s.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c)).Select(c => ReplaceChar(char.ToLowerInvariant(c))).ToArray());
    }

    private static IEnumerable<string> Break(string s)
    {
        int len = 1;
        var pn = s.Any(c => char.IsDigit(c));
        if (s.Length <= Math.Max(3, len))
        {
            yield return s;
            yield break;
        }

        int i = 0;
        while (i <= s.Length)
        {
            if (i < Math.Max(3, len))
            {
                i++;
                continue;
            }
            if (pn || len == 1)
            {
                yield return s.Substring(0, i);
            }
            else if ((i % len == 0 && i > 0) || (i == s.Length))
                yield return s.Substring(0, i);
            i++;
        }

    }

    private static char ReplaceChar(char c)
    {
        if (char.IsWhiteSpace(c) || char.IsPunctuation(c))
            return ' ';
        if (fromTo.ContainsKey(c))
            return fromTo[c];

        return c;
    }
}
