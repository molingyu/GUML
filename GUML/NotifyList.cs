namespace GUML;

public class NotifyList<T> : List<T>, INotifyListChanged
{
    public event ListChangedEventHandler? ListChanged;
    public event ValueChangedEventHandler? ValueChanged;

    public new T this[int index]
    {
        get => base[index];
        set
        {
            base[index] = value;
            ValueChanged?.Invoke(this, index, value);
        }
    }
    
    public new void Add(T item)
    {
        base.Add(item);
        ListChanged?.Invoke(this, ListChangedType.Add, Count, item);
    }

    public new void Insert(int index, T item)
    {
        base.Insert(index, item);
        ListChanged?.Invoke(this, ListChangedType.Insert, index, item);
    }

    public new void AddRange(IEnumerable<T> collection)
    {
        var index = Count;
        IEnumerable<T> enumerable = collection.ToList();
        base.AddRange(enumerable);
        foreach (var item in enumerable)
        {
            ListChanged?.Invoke(this, ListChangedType.Add, index, item);
            index += 1;
        }
    }

    public new void Remove(T item)
    {
        base.Remove(item);
        ListChanged?.Invoke(this, ListChangedType.Remove, Count - 1, item);
    }

    public new void RemoveRange(int start, int count)
    {
        base.RemoveRange(start, count);
        for (var index = start; index < start + count; index++)
        {
            ListChanged?.Invoke(this, ListChangedType.Remove, index, this[index]);
        }
    }

    public new void Clear()
    {
        for (var index = 0; index < Count; index++)
        {
            ListChanged?.Invoke(this, ListChangedType.Remove, index, this[index]);
        }
        base.Clear();
    }
}