namespace LoschScript.Lowering;

internal static class Extensions
{
    public static string ReplaceFirst(this string str, string search, string replace)
    {
        int pos = str.IndexOf(search);
        if (pos < 0)
            return str;

        return str[..pos] + replace + str[(pos + search.Length)..];
    }
}