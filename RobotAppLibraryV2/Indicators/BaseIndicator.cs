using System.Buffers;
using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators;

public abstract class BaseIndicator<T> : IIndicator, IDisposable where T : ResultBase
{
    private T[] _data = ArrayPool<T>.Shared.Rent(2100);
    private int _count = 0;

    public int LoopBackPeriod { get; set; } = 1;
    public Tick LastTick { get; set; } = new();

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();
            return _data[index];
        }
        set
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();
            _data[index] = value;
        }
    }

    public int Count => _count;

    public void UpdateIndicator(IEnumerable<Candle> data)
    {
       
        if (_count > 0)
        {
            Array.Clear(_data, 0, _count);
            _count = 0;
        }


        var dataToAdd = Update(data).ToArray();
        EnsureCapacity(dataToAdd.Length);


        Array.Copy(dataToAdd, _data, dataToAdd.Length);
        _count = dataToAdd.Length;
    }

    private void EnsureCapacity(int newLength)
    {
        if (_data.Length < newLength)
        {
            ArrayPool<T>.Shared.Return(_data);
            _data = ArrayPool<T>.Shared.Rent(newLength);
        }
    }
    
    public T Last()
    {
        if (_count == 0)
            throw new InvalidOperationException("The list is empty.");

        return _data[_count - 1];
    }
    
    public T LastOrDefault()
    {
        return _count == 0 ? default(T) : _data[_count - 1];
    }

    protected abstract IEnumerable<T> Update(IEnumerable<Candle> data);

 
    public void Dispose()
    {
        if (_data != null)
        {
            ArrayPool<T>.Shared.Return(_data);
            _data = null;
        }
    }
}