namespace GUML;

public class PrefixOpNode : GumlOpNode
{
    public static readonly Dictionary<string, int> OpPrecedence = new() { { "!", 70 }, { "+", 70 }, { "-", 70 } };
    public override void Add(GumlExprNode node, bool isLeft = false)
    {
        if (isLeft)
        {
            throw new Exception("Node type is prefix node.");
        }

        Right = node;
        node.Parent = this;
    }
}
