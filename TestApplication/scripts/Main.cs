using System;
using System.IO;
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
		Guml.ResourceLoader += resPath =>
		{
			var type = Path.GetExtension(resPath);
			switch (type)
			{
				case "png":
					return ImageTexture.CreateFromImage(Image.LoadFromFile(resPath));
				case "ogg":
					return AudioStreamOggVorbis.LoadFromFile(resPath);
				case "ttf":
				case "woff":
				case "woff2":
				case "pfb":
				case "pfm":
				case "otf":
					var font = new FontFile();
					font.LoadDynamicFont(resPath);
					return font;
				default:
					throw new Exception($"Error resource type('{type}').");
			}
		};
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
