using GUML;

public class SettingController : GuiController
{
    public int ListWidth = 100;
    public NotifyList<string> Names
    {
        get => _names;
        set
        {
            _names = value;
            OnPropertyChanged();
        }
    }

    public NotifyList<int> XPos
    {
        get => _xPos;
        set
        {
            _xPos = value;
            OnPropertyChanged();
        }
    }
    
    public NotifyList<int> YPos
    {
        get => _yPos;
        set
        {
            _yPos = value;
            OnPropertyChanged();
        }
    }

    private NotifyList<string> _names = ["Bob", "John", "Jeff", "Dave"];
    private NotifyList<int> _xPos = [12, 224];
    private NotifyList<int> _yPos = [40, 80];
    
    public override void Update()
    {
    }

    public override void Dispose()
    {
    }

    public void AddXPressed()
    {
        XPos.Add(424);
    }
    
    public void AddYPressed()
    {
        YPos.Add(120);
    }
}