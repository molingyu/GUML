using System.Text.RegularExpressions;

namespace GUML;

public partial class KeyConverter : IConverter
{
    public ConverterType ConverterType => ConverterType.KeyName;

    public object Convert(object source)
    {
        if (source is string keyName)
        {
            return ToPascalCase(keyName);
        }

        throw new Exception("");
    }
    
    public static string ToPascalCase(string str)
    {
        var text = MyRegex().Replace(str, match => match.Groups["char"].Value.ToUpperInvariant());
        return char.ToUpperInvariant(text[0]) + text[1..];
    }

    [GeneratedRegex("([_\\-])(?<char>[a-z])", RegexOptions.IgnoreCase, "zh-CN")]
    private static partial Regex MyRegex();
}
