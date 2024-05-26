namespace GUML;

public delegate void ListChangedEventHandler(object? sender, bool isRemove, object obj);

public interface INotifyListChanged
{
    public event ListChangedEventHandler ListChanged;
}
