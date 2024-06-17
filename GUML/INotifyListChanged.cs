using System.Collections;

namespace GUML;

public enum ListChangedType
{
    Remove,
    Add,
    Insert
}
public delegate void ListChangedEventHandler(object? sender, ListChangedType changedType, int index, object? obj);
public delegate void ValueChangedEventHandler(object? sender, int index, object? obj);

public interface INotifyListChanged : IList
{
    public event ListChangedEventHandler ListChanged;

    public event ValueChangedEventHandler ValueChanged;
}
