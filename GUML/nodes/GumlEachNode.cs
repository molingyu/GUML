namespace GUML;

public class GumlEachNode : GumlSyntaxNode
{
    public required GumlValueNode DataSource { get; init; }
    public required string IndexName { get; init; }
    public required string ValueName { get; init; }
}
