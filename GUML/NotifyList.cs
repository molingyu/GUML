namespace GUML;

public class NotifyList<T> : List<T>, INotifyListChanged
{
    public event ListChangedEventHandler? ListChanged;

    public new void Add(T item)
    {
        base.Add(item);
        ListChanged?.Invoke(this, false, item!);
    }

    public new void AddRange(IEnumerable<T> collection)
    {
        IEnumerable<T> enumerable = collection.ToList();
        base.AddRange(enumerable);
        foreach (var item in enumerable)
        {
            ListChanged?.Invoke(this, false, item!);
        }
    }

    public new void Remove(T item)
    {
        base.Remove(item);
        ListChanged?.Invoke(this, true, item!);
    }

    public new void RemoveRange(int start, int count)
    {
        base.RemoveRange(start, count);
        for (var index = start; index < start + count; index++)
        {
            ListChanged?.Invoke(this, false, this[index]!);
        }
    }

    public new void Clear()
    {
        base.Clear();
        ForEach(removeObj => ListChanged?.Invoke(this, true, removeObj!));
    }
}