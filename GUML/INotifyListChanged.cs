namespace GUML;

public delegate void ListChangedEventHandler<in T>(object? sender, bool isRemove, T obj);

public interface INotifyListChanged<out T>
{
    public event ListChangedEventHandler<T> ListChanged;
}
