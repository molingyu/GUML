namespace GUML;

public class GumlEachNode : GumlSyntaxNode
{
    public GumlValueNode DataSource { get; init; }
    public string IndexName { get; init; }
    public string ValueName { get; init; }
}
