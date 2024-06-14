using GUML;

public class MainController : GuiController
{
    public SettingController SettingController { get; set; }
    public string SayHello {
        get => _sayHello;
        set
        {
            _sayHello = value;
            OnPropertyChanged();
        }
    }

    private string _sayHello = "world!";

    public override void Created()
    {
        
    }

    public void ChangeHelloBtnPressed()
    {
        SayHello = "new world!";
    }
}