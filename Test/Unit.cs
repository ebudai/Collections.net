using Budaisoft.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Unit;

public partial class Test
{
    private const int Index = 3;
    private const int Value = 29;

    [Fact]
    public void TryRemove()
    {
        int extracted = -1;
        bool success = false;

        ConcurrentCache<int, int> counter = new();
        counter[Index] = Value;

        BackgroundWorker worker = new();
        worker.DoWork += Worker_DoWork;
        worker.RunWorkerAsync();

        while (worker.IsBusy) { }

        Assert.True(success);
        Assert.Equal(Value, extracted);
        Assert.Equal(0, counter.Keys.Count);
        Assert.Equal(0, counter.Values.Count);

        void Worker_DoWork(object sender, DoWorkEventArgs e) => success = counter.TryRemove(Index, out extracted);
    }

    [Fact]
    public void TryRemoveNonExistent()
    {
        int extracted = -1;
        bool success = false;

        ConcurrentCache<int, int> counter = new();

        BackgroundWorker worker = new();
        worker.DoWork += Worker_DoWork;
        worker.RunWorkerAsync();

        while (worker.IsBusy) { }

        Assert.Equal(0, counter.Keys.Count);
        Assert.Equal(0, counter.Values.Count);
        Assert.False(success);
        Assert.Equal(default, extracted);
        Assert.Equal(0, counter.Keys.Count);
        Assert.Equal(0, counter.Values.Count);

        void Worker_DoWork(object sender, DoWorkEventArgs e) => success = counter.TryRemove(Index, out extracted);
    }

    [Fact]
    public void ContainsKey()
    {
        bool contains = false;

        ConcurrentCache<int, int> counter = new();
        counter[Index] = Value;

        BackgroundWorker worker = new();
        worker.DoWork += Worker_DoWork;
        worker.RunWorkerAsync();

        while (worker.IsBusy) { }

        Assert.True(contains);
        Assert.Equal(1, counter.Keys.Count);
        Assert.Equal(1, counter.Values.Count);

        void Worker_DoWork(object sender, DoWorkEventArgs e) => contains = counter.ContainsKey(Index);
    }

    [Fact]
    public void TryAdd()
    {
        bool success = false;

        ConcurrentCache<int, int> counter = new();

        BackgroundWorker worker = new();
        worker.DoWork += Worker_DoWork;
        worker.RunWorkerAsync();

        while (worker.IsBusy) { }

        Assert.True(success);
        Assert.Equal(1, counter.Keys.Count);
        Assert.Equal(1, counter.Values.Count);

        void Worker_DoWork(object sender, DoWorkEventArgs e) => success = counter.TryAdd(Index, Value);
    }

    [Fact]
    public void Lookup()
    {
        ConcurrentCache<int, int> counter = new();

        BackgroundWorker worker = new();
        worker.DoWork += Worker_DoWork;
        worker.RunWorkerAsync();

        while (worker.IsBusy) { }

        Assert.Equal(Value, counter[Index]);

        void Worker_DoWork(object sender, DoWorkEventArgs e) => counter[Index] = Value;
    }

    [Fact]
    public void CustomComparer()
    {
        object alice = new();
        object bob = new();

        ConcurrentCache<object, int> counter = new(ReferenceEqualityComparer.Instance);

        BackgroundWorker worker = new();
        worker.DoWork += Worker_DoWork;
        worker.RunWorkerAsync();

        while (worker.IsBusy) { }

        Assert.True(counter.ContainsKey(alice));
        Assert.False(counter.ContainsKey(bob));
        Assert.Equal(Value, counter[alice]);
        Assert.Equal(1, counter.Keys.Count);
        Assert.Equal(1, counter.Values.Count);

        void Worker_DoWork(object sender, DoWorkEventArgs e) => counter[alice] = Value;
    }

    [Fact]
    public void CustomDefaultValueFactory()
    {
        ConcurrentCache<int, int> counter = new(_ => Value);

        Assert.False(counter.ContainsKey(Value));

        Assert.Equal(Value, counter[Index]);
    }

    [Fact]
    public void CustomDefaultValueFactoryAndComparer()
    {
        object alice = new();
        object bob = new();

        ConcurrentCache<object, int> counter = new(ReferenceEqualityComparer.Instance, _ => Value);

        Assert.False(counter.ContainsKey(alice));
        Assert.False(counter.ContainsKey(bob));
        Assert.Equal(Value, counter[alice]);
        Assert.True(counter.ContainsKey(alice));
        Assert.Equal(1, counter.Keys.Count);
        Assert.Equal(1, counter.Values.Count);
    }

    [Fact]
    public void GetOrAdd()
    {
        int value = -1;

        ConcurrentCache<int, int> counter = new();

        BackgroundWorker worker = new();
        worker.DoWork += Worker_DoWork;
        worker.RunWorkerAsync();

        while (worker.IsBusy) { }

        Assert.Equal(Value, value);

        void Worker_DoWork(object sender, DoWorkEventArgs e) => value = counter.GetOrAdd(Index, () => Value);
    }

    [Fact]
    public void GetOrAddWithKey()
    {
        int value = -1;

        ConcurrentCache<int, int> counter = new();

        BackgroundWorker worker = new();
        worker.DoWork += Worker_DoWork;
        worker.RunWorkerAsync();

        while (worker.IsBusy) { }

        Assert.Equal(Value, value);

        void Worker_DoWork(object sender, DoWorkEventArgs e) => value = counter.GetOrAdd(Value, k => k);
    }
}    
