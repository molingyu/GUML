namespace GUML;

public class GumlParserException(string msg, IPosInfo posInfo) : Exception($"{msg}(at {posInfo.Line}:{posInfo.Column})");

public class GumlParser
{
    private static readonly string[] SKeywords = ["each", "using", "import", "import_top", "resource", "vec2"];

    private static readonly string[] SOperators = ["!=", "<=", ">=", "==", "!", "||", "&&", "+", "-", "*", "/", "%", "^"];

    private static readonly string[] SSpecials = [",", ".", "|", ":=", ":", "(", "[", "{", "}", "]", ")"];

    private readonly List<string> _componentParamKey = ["signal_name", "name"];
    private readonly List<string> _ref =
    [
        "global_ref",
        "alias_ref",
        "name",
        "."
    ];
    private readonly List<string> _valueEnd = [",", "|"];

    private readonly TokenizeGenerator _tokenizer;
    private Token? _tokenCache;
    private int _tokenIdCache;
    private int _index;

    private GumlDoc _gumlDoc;
    private List<Token> _tokens = null!;
    private Stack<GumlSyntaxNode> _nodeStack = null!;
    
    public GumlParser()
    {
        Converters = new List<IConverter>();
        _tokenizer = InitTokenizer();
    }

    public List<IConverter> Converters { get; }
    public string Code { get; private set; } = "";

    private static TokenizeGenerator InitTokenizer()
    {
        var patternWhitespace = TokenizeGenerator.CharsPattern(['\t', (char)32]);
        var patternNewline = TokenizeGenerator.CharsPattern(['\r', '\n']);
        var patternComment = TokenizeGenerator.CommentPattern("//");
        var patternString = TokenizeGenerator.StringPattern();
        var patternFloat = TokenizeGenerator.NumberPattern(true);
        var patternInteger = TokenizeGenerator.NumberPattern();
        var patternNull = TokenizeGenerator.ValuePattern("Null");
        List<(Func<string, ITokenize, string>, Func<ITokenize, string>)> rule =
        [
            ((_, _) => "", patternWhitespace),
            ((_, _) => "", patternNewline),
            ((_, _) => "", patternComment),
            ((_, _) => "string", patternString),
            ((_, _) => "float", patternFloat),
            ((_, _) => "integer", patternInteger),
            ((_, _) => "null", patternNull),
            ((_, _) => "boolean", TokenizeGenerator.ValuePattern("false")),
            ((_, _) => "boolean", TokenizeGenerator.ValuePattern("true")),
            (CheckName, PatternName)
        ];
        SSpecials.ToList().ForEach(special => rule.Add(((_, _) => special, TokenizeGenerator.ValuePattern(special))));
        SOperators.ToList().ForEach(op => rule.Add(((_, _) => "operator", TokenizeGenerator.ValuePattern(op))));
        return new TokenizeGenerator(rule);
    }

    private static string CheckName(string text, ITokenize tokenize)
    {
        if (SKeywords.Contains(text))
        {
            return text;
        }

        switch (text.Length)
        {
            case > 1 when char.IsUpper(text[0]):
            {
                if (tokenize.Index - text.Length <= 0) return "component";
                var lastCh = tokenize.CodeString[tokenize.Index - text.Length - 1];
                return lastCh == '.' ? "name" : "component";
            }
            case > 1 when text[0] == '$':
                return "global_ref";
            case > 1 when text[0] == '@':
                return "alias_ref";
            case > 1 when text[0] == '#':
                return "signal_name";
            default:
                return "name";
        }
    }

    private static string PatternName(ITokenize tokenize)
    {
        var currentChar = tokenize.Next();
        var result = "" + currentChar;
        if (currentChar == null ||
            (!char.IsLetter(currentChar.Value) && currentChar.Value is not ('$' or '@' or '_' or '#')))
        {
            return "";
        }

        currentChar = tokenize.Next();
        while (currentChar != null && (char.IsLetterOrDigit(currentChar.Value) || currentChar == '_'))
        {
            result += currentChar;
            currentChar = tokenize.Next();
        }

        tokenize.Back();
        return string.IsNullOrEmpty(result) ? "" : result;
    }

    public void WithConverter(IConverter converter) => Converters.Add(converter);

    public GumlDoc Parse(string code)
    {
        Code = code;
        _index = 0;
        _tokenIdCache = 0;
        _nodeStack = new Stack<GumlSyntaxNode>();
        _tokens = _tokenizer.Tokenize(Code);
        _gumlDoc = new GumlDoc();
        ParseImport();
        if (CurrentToken().Name == "eof") throw new GumlParserException("Must has root component.", CurrentToken());
        ParseComponent();
        return _gumlDoc;
    }

    private Token CurrentToken(string? name = null)
    {
        if (name != null && _tokenCache?.Name != name)
        {
            var errorMsg = $"Unexpected symbol type, expected '{name}' but currently is '{_tokenCache?.Name}'.";
            throw new GumlParserException(errorMsg, _tokenCache!);
        }
        if (_tokenCache is not null && _tokenIdCache == _index) return _tokenCache.Value;
        _tokenCache = (Token)ApplyConverters(_tokens[_index], ConverterType.Token);
        _tokenIdCache = _index;
        return _tokenCache.Value;
    }

    private Token NextToken(string? name = null)
    {
        _index += 1;
        var newToken = CurrentToken();
        if (name == null || newToken.Name == name) return newToken;
        var errorMsg = $"Unexpected symbol type, expected '{name}' but currently is '{newToken.Name}'.";
        throw new GumlParserException(errorMsg, newToken);

    }

    private static GumlParserException UnexpectedException(Token token) => 
        new ($"Unexpected symbol '{token.Value}'.", token);

    private void ThrowEofException()
    {
        if (CurrentToken().Name == "eof") throw UnexpectedException(CurrentToken());
    }

    private void ParseImport()
    {
        if (CurrentToken().Name != "import" && CurrentToken().Name != "import_top") return;
        while (CurrentToken().Name == "import" ||  CurrentToken().Name != "import_top")
        {
            var isTop = CurrentToken().Name == "import_top";
            ThrowEofException();
            NextToken();
            if (CurrentToken().Name != "string") throw UnexpectedException(CurrentToken());
            var import = CurrentToken().Value;
            if (!_gumlDoc.Imports.TryAdd(import, isTop))
                throw new GumlParserException($"Module '{import}' is already imported.", CurrentToken());
            NextToken();
        }
    }
    private void ParseComponent(GumlSyntaxNode? parent = null)
    {
        var aliasName = "";
        if (CurrentToken().Name == "alias_ref")
        {
            aliasName = CurrentToken().Value;
            NextToken(":");
            NextToken();
        }
        var componentNode = CurrentToken("component");
        _nodeStack.Push(new GumlSyntaxNode
        {
            Name = componentNode.Value,
            Start = componentNode.Start,
            Line = componentNode.Line,
            Column = componentNode.Column
        });
        if (aliasName != "")
        {
            _gumlDoc.LocalAlias.Add(aliasName, _nodeStack.Peek());
        }
        NextToken("{");
        NextToken();
        ParseComponentBody();
        var node = (GumlSyntaxNode)ApplyConverters(_nodeStack.Pop(), ConverterType.Component);
        parent?.Children.Add(node);

        node.End = CurrentToken("}").End;
        NextToken();
        if (_nodeStack.Count != 0)
        {
            return;
        }
        _gumlDoc.RootNode = node;
        if (CurrentToken().Name != "eof") throw UnexpectedException(CurrentToken());
    }

    private void ParseComponentBody()
    {
        while (CurrentToken().Name != "}")
        {

            ThrowEofException();
            switch (CurrentToken().Name)
            {
                case "component":
                case "alias_ref":
                    ParseComponent(_nodeStack.Peek());
                    break;
                case "name":
                case "signal_name":
                    ParseKeyValuePair();
                    break;
                case "each":
                    ParseEach();
                    break;
                default:
                    throw UnexpectedException(CurrentToken());
            }
        }
    }

    private void ParseKeyValuePair(GumlValueNode? gumlValueNode = null)
    {
        while (_componentParamKey.Contains(CurrentToken().Name))
        {
            ThrowEofException();
            var key = (string)ApplyConverters(CurrentToken().Value, ConverterType.KeyName);
            var isBind = false;
            if (gumlValueNode != null)
            {
                NextToken(":");
            }
            else
            {
                var bindingSymbol = NextToken();
                isBind = bindingSymbol.Value switch
                {
                    ":" => false,
                    ":=" => true,
                    _ => throw UnexpectedException(bindingSymbol)
                };
            }

            NextToken();
            var valueNode = (GumlExprNode)ApplyConverters(ParseValue(), ConverterType.Value);
            if (gumlValueNode != null)
            {
                gumlValueNode.ObjectValue!.Add(key, valueNode);
            }
            else
            {
                if (key[0] == '#')
                {
                    var signalName = (string)ApplyConverters(key.Substring(1, key.Length - 1),
                        ConverterType.KeyName);
                    _nodeStack.Peek().Signals.Add(signalName, ((GumlValueNode)valueNode).StringValue);
                }
                else
                {
                    _nodeStack.Peek().Properties.Add(key, (isBind, valueNode));
                }

            }

            if (CurrentToken().Name == "}") return;
            NextToken();
        }
    }

    private void ParseEach()
    {
        var startToken = CurrentToken("each");
        NextToken();
        var dataSource = ParseRef(new GumlValueNode
        {
            Start = CurrentToken().Start,
            End = CurrentToken().End,
            Line = CurrentToken().Line,
            Column = CurrentToken().Column
        });
        NextToken("using");
        var controllerType = NextToken("component").Value;
        NextToken("{");
        NextToken("|");
        var indexName = NextToken("name").Value;
        NextToken(",");
        var valueName = NextToken("name").Value;
        NextToken("|");
        var each = new GumlEachNode
        {
            Start = startToken.Start,
            Line = startToken.Line,
            Column = startToken.Column,
            DataSource = dataSource,
            ControllerType = controllerType,
            IndexName = indexName,
            ValueName = valueName,
            Name = "each"
        };
        _nodeStack.Push(each);
        NextToken();
        ParseComponentBody();
        CurrentToken("}");
        each.End = CurrentToken().End;
        _nodeStack.Pop();
        _nodeStack.Peek().EachNodes.Add(each);
        NextToken();
    }

    private GumlExprNode ParseValue()
    {
        Stack<GumlExprNode?> opNodeStack = new();
        opNodeStack.Push(null);
        var lastIsValue = false;
        while (!_valueEnd.Contains(CurrentToken().Value))
        {
            ThrowEofException();
            var current = CurrentToken();
            switch (current.Name)
            {
                case "operator":
                    var newOp = ParseOperator(!lastIsValue && PrefixOpNode.OpPrecedence.ContainsKey(current.Value));

                    switch (opNodeStack.Peek())
                    {
                        case GumlValueNode:
                            switch (newOp)
                            {
                                case PrefixOpNode:
                                    newOp.Right = opNodeStack.Pop() ?? throw new InvalidOperationException();
                                    break;
                                case InfixOpNode newInfixOp:
                                    newInfixOp.Left = opNodeStack.Pop() ?? throw new InvalidOperationException();
                                    break;
                            }

                            opNodeStack.Push(newOp);
                            break;
                        case null:
                            opNodeStack.Push(newOp);
                            break;
                        default:
                            opNodeStack.Push(ComputePriority(opNodeStack.Pop() as GumlOpNode ?? throw new InvalidOperationException(), newOp));
                            break;
                    }
                    lastIsValue = false;
                    break;
                case "(":
                    if (lastIsValue) throw new GumlParserException("", CurrentToken());
                    opNodeStack.Push(null);
                    break;
                case ")":
                    var popNode = opNodeStack.Pop();
                    if (popNode is GumlValueNode)
                    {
                        opNodeStack.Push(popNode);
                        goto LoopEnd;
                    }
                    var precedenceNode = (GumlOpNode)popNode!;
                    precedenceNode.FirstPrecedence = true;
                    opNodeStack.Pop();
                    if (opNodeStack.Peek() is GumlOpNode opNode)
                        opNode.Right = precedenceNode;
                    else
                        opNodeStack.Push(precedenceNode);
                    lastIsValue = false;
                    break;
                default:
                    // if (lastIsValue) throw UnexpectedException(CurrentToken());
                    var valueNode = new GumlValueNode
                    {
                        Start = current.Start, End = current.End, Line = current.Line, Column = current.Column
                    };
                    valueNode = GetRmlValueNode(current, valueNode);
                    if (opNodeStack.Peek() == null) opNodeStack.Push(valueNode);
                    if (opNodeStack.Peek() is GumlOpNode op) op.Right = valueNode;
                    lastIsValue = true;
                    break;
            }
            if (CurrentToken().Name == "}") break;
            if (CurrentToken().Name != ",") NextToken();
            if (CurrentToken().Name == "}")  break;
        }
        LoopEnd:
        var value = GetRoot(opNodeStack.Pop()!);
        value.End = CurrentToken().Start - 1;
        return value;
    }

    private static GumlExprNode GetRoot(GumlExprNode node)
    {
        if (node is not GumlOpNode opNode) return node;
        var current = opNode;
        while (current.Parent != null)
        {
            current = current.Parent;
        }
        return current;
    }

    private GumlOpNode ParseOperator(bool isPrefix)
    {
        GumlOpNode opNode;
        if (!isPrefix && InfixOpNode.OpPrecedence.ContainsKey(CurrentToken().Value) )
        {
            opNode = new InfixOpNode
            {
                Start = CurrentToken().Start,
                End = CurrentToken().End,
                Line = CurrentToken().Line,
                Column = CurrentToken().Column,
                Op = CurrentToken().Value
            };
        }
        else if (PrefixOpNode.OpPrecedence.ContainsKey(CurrentToken().Value))
        {
            opNode = new PrefixOpNode
            {
                Start = CurrentToken().Start,
                End = CurrentToken().End,
                Line = CurrentToken().Line,
                Column = CurrentToken().Column,
                Op = CurrentToken().Value
            };
        }
        else
        {
            throw UnexpectedException(CurrentToken());
        }

        return opNode;
    }

    private GumlValueNode ParseRef(GumlValueNode valueNode)
    {
        var currentNode = valueNode;
        while (_ref.Contains(CurrentToken().Name) || CurrentToken().Name == ".")
        {
            ThrowEofException();

            switch (CurrentToken().Name)
            {
                case "global_ref":
                    currentNode.RefType = RefType.GlobalRef;
                    currentNode.RefName = CurrentToken().Value;
                    break;
                case "alias_ref":
                    currentNode.RefType = RefType.LocalAliasRef;
                    currentNode.RefName = CurrentToken().Value;
                    break;
                case "name":
                    currentNode.RefType = RefType.LocalRef;
                    currentNode.RefName = CurrentToken().Value;
                    break;
                case ".":
                    NextToken("name");
                    currentNode = new GumlValueNode
                    {
                        Start = CurrentToken().Start,
                        End = CurrentToken().End,
                        Line = CurrentToken().Line,
                        Column = CurrentToken().Column,
                        ValueType = GumlValueType.Ref,
                        RefType = RefType.PropertyRef,
                        RefName = CurrentToken().Value,
                        RefNode = currentNode
                    };
                    break;
            }
            NextToken();
            if (CurrentToken().Name == "}")  break;
        }
        return currentNode;
    }

    private GumlValueNode ParseObject(GumlValueNode valueNode)
    {
        CurrentToken("{");
        NextToken();
        valueNode.ObjectValue = new Dictionary<string, GumlExprNode>();
        ParseKeyValuePair(valueNode);
        CurrentToken("}");
        NextToken();
        return valueNode;
    }

    private GumlValueNode ParseResource(GumlValueNode valueNode)
    {
        NextToken("(");
        valueNode.ResourceNode = ParseValue();
        CurrentToken(")");
        NextToken();
        return valueNode;
    }
    private GumlValueNode ParseVector2(GumlValueNode valueNode)
    {
        NextToken("(");
        valueNode.Vector2XNode = ParseValue();
        CurrentToken(",");
        NextToken();
        valueNode.Vector2YNode = ParseValue();
        CurrentToken(")");
        NextToken();
        return valueNode;
    }
    private GumlValueNode GetRmlValueNode(Token current, GumlValueNode valueNode)
    {
        switch (current.Name)
        {
            case "string":
                valueNode.ValueType = GumlValueType.String;
                valueNode.StringValue = current.Value;
                break;
            case "float":
                valueNode.ValueType = GumlValueType.Float;
                valueNode.FloatValue = float.Parse(current.Value);
                break;
            case "integer":
                valueNode.ValueType = GumlValueType.Int;
                valueNode.IntValue = int.Parse(current.Value);
                break;
            case "boolean":
                valueNode.ValueType = GumlValueType.Boolean;
                valueNode.BooleanValue = bool.Parse(current.Value);
                break;
            case "null":
                valueNode.ValueType = GumlValueType.Null;
                break;
            case "resource":
                valueNode.ValueType = GumlValueType.Vector2;
                valueNode = ParseResource(valueNode);
                break;
            case "vec2":
                valueNode.ValueType = GumlValueType.Vector2;
                valueNode = ParseVector2(valueNode);
                break;
            case "{":
                valueNode.ValueType = GumlValueType.Object;
                valueNode = ParseObject(valueNode);
                break;
            case "global_ref":
            case "alias_ref":
            case "name":
                valueNode.ValueType = GumlValueType.Ref;
                valueNode = ParseRef(valueNode);
                break;
        }
        return valueNode;
    }
    
    // TODO error
    private GumlOpNode ComputePriority(GumlOpNode oldOp, GumlOpNode newOp)
    {
        var currentOp = oldOp;
        var newPrecedence = newOp is PrefixOpNode
            ? PrefixOpNode.OpPrecedence[newOp.Op] : InfixOpNode.OpPrecedence[newOp.Op];
        while (currentOp is PrefixOpNode ||
               (!currentOp.FirstPrecedence && InfixOpNode.OpPrecedence[currentOp.Op] > newPrecedence))
        {
            if (currentOp.Parent == null) break;
            currentOp = currentOp.Parent;
        }
        var temp = currentOp.Right;
        currentOp.Right = newOp;
        if (newOp is not InfixOpNode newInfixOp) return newOp;
        if (temp != null) newInfixOp.Left = temp;
        else
        if (temp != null) newOp.Right = temp;
        return newOp;
    }

    private object ApplyConverters(object node, ConverterType converterType)
    {
        var result = node;
        Converters.Where(converter => converter.ConverterType == converterType).ToList().ForEach(converter =>
        {
            result = converter.Convert(result);
        });
        return result;
    }
}
