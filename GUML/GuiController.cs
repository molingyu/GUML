using System.ComponentModel;
using System.Runtime.CompilerServices;
using Godot;

namespace GUML;

public abstract class GuiController : INotifyPropertyChanged
{
    public readonly Dictionary<string, Control> NamedNode = new ();

    public Control RootNode = null!;
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void Created()
    {
        
    }
    
    public virtual void Update()
    {
        
    }

    public virtual void Dispose()
    {
        RootNode.GetParent().RemoveChild(RootNode);
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
