namespace GUML;

public class InfixOpNode : GumlOpNode
{


    public static readonly Dictionary<string, int> OpPrecedence = new()
    {
        { "||", 10 },
        { "&&", 10 },
        { "!=", 20 },
        { "==", 20 },
        { ">=", 30 },
        { "<=", 30 },
        { ">", 30 },
        { "<", 30 },
        { "+", 40 },
        { "-", 40 },
        { "*", 50 },
        { "/", 50 },
        { "%", 50 }
    };

    private GumlExprNode _left = null!;
    public GumlExprNode Left
    {
        get => _left;
        set
        {
            _left = value;
            value.Parent = this;
        }
    }

    public override void Add(GumlExprNode node, bool isLeft = false)
    {
        if (isLeft)
        {
            Left = node;
        }
        else
        {
            Right = node;
        }

        node.Parent = this;
    }
}
