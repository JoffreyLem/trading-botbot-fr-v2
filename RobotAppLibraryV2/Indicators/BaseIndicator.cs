using System.Buffers;
using RobotAppLibraryV2.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibraryV2.Indicators;

public abstract class BaseIndicator<T> : IIndicator, IDisposable where T : ResultBase
{
    private T[] _data = ArrayPool<T>.Shared.Rent(2100);

    public int LoopBackPeriod { get; set; } = 1;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
            return _data[index];
        }
        set
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
            _data[index] = value;
        }
    }

    public int Count { get; private set; }


    public void Dispose()
    {
        if (_data != null)
        {
            ArrayPool<T>.Shared.Return(_data);
            _data = null;
        }
    }

    public Tick LastTick { get; set; } = new();

    public void UpdateIndicator(IEnumerable<Candle> data)
    {
        if (Count > 0)
        {
            Array.Clear(_data, 0, Count);
            Count = 0;
        }


        var dataToAdd = Update(data).ToArray();
        EnsureCapacity(dataToAdd.Length);


        Array.Copy(dataToAdd, _data, dataToAdd.Length);
        Count = dataToAdd.Length;
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
        if (Count == 0)
            throw new InvalidOperationException("The list is empty.");

        return _data[Count - 1];
    }

    public T LastOrDefault()
    {
        return Count == 0 ? default : _data[Count - 1];
    }

    protected abstract IEnumerable<T> Update(IEnumerable<Candle> data);
}