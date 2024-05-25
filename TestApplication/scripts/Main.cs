using Godot;
using GUML;

public partial class Main : Node
{
	[Export]
	public Node Root;

	private GuiController _controller;
	public override void _Ready()
	{
		Guml.Init();
		Guml.Assemblies.Add(typeof(MainController).Assembly);
		Guml.ResourceLoader += ResourceManager.LoadResource;
		_controller = Guml.LoadGuml(Root,"gui/main.guml");
	}

	public override void _Process(double delta)
	{
		_controller?.Update();

	}

	public override void _ExitTree()
	{
		_controller?.Dispose();
	}
}
