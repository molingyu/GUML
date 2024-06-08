namespace GUML;

public enum GumlValueType
{
    String,
    Int,
    Float,
    Boolean,
    Vector2,
    Color,
    StyleBox,
    Object,
    Resource,
    Ref,
    Null
}

public enum RefType
{
    No,
    GlobalRef,
    LocalAliasRef,
    LocalRef,
    PropertyRef
}

public enum StyleNodeType
{
    Empty,
    Flat,
    Line,
    Texture
}

public class GumlValueNode : GumlExprNode
{
    public GumlValueType ValueType { get; set; }

    public string StringValue { get; set; } = "";
    public int IntValue { get; set; }
    public float FloatValue { get; set; }
    public bool BooleanValue { get; set; }
    public Dictionary<string, GumlExprNode>? ObjectValue { get; set; }
    public RefType RefType { get; set; } = RefType.No;
    public string RefName { get; set; } = "";
    public GumlValueNode? RefNode { get; init; }
    public GumlExprNode? ResourceNode { get; set; }
    public GumlExprNode? Vector2XNode { get; set; }
    public GumlExprNode? Vector2YNode { get; set; }
    public GumlExprNode? ColorRNode { get; set; }
    public GumlExprNode? ColorGNode { get; set; }
    public GumlExprNode? ColorBNode { get; set; }
    public GumlExprNode? ColorANode { get; set; }
    public StyleNodeType? StyleNodeType { get; set; }
    public GumlExprNode? StyleNode { get; set; }
    
    public override void Add(GumlExprNode node, bool isLeft = false) =>
        throw new GumlParserException("Node type is value node.", this);
}
