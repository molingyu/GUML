using System.ComponentModel;
using Godot;

namespace GUML;

public struct BindingExprEnv
{
    public string PropertyName;
    public GumlExprNode Source;
    public Control BindObj;
    public bool IsDefine;
    public Dictionary<string, object> GlobalRefs;
    public Dictionary<string, (INotifyListChanged, int)> LocalRefs;
}

public static class GumlRenderer
{
    private static GumlDoc _sGumlDoc;
    private static GuiController? _sController;
    private static Stack<Dictionary<string, object?>> _sLocalStack = new ();

    public static void Render(GumlDoc gumlDoc, GuiController controller, Node rootNode, string dir)
    {
        ReinitializeRender();
        _sGumlDoc = gumlDoc;
        _sController = _sGumlDoc.Redirect != null ? Activator.CreateInstance(Guml.FindType(_sGumlDoc.Redirect )) as 
            GuiController ?? throw new InvalidOperationException() : controller;
        Guml.GlobalRefs["$controller"] = _sController;
        var component = CreateComponent(_sGumlDoc.RootNode);
        _sController.GumlRootNode = component;
        RenderImports(controller, rootNode, component, dir);
        if (Guml.DefaultTheme != null) component.Theme = Guml.DefaultTheme;
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
        _sLocalStack = new Stack<Dictionary<string, object?>>();
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
                    var env = new BindingExprEnv
                    {
                        PropertyName = propertyName,
                        BindObj = guiNode,
                        IsDefine = true,
                        GlobalRefs = new Dictionary<string, object>(),
                        LocalRefs = new Dictionary<string, (INotifyListChanged, int)>(),
                        Source = propertyNode
                    };
                    value = ExprEval(propertyNode, env);
                    env.IsDefine = false;
                }
                else
                {
                    value = ExprEval(propertyNode);
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
            var dataSource = (INotifyListChanged)ExprEval(eachNode.DataSource)!;
            _sLocalStack.Push(new Dictionary<string, object?>());
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
            
            var localStack = CopyEnv(_sLocalStack);
            var controller = _sController;
            dataSource.ListChanged += (_, changeType, changeIndex, obj) =>
            {
                switch (changeType)
                {
                    case ListChangedType.Add:
                        localStack.Peek()[eachNode.IndexName] = dataSource.Count - 1;
                        localStack.Peek()[eachNode.ValueName] = obj;
                        _sLocalStack = localStack;
                        _sController = controller;
                        eachNode.Children.ForEach(child =>
                        {
                            guiNode.AddChild(CreateComponent(child));
                        });
                        ReinitializeRender();
                        break;
                    case ListChangedType.Remove:
                        guiNode.RemoveChild(guiNode.GetChildren()[dataSource.IndexOf(obj)]);
                        break;
                    case ListChangedType.Insert:
                        localStack.Peek()[eachNode.IndexName] = dataSource.Count - 1;
                        localStack.Peek()[eachNode.ValueName] = obj;
                        _sLocalStack = localStack;
                        _sController = controller;
                        var insertIndex = 0;
                        eachNode.Children.ForEach(child =>
                        {
                            var childNode = CreateComponent(child);
                            guiNode.AddChild(childNode);
                            guiNode.MoveChild(childNode, changeIndex + insertIndex);
                            insertIndex += 1;
                        });
                        ReinitializeRender();
                        break;
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
                    @event.GetAddMethod()?.Invoke(guiNode, [signalDelegate]);
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
        if (Guml.ThemeOverrides.TryGetValue(guiNodeName, out var themeValue))
        {
            if (themeValue.TryGetValue(key, out var themeValueType))
            {
                switch (themeValueType)
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

    private static object? ExprEval(GumlExprNode exprNode, BindingExprEnv? env = null)
    {
        switch (exprNode)
        {
            case GumlValueNode valueNode:
                return GetValue(valueNode, env);
            case InfixOpNode infixOpNode:
            {
                var left = ExprEval(infixOpNode.Left, env);
                var right = ExprEval(infixOpNode.Right, env);
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
                        if (left is string || right is string)
                        {
                            return $"{left}{right}";
                        }
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
                var right = ExprEval(prefixOpNode.Right, env);
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

    private static object? GetValue(GumlValueNode valueNode, BindingExprEnv? env)
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
                    result[key] = ExprEval(valueNode.ObjectValue[key], env);
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
                    StyleNodeType.Empty => new StyleBoxEmpty(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                var styleDictionary = (Dictionary<string, object>)ExprEval(valueNode.StyleNode!)!;
                foreach (var gumlExprNode in styleDictionary)
                {
                    SetObjProperty(obj, gumlExprNode.Key, gumlExprNode.Value);
                }
                return obj;
            case GumlValueType.Ref:
                return GetRefValue(valueNode, env);
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

    private static object? GetRefValue(GumlValueNode valueNode, BindingExprEnv? env)
    {
        return valueNode.RefType switch
        {
            RefType.GlobalRef => GetGlobalRefValue(valueNode, env),
            RefType.LocalAliasRef => GetLocalAliasRefValue(valueNode),
            RefType.LocalRef => GetLocalRefValue(valueNode, env),
            RefType.PropertyRef => GetPropertyRefValue(valueNode, env),
            _ => throw new ArgumentOutOfRangeException(nameof(valueNode))
        };
    }

    private static object GetGlobalRefValue(GumlValueNode valueNode, BindingExprEnv? env)
    {
        var source = Guml.GlobalRefs;
        if (env != null)
        {
            if (env.Value.IsDefine)
            {
                if (source.TryGetValue(valueNode.RefName, out var refValue))
                {
                    env.Value.GlobalRefs.Add(valueNode.RefName, refValue);
                }
                else
                {
                    throw new Exception($"Global ref '{valueNode.RefName}' not find.");
                }
            }
            else
            {
                source = env.Value.GlobalRefs;
            }
        }
        if (source.TryGetValue(valueNode.RefName, out var value))
        {
            return value;
        }
        throw new Exception($"Global ref '{valueNode.RefName}' not find.");
    }

    private static Control GetLocalAliasRefValue(GumlValueNode valueNode)
    {
        if (!_sController!.NamedNode.TryGetValue(valueNode.RefName.Substring(1, valueNode.RefName.Length - 1), out 
            var value))
        {
            throw new Exception($"Local alias ref '{valueNode.RefName}' not find.");
        }
        return value;
    }

    private static object? GetLocalRefValue(GumlValueNode valueNode, BindingExprEnv? env)
    {
        if (env != null)
        {
            if (!env.Value.IsDefine)
            {
                if (env.Value.LocalRefs.TryGetValue(valueNode.RefName, out var value))
                {
                    if (value.Item1.Count - 1 > value.Item2)
                    {
                        return value.Item1[value.Item2]!;
                    }
                    throw new Exception();
                }
            }
        }
        foreach (var dict in _sLocalStack)
        {
            if (dict.TryGetValue(valueNode.RefName, out var value))
            {
                return value;
            }
        }
        
        throw new Exception($"Local ref '{valueNode.RefName}' not find.");
    }

    private static object? GetPropertyRefValue(GumlValueNode valueNode, BindingExprEnv? env)
    {
        var refValue = GetRefValue(valueNode.RefNode!, env);
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

        if (env != null && env.Value.IsDefine && refValue is INotifyPropertyChanged notifyObj)
        {
            var bindObj = env.Value.BindObj;
            notifyObj.PropertyChanged += (_, _) =>
            {
                var propertyName = env.Value.PropertyName;
                bindObj.GetType().GetProperty(propertyName)?.SetValue(bindObj,ExprEval(env.Value.Source, env));
            };
        }
        return propertyInfo.GetValue(refValue);

    }

    private static Stack<Dictionary<string, object?>> CopyEnv(Stack<Dictionary<string, object?>> source)
    {
        var result = new Stack<Dictionary<string, object?>>();
        foreach (var dictionary in source)
        {
            result.Push(new Dictionary<string, object?>(dictionary));
        }
        return result;
    }
    
    private static TKey GetKeyByValue<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TValue value) where TKey : notnull =>
        // 使用 LINQ 查询字典中第一个值匹配的键
        dictionary.FirstOrDefault(x => EqualityComparer<TValue>.Default.Equals(x.Value, value)).Key;
}
