using System.Collections;
using System.Collections.Immutable;

namespace azurestream
{
    public class ObservableList<T> : IReadOnlyCollection<T>, IObserver<T>, IDisposable
    {
        private readonly IDisposable _disposable;
        private readonly Action? _onChange;
        private ImmutableList<T> _list = ImmutableList<T>.Empty;

        public ObservableList(IObservable<T> observer, Action? onChange = null)
        {
            _disposable = observer.Subscribe(this);
            _onChange = onChange;
        }

        public int Count => _list.Count;

        public void Dispose()
        {
            _disposable.Dispose();
        }

        public void Clear()
        {
            Interlocked.Exchange(ref _list, ImmutableList<T>.Empty);
            _onChange?.Invoke();
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
            Interlocked.Exchange(ref _list, _list.Insert(0, value));
            _onChange?.Invoke();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
