using System.Collections;
using System.ComponentModel;
using Godot;

namespace GUML;

public static class GumlRenderer
{
    private static readonly Dictionary<string, GumlExprNode> SBindingExprCache = new();
    private static GumlDoc _sGumlDoc;
    private static GuiController? _sController;
    private static Control? _sBindObj;
    private static Stack<Dictionary<string, object>> _sLocalStack = new ();

    public static void Render(GumlDoc gumlDoc, GuiController controller, Node rootNode, string dir)
    {
        ReinitializeRender();
        _sGumlDoc = gumlDoc;
        _sController = controller;
        Guml.GlobalRefs["$controller"] = _sController;
        var component = CreateComponent(_sGumlDoc.RootNode);
        _sController.RootNode = component;
        RenderImports(controller, rootNode, component, dir);
        rootNode.AddChild(component);
    }

    private static void RenderImports(GuiController controller, Node rootNode, Node currentNode, string dir)
    {
        var cacheDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(dir);
        foreach (var gumlDocImport in _sGumlDoc.Imports)
        {
            var importPath = $"{gumlDocImport.Key}.guml";
            var controllerName = $"{KeyConverter.ToPascalCase(Path.GetFileNameWithoutExtension(importPath))}Controller";
            var importController = Guml.LoadGuml(gumlDocImport.Value ? rootNode : currentNode, importPath);
            if (gumlDocImport.Value) Guml.TopControllers.Add(controllerName, importController);
            SetObjProperty(controller, controllerName, importController);
        }
        Directory.SetCurrentDirectory(cacheDirectory);
    }
    private static void SetObjProperty<T>(object obj, string key, T value)
    {
        var propertyInfo = obj.GetType().GetProperty(key);
        
        if (propertyInfo != null)
        {
            var propertyType = propertyInfo.PropertyType;
            if (propertyType == typeof(string))
            {
                propertyInfo.SetValue(obj, value?.ToString());
            }
            else
            {
                propertyInfo.SetValue(obj, value);
            }
            
        }
        else
        {
            throw new Exception($"Property '{key}' not find on object '{obj}'!");
        }

    }

    private static void ReinitializeRender()
    {
        _sController = null;
        _sLocalStack = new Stack<Dictionary<string, object>>();
    }

    private static Control CreateComponent(GumlSyntaxNode node)
    {
        var guiNode = FindComponent(node.Name);

        if (_sGumlDoc.LocalAlias.ContainsValue(node))
        {
            var name = GetKeyByValue(_sGumlDoc.LocalAlias, node);
            _sController?.NamedNode.Add(name.Substring(1, name.Length - 1), guiNode);
        }

        #region Properties
        var type = guiNode.GetType();
        object? value;
        node.Properties.Keys.ToList().ForEach(propertyName =>
        {
            if (propertyName == "ThemeOverrides")
            {
                value = ExprEval(node.Properties[propertyName].Item2);
                if (value is not Dictionary<string, object> objValue) throw new Exception("The ThemeOverrides type error.");
                foreach (var keyValuePair in objValue)
                {
                    SetThemeOverride(guiNode, KeyConverter.FromCamelCase(keyValuePair.Key), keyValuePair.Value);
                }
            }
            else
            {
                var bingType = node.Properties[propertyName].Item1;
                var propertyNode = node.Properties[propertyName].Item2;
                if (bingType)
                {
                    var cacheKey = $"{guiNode.GetHashCode()}_{propertyName}";
                    SBindingExprCache.Add(cacheKey, propertyNode);
                    _sBindObj = guiNode;
                    value = ExprEval(propertyNode, cacheKey);
                }
                else
                {
                    value = ExprEval(propertyNode);
                    _sBindObj = null;
                }
                if (value is Dictionary<string, object> objValue)
                {
                    var propertyInfo = type.GetProperty(propertyName);
                    if (propertyInfo?.GetValue(guiNode) == null)
                    {
                        SetObjProperty(guiNode, propertyName, Activator.CreateInstance(propertyInfo!.GetType()));
                    }
                    var obj = propertyInfo.GetValue(guiNode);
                    if (obj != null)
                    {
                        foreach (var keyValuePair in objValue)
                        {
                            SetObjProperty(obj, keyValuePair.Key, keyValuePair.Value);
                        }
                    }
                }
                SetObjProperty(guiNode, propertyName, value);
            }
        });
        #endregion

        node.Children.ForEach(child => { guiNode.AddChild(CreateComponent(child)); });

        #region EachNode
        node.EachNodes.ForEach(eachNode =>
        {
            var dataSource = (IList)ExprEval(eachNode.DataSource)!;
            _sLocalStack.Push(new Dictionary<string, object>());
            var index = 0;
            foreach (var obj in dataSource)
            {
                _sLocalStack.Peek()[eachNode.IndexName] = index;
                _sLocalStack.Peek()[eachNode.ValueName] = obj;
                eachNode.Children.ForEach(child =>
                {
                    guiNode.AddChild(CreateComponent(child));
                });
                index += 1;
            }

            var notifyList = (INotifyListChanged)dataSource;
            notifyList.ListChanged += (_, remove, obj) =>
            {
                if (remove)
                    guiNode.RemoveChild(guiNode.GetChildren()[dataSource.IndexOf(obj)]);
                else
                {
                    _sLocalStack.Push(new Dictionary<string, object>());
                    _sLocalStack.Peek().Add(eachNode.IndexName, dataSource.Count - 1);
                    _sLocalStack.Peek().Add(eachNode.ValueName, obj);
                    eachNode.Children.ForEach(child =>
                    {
                        guiNode.AddChild(CreateComponent(child));
                    });
                    _sLocalStack.Pop();
                }
            };
            _sLocalStack.Pop();
        });
        #endregion

        #region Signal
        foreach (var nodeSignal in node.Signals)
        {
            var @event = guiNode.GetType().GetEvent(nodeSignal.Key);
            var methodInfo = _sController?.GetType().GetMethod(nodeSignal.Value);
            if (@event != null)
            {
                if (methodInfo != null)
                {
                    if (@event.EventHandlerType == null) continue;
                    var signalDelegate = Delegate.CreateDelegate(@event.EventHandlerType, _sController, methodInfo);
                    @event.GetAddMethod()?.Invoke(guiNode, new object[] { signalDelegate });
                }
                else
                {
                    throw new Exception($"The method to which the signal is bound does not exist({nodeSignal.Value}).");
                }
            }
            else
            {
                throw new Exception($"The signal to be bound does not exist({nodeSignal.Key}).");
            }
        }
        #endregion
        return guiNode;
    }
    
    private static void SetThemeOverride(Control obj, string key, object value)
    {
        var guiNodeName = obj.GetType().ToString().Split(".")[^1];
        if (Guml.ThemeOverrides.ContainsKey(guiNodeName))
        {
            if (Guml.ThemeOverrides[guiNodeName].ContainsKey(key))
            {
                var type = Guml.ThemeOverrides[guiNodeName][key];
                switch (type)
                {
                    case ThemeValueType.Color:
                        obj.AddThemeColorOverride(key, (Color)value);
                        break;
                    case ThemeValueType.Constant:
                        obj.AddThemeConstantOverride(key, (int)value);
                        break;
                    case ThemeValueType.Font:
                        obj.AddThemeFontOverride(key, (FontFile)value);
                        break;
                    case ThemeValueType.FontSize:
                        obj.AddThemeFontSizeOverride(key, (int)value);
                        break;
                    case ThemeValueType.Icon:
                        obj.AddThemeIconOverride(key, (ImageTexture)value);
                        break;
                    case ThemeValueType.Style:
                        obj.AddThemeStyleboxOverride(key, (StyleBox)value);
                        break;
                }
            }
        }
    }

    private static Control FindComponent(string name)
    {
        var namespaceList = new List<string>() { "", "Godot" };
        foreach (var @namespace in namespaceList)
        {
            var typeName = $"{@namespace}.{name}";
            foreach (var assembly in Guml.Assemblies)
            {
                if (assembly.GetType(typeName) != null) return assembly.CreateInstance(typeName) as Control ?? throw new InvalidOperationException();
            }
        }
        throw new TypeNotFoundException($"GUI Node of type {name} not found!");
    }

    private static object? ExprEval(GumlExprNode exprNode, string bindKey = "")
    {
        switch (exprNode)
        {
            case GumlValueNode valueNode:
                return GetValue(valueNode, bindKey);
            case InfixOpNode infixOpNode:
            {
                var left = ExprEval(infixOpNode.Left, bindKey);
                var right = ExprEval(infixOpNode.Right, bindKey);
                switch (infixOpNode.Op)
                {
                    case "||":
                        if (left is bool orBoolLeft && right is bool orBoolRight)
                        {
                            return orBoolLeft || orBoolRight;
                        }
                        throw new TypeErrorException($"Both ends of expression must be of type bool.");
                    case "&&":
                        if (left is bool andBoolLeft && right is bool andBoolRight)
                        {
                            return andBoolLeft && andBoolRight;
                        }
                        throw new TypeErrorException($"Both ends of expression must be of type bool.");
                    case "!=":
                        switch (left)
                        {
                            // string
                            case string stringNotEqualLeft when right is string stringNotEqualRight:
                                return stringNotEqualLeft != stringNotEqualRight;
                            // int & float
                            case int intNotEqualLeft:
                                switch (right)
                                {
                                    case int intNotEqualRight:
                                        return intNotEqualLeft != intNotEqualRight;
                                    case float floatNotEqualRight:
                                        return Math.Abs(intNotEqualLeft - floatNotEqualRight) > 0.000001f;
                                }
                                break;
                            case float floatNotEqualLeft:
                                switch (right)
                                {
                                    case int intNotEqualRight:
                                        return Math.Abs(floatNotEqualLeft - intNotEqualRight) > 0.000001f;
                                    case float floatNotEqualRight:
                                        return Math.Abs(floatNotEqualLeft - floatNotEqualRight) > 0.000001f;
                                }
                                break;
                            // bool
                            case bool boolNotEqualLeft when right is bool boolNotEqualRight:
                                return boolNotEqualLeft != boolNotEqualRight;
                            default:
                                return left != right;
                        }
                        break;
                    case "==":
                        switch (left)
                        {
                            // string
                            case string stringEqualLeft when right is string stringEqualRight:
                                return stringEqualLeft == stringEqualRight;
                            // int & float
                            case int intEqualLeft:
                                switch (right)
                                {
                                    case int intEqualRight:
                                        return intEqualLeft == intEqualRight;
                                    case float floatEqualRight:
                                        return Math.Abs(intEqualLeft - floatEqualRight) > 0.000001f;
                                }
                                break;
                            case float floatEqualLeft:
                                switch (right)
                                {
                                    case int intEqualRight:
                                        return Math.Abs(floatEqualLeft - intEqualRight) > 0.000001f;
                                    case float floatEqualRight:
                                        return Math.Abs(floatEqualLeft - floatEqualRight) > 0.000001f;
                                }
                                break;
                            // bool
                            case bool boolEqualLeft when right is bool boolEqualRight:
                                return boolEqualLeft == boolEqualRight;
                            default:
                                return left == right;
                        }
                        break;
                    case ">=":
                        return left switch
                        {
                            int intGreaterEqualLeft when right is int intGreaterEqualRight =>
                                intGreaterEqualLeft - intGreaterEqualRight > 0.000001f,
                            int intGreaterEqualLeft when right is float floatGreaterEqualRight =>
                                intGreaterEqualLeft - floatGreaterEqualRight > 0.000001f,
                            float floatGreaterEqualLeft when right is int intGreaterEqualRight =>
                                floatGreaterEqualLeft - intGreaterEqualRight > 0.000001f,
                            float floatGreaterEqualLeft when right is float floatGreaterEqualRight =>
                                floatGreaterEqualLeft - floatGreaterEqualRight > 0.000001f,
                            _ => throw new TypeErrorException($"Both ends of expression must be of type int or float.")
                        };
                    case "<=":
                        return left switch
                        {
                            int intLessEqualLeft when right is int intLessEqualRight =>
                                intLessEqualRight - intLessEqualLeft > 0.000001f,
                            int intLessEqualLeft when right is float floatLessEqualRight =>
                                floatLessEqualRight - intLessEqualLeft > 0.000001f,
                            float floatLessEqualLeft when right is int intLessEqualRight =>
                                intLessEqualRight - floatLessEqualLeft > 0.000001f,
                            float floatLessEqualLeft when right is float floatLessEqualRight =>
                                floatLessEqualRight - floatLessEqualLeft > 0.000001f,
                            _ => throw new TypeErrorException($"Both ends of expression must be of type int or float.")
                        };
                    case ">":
                        switch (left)
                        {
                            case int intGreaterLeft:
                                switch (right)
                                {
                                    case int intGreaterRight:
                                        return intGreaterLeft > intGreaterRight;
                                    case float floatGreaterRight:
                                        return intGreaterLeft > floatGreaterRight;
                                }
                                break;
                            case float floatGreaterLeft:
                                switch (right)
                                {
                                    case int intGreaterRight:
                                        return floatGreaterLeft > intGreaterRight;
                                    case float floatGreaterRight:
                                        return floatGreaterLeft > floatGreaterRight;
                                }
                                break;
                        }
                        throw new TypeErrorException($"Both ends of expression must be of type int or float.");
                    case "<":
                        switch (left)
                        {
                            case int intLessLeft:
                                switch (right)
                                {
                                    case int intLessRight:
                                        return intLessLeft < intLessRight;
                                    case float floatLessRight:
                                        return intLessLeft < floatLessRight;
                                }
                                break;
                            case float floatLessLeft:
                                switch (right)
                                {
                                    case int intLessRight:
                                        return floatLessLeft < intLessRight;
                                    case float floatLessRight:
                                        return floatLessLeft < floatLessRight;
                                }
                                break;
                        }
                        throw new TypeErrorException($"Both ends of expression must be of type int or float.");
                    case "+":
                        return left switch
                        {
                            int intAddLeft when right is int intAddRight => intAddLeft + intAddRight,
                            int intAddLeft when right is float floatAddRight => intAddLeft + floatAddRight,
                            float floatAddLeft when right is int intAddRight => floatAddLeft + intAddRight,
                            float floatAddLeft when right is float floatAddRight => floatAddLeft + floatAddRight,
                            _ => throw new TypeErrorException($"Both ends of expression must be of type int or float.")
                        };
                    case "-":
                        return left switch
                        {
                            int intSubLeft when right is int intSubRight => intSubLeft - intSubRight,
                            int intSubLeft when right is float floatSubRight => intSubLeft - floatSubRight,
                            float floatSubLeft when right is int intSubRight => floatSubLeft - intSubRight,
                            float floatSubLeft when right is float floatSubRight => floatSubLeft - floatSubRight,
                            _ => throw new TypeErrorException($"Both ends of expression must be of type int or float.")
                        };
                    case "*":
                        return left switch
                        {
                            int intMulLeft when right is int intMulRight => intMulLeft * intMulRight,
                            int intMulLeft when right is float floatMulRight => intMulLeft * floatMulRight,
                            float floatMulLeft when right is int intMulRight => floatMulLeft * intMulRight,
                            float floatMulLeft when right is float floatMulRight => floatMulLeft * floatMulRight,
                            _ => throw new TypeErrorException($"Both ends of expression must be of type int or float.")
                        };
                    case "/":
                        return left switch
                        {
                            int intDivLeft when right is int intDivRight => intDivLeft / (float)intDivRight,
                            int intDivLeft when right is float floatDivRight => intDivLeft / floatDivRight,
                            float floatDivLeft when right is int intDivRight => floatDivLeft / intDivRight,
                            float floatDivLeft when right is float floatDivRight => floatDivLeft / floatDivRight,
                            _ => throw new TypeErrorException($"Both ends of expression must be of type int or float.")
                        };
                    case "%":
                        return left switch
                        {
                            int intRemLeft when right is int intRemRight => intRemLeft % intRemRight,
                            int intRemLeft when right is float floatRemRight => intRemLeft % floatRemRight,
                            float floatRemLeft when right is int intRemRight => floatRemLeft % intRemRight,
                            float floatRemLeft when right is float floatRemRight => floatRemLeft % floatRemRight,
                            _ => throw new TypeErrorException($"Both ends of expression must be of type int or float.")
                        };
                }

                break;
            }
            case PrefixOpNode prefixOpNode:
            {
                var right = ExprEval(prefixOpNode.Right, bindKey);
                switch (prefixOpNode.Op)
                {
                    case "!":
                        if (right is bool boolRight)
                        {
                            return !boolRight;
                        }
                        throw new TypeErrorException($"The expression must be of type bool.");
                    case "+":
                        return right switch
                        {
                            int intPositiveRight => +intPositiveRight,
                            float floatPositiveRight => +floatPositiveRight,
                            _ => throw new TypeErrorException($"The expression must be of type int or float.")
                        };
                    case "-":
                        return right switch
                        {
                            int intPositiveRight => -intPositiveRight,
                            float floatPositiveRight => -floatPositiveRight,
                            _ => throw new TypeErrorException($"The expression must be of type int or float.")
                        };
                }

                break;
            }
        }

        return null;
    }

    private static object? GetValue(GumlValueNode valueNode, string bindKey)
    {
        switch (valueNode.ValueType)
        {
            case GumlValueType.Int:
                return valueNode.IntValue;
            case GumlValueType.Float:
                return valueNode.FloatValue;
            case GumlValueType.Boolean:
                return valueNode.BooleanValue;
            case GumlValueType.String:
                return valueNode.StringValue;
            case GumlValueType.Object:
                Dictionary<string, object?> result = new ();
                foreach (var key in valueNode.ObjectValue!.Keys)
                {
                    result[key] = ExprEval(valueNode.ObjectValue[key], bindKey);
                }
                return result;
            case GumlValueType.Null:
                return null;
            case GumlValueType.Vector2:
                var xValue = ExprEval(valueNode.Vector2XNode!);
                var yValue = ExprEval(valueNode.Vector2YNode!);
                return xValue switch
                {
                    int xInt when yValue is int yInt => new Vector2(xInt, yInt),
                    int xInt when yValue is float yFloat => new Vector2(xInt, yFloat),
                    float xFloat when yValue is int yInt => new Vector2(xFloat, yInt),
                    float xFloat when yValue is float yFloat => new Vector2(xFloat, yFloat),
                    _ => new Exception("Vector2 value node type error.")
                };
            case GumlValueType.Color:
                var rValue = (float)(ExprEval(valueNode.ColorRNode!) ?? throw new InvalidOperationException());
                var gValue = (float)(ExprEval(valueNode.ColorGNode!) ?? throw new InvalidOperationException());
                var bValue = (float)(ExprEval(valueNode.ColorBNode!) ?? throw new InvalidOperationException());
                var aValue = (float)(ExprEval(valueNode.ColorANode!) ?? throw new InvalidOperationException());
                return new Color(rValue, gValue, bValue, aValue);
            case GumlValueType.StyleBox:
                if (valueNode.StyleNodeType == StyleNodeType.Empty) return new StyleBoxEmpty();
                StyleBox obj = valueNode.StyleNodeType switch
                {
                    StyleNodeType.Flat => new StyleBoxFlat(),
                    StyleNodeType.Line => new StyleBoxLine(),
                    StyleNodeType.Texture => new StyleBoxTexture(),
                };
                var styleDictionary = (Dictionary<string, object>)ExprEval(valueNode.StyleNode!)!;
                foreach (var gumlExprNode in styleDictionary)
                {
                    SetObjProperty(obj, gumlExprNode.Key, gumlExprNode.Value);
                }
                return obj;
            case GumlValueType.Ref:
                return GetRefValue(valueNode, bindKey);
            case GumlValueType.Resource:
                var resourcePath = ExprEval(valueNode.ResourceNode!);
                if (resourcePath is string resourcePathStr)
                {
                    return Guml.GetResource(resourcePathStr);
                }
                throw new Exception("Resource value node type error.");
        }
        return null;
    }

    private static object? GetRefValue(GumlValueNode valueNode, string bindKey)
    {
        return valueNode.RefType switch
        {
            RefType.GlobalRef => GetGlobalRefValue(valueNode),
            RefType.LocalAliasRef => GetLocalAliasRefValue(valueNode),
            RefType.LocalRef => GetLocalRefValue(valueNode),
            RefType.PropertyRef => GetPropertyRefValue(valueNode, bindKey),
            _ => throw new ArgumentOutOfRangeException(nameof(valueNode))
        };
    }

    private static object GetGlobalRefValue(GumlValueNode valueNode)
    {
        if (Guml.GlobalRefs.TryGetValue(valueNode.RefName, out var value))
        {
            return value;
        }
        throw new Exception($"Global ref '{valueNode.RefName}' not find.");
    }

    private static Control GetLocalAliasRefValue(GumlValueNode valueNode)
    {
        if (!_sController!.NamedNode.TryGetValue(valueNode.RefName, out var value))
        {
            throw new Exception($"Local alias ref '{valueNode.RefName}' not find.");
        }
        return value;
    }

    private static object GetLocalRefValue(GumlValueNode valueNode)
    {
        if (!_sLocalStack.Peek().ContainsKey(valueNode.RefName))
        {
            throw new Exception($"Local ref '{valueNode.RefName}' not find.");
        }

        return _sLocalStack.Peek()[valueNode.RefName];
    }

    private static object? GetPropertyRefValue(GumlValueNode valueNode, string bindKey)
    {
        var refValue = GetRefValue(valueNode.RefNode!, bindKey);
        if (refValue == null) throw new Exception($"Property '{valueNode.RefName}' not find on '{valueNode}'.");
        var refType = refValue.GetType();
        var propertyInfo = refType.GetProperty(valueNode.RefName);
        
        if (propertyInfo == null)
        {
            var fieldInfo = refType.GetField(valueNode.RefName);
            if (fieldInfo == null)
            {
                throw new Exception($"Property '{valueNode.RefName}' not find on object '{valueNode.RefNode}({refValue})'!");
            }

            return fieldInfo.GetValue(refValue);

        } 

        if (bindKey != "" && refValue is INotifyPropertyChanged notifyObj)
        {
            notifyObj.PropertyChanged += (_, _) =>
            {
                var propertyName = bindKey.Split("_")[1];
                _sBindObj?.GetType().GetProperty(propertyName)?.SetValue(_sBindObj,ExprEval(SBindingExprCache[bindKey]));
            };
        }
        return propertyInfo.GetValue(refValue);

    }

    private static TKey GetKeyByValue<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TValue value) where TKey : notnull =>
        // 使用 LINQ 查询字典中第一个值匹配的键
        dictionary.FirstOrDefault(x => EqualityComparer<TValue>.Default.Equals(x.Value, value)).Key;
}
