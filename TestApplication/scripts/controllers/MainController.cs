using GUML;

public class Actor
{
	public string Name;
	public int Level;
	public int Hp;
	public int Mp;
}

public class MainController : GuiController
{
	public SettingController SettingController;
	
	public NotifyList<Actor> Actors =
	[
		new Actor
		{
			Name = "Bob",
			Level = 10,
			Hp = 1200,
			Mp = 600
		},
		new Actor
		{
			Name = "John",
			Level = 10,
			Hp = 1200,
			Mp = 600
		}
	
	];

	public void ExitGamePressed()
	{
		RootNode.GetTree().Quit();
	}
}
