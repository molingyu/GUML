using System.ComponentModel;
using System.Runtime.CompilerServices;
using Godot;

namespace GUML;

public abstract class GuiController : INotifyPropertyChanged
{
    public readonly Dictionary<string, Control> NamedNode = new ();

    public Control RootNode;
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void Update()
    {
        
    }

    public virtual void Dispose()
    {
        
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
