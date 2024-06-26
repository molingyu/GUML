namespace GUML;

public struct GumlDoc()
{
    public GumlSyntaxNode RootNode;
    public string? Redirect;
    public readonly Dictionary<string, GumlSyntaxNode> LocalAlias = new();
    public readonly Dictionary<string, bool> Imports = new();
}
