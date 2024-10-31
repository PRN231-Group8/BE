namespace PRN231.ExploreNow.UnitTests.OtherObjects
{
	internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
	{
		private readonly IEnumerator<T> _inner;

		public TestAsyncEnumerator(IEnumerator<T> inner)
		{
			_inner = inner;
		}

		public T Current
		{
			get { return _inner.Current; }
		}

		public ValueTask<bool> MoveNextAsync()
		{
			return new ValueTask<bool>(_inner.MoveNext());
		}

		public ValueTask DisposeAsync()
		{
			_inner.Dispose();
			return new ValueTask();
		}
	}
}
