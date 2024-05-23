using System.Text;
using System.Text.RegularExpressions;

namespace GUML;

public struct Token : IPosInfo
{
    public string Name { get; init; }
    public string Value { get; init; }
    public int Start { get; set; }
    public int End { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

    public override string ToString() => $"<Token {Name}: {Regex.Escape(Value)} start: {Start}, end: {End}, line: {Line}, column: {Column}>";
}

public class TokenizeException(string message, int line, int column) : Exception($"{message} at {line}:{column}.");

public interface ITokenize
{
    public int Index { get; }
    public string CodeString { get; }
    public char? Next();
    public char? Back();
}

public class TokenizeGenerator(List<(Func<string, ITokenize, string>, Func<ITokenize, string>)> specs)
    : ITokenize
{
    private readonly List<int> _lineStart = [0];

    private int _cacheIndex;
    private int _column;
    private int _line;

    public int Index { get; private set; }

    public string CodeString { get; private set; } = "";

    public char? Next()
    {
        if (Index >= CodeString.Length) return null;
        var ch = CodeString[Index];
        if (ch == '\n')
        {
            if (!_lineStart.Contains(Index + 1)) _lineStart.Add(Index + 1);
        }
        Index += 1;
        return ch;

    }

    public char? Back()
    {
        if (Index < 0) return null;
        Index -= 1;
        var ch = CodeString[Index];
        return ch;
    }

    public List<Token> Tokenize(string code)
    {
        var result = new List<Token>();
        if (specs.Count == 0)
        {
            return result;
        }

        CodeString = code;
        _line = 1;
        _column = 0;
        Index = 0;
        while (Index < CodeString.Length)
        {
            var patternString = "";
            var index = 0;
            while (index < specs.Count)
            {
                var (nameFunc, patternFunc) = specs[index];
                var start = Index;
                patternString = patternFunc(this);
                if (patternString != "")
                {
                    index = 0;
                    _cacheIndex = Index;
                    var name = nameFunc(patternString, this);
                    if (name == "")
                    {
                        continue;
                    }
                    SetLineFormIndex(start);
                    result.Add(new Token
                    {
                        Name = name,
                        Value = patternString,
                        Start = start,
                        End = Index,
                        Line = _line,
                        Column = _column
                    });
                }
                else
                {
                    index += 1;
                    ReBack();
                }
            }

            if (Index >= CodeString.Length || !string.IsNullOrEmpty(patternString)) continue;
            var errorString = code.Substring(Index, 1);
            throw new TokenizeException($"Unexpected token '{errorString}'", _line, _column + 1);
        }
        result.Add(new Token
        {
            Name = "eof",
            Value = "eof",
            Start = CodeString.Length + 1,
            End = Index - 1,
            Line = _lineStart.Count + 1,
            Column = 0
        });
        return result;
    }

    private void ReBack()
    {
        Index = _cacheIndex;
    }

    private void SetLineFormIndex(int start)
    {
        for (var index = _lineStart.Count - 1; index >= 0; index--)
        {
            if (_lineStart[index] > start) continue;
            _line = index + 1;
            _column = start - _lineStart[index] + 1;
            return;
        }
    }

    public static Func<ITokenize, string> ValuePattern(string pattern) => tokenize =>
    {
        return pattern.Any(ch => tokenize.Next() != ch) ? "" : pattern;
    };

    public static Func<ITokenize, string> ValuesPattern(string[] patterns) =>
        tokenize =>
        {
            var result = "";
            foreach (var pattern in patterns)
            {
                if (pattern.All(ch => tokenize.Next() == ch))
                {
                    result = pattern;
                }
            }
            return result;
        };

    public static Func<ITokenize, string> CharsPattern(char[] chars) =>
        tokenize =>
        {
            var ch = tokenize.Next();
            var result = "";
            if (ch != null && chars.Contains(ch.Value))
            {
                result += ch;
            }
            else
            {
                return "";
            }

            return result;
        };

    public static Func<ITokenize, string> CharPattern(char patternChar) =>
        tokenize =>
        {
            var ch = tokenize.Next();
            return patternChar == ch ? patternChar.ToString() : "";
        };

    public static Func<ITokenize, string> CommentPattern(string comment) =>
        tokenize =>
        {
            if (comment.Length == 0)
            {
                throw new ArgumentException("Comment string should not be empty.");
            }

            var commentStr = new StringBuilder("");
            char? currentChar = null;
            foreach (var ch in comment)
            {
                currentChar = tokenize.Next();
                if (currentChar == null || currentChar.Value != ch)
                {
                    return "";
                }

                commentStr.Append(currentChar);
            }

            while (currentChar != null && currentChar.Value != '\n')
            {
                currentChar = tokenize.Next();
                commentStr.Append(currentChar);
            }

            return commentStr.ToString();
        };

    public static Func<ITokenize, string> StringPattern() =>
        tokenize =>
        {
            var result = new StringBuilder();
            var quoteChar = tokenize.Next();
            // 如果不是 ' 或 "
            if (quoteChar != '\'' && quoteChar != '"')
            {
                return "";
            }

            var isEscape = false;
            var currentChar = tokenize.Next();
            while (currentChar != null)
            {
                if (currentChar == '\\')
                {
                    isEscape = !isEscape;
                }

                // 如果遇到结束的 ' 或 "
                if (currentChar == quoteChar && !isEscape)
                {
                    return result.ToString();
                }

                // 如果遇到换行，则字符串未闭合
                if (currentChar == '\n')
                {
                    return "";
                }

                result.Append(currentChar);
                currentChar = tokenize.Next();
            }

            return result.ToString();
        };

    public static Func<ITokenize, string> NumberPattern(bool hasDecimal = false, bool hasScientificNotation = false) =>
        tokenize =>
        {
            var result = "";
            var isNumber = true;
            var hasDot = false;
            var decimalStr = "";
            while (true)
            {
                var ch = tokenize.Next();
                switch (ch)
                {
                    case '.':
                        // 如果已经有小数点或小数点前位数为0，则标记为非法
                        if (hasDot || result.Length == 0)
                        {
                            isNumber = false;
                        }
                        else
                        {
                            hasDot = true;
                        }

                        break;
                    default:
                        // 遇到非数字字符，结束循环
                        if (ch == null || !char.IsDigit(ch.Value))
                        {
                            tokenize.Back();
                            goto endLoop;
                        }

                        if (hasDot)
                        {
                            decimalStr += ch;
                        }

                        break;
                }

                if (!isNumber)
                {
                    goto endLoop;
                }

                result += ch;
            }

            endLoop:

            if (hasDecimal)
            {
                if (!hasDot || decimalStr == "")
                {
                    isNumber = false;
                }
            }
            else
            {
                if (hasDot)
                {
                    if (decimalStr == "")
                    {
                        tokenize.Back();
                        result = result[..^1];
                    }
                    else
                    {
                        isNumber = false;
                    }
                }
            }

            if (string.IsNullOrEmpty(result) || !isNumber)
            {
                return "";
            }

            return result;
        };
}
