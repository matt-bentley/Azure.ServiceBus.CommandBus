
namespace Azure.ServiceBus.CommandBus.Sender
{
	public class AsyncIterator
	{
		private readonly int _concurrency;

		public AsyncIterator(int concurrency)
		{
			_concurrency = concurrency;
		}

		public async Task IterateAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken, Func<T, Task> processor)
		{
			var exceptions = new Queue<Exception>();
			var nextIndex = 0;
			var tasks = new List<Task>();
			var itemList = items.ToList();

			// populate task list with number of concurrent tasks
			while (nextIndex < _concurrency && nextIndex < itemList.Count)
			{
				tasks.Add(ProcessItemAsync(itemList[nextIndex], processor, exceptions));
				nextIndex++;
			}

			while (tasks.Count > 0)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}

				var task = await Task.WhenAny(tasks);
				tasks.Remove(task);

				// add another item if there are any left
				if (nextIndex < itemList.Count)
				{
					tasks.Add(ProcessItemAsync(itemList[nextIndex], processor, exceptions));
					nextIndex++;
				}
			}

			if (exceptions.Count > 0)
			{
				throw new AggregateException(exceptions);
			}
		}

		private async Task ProcessItemAsync<T>(T item, Func<T, Task> processor, Queue<Exception> exceptions)
		{
			try
			{
				await processor(item);
			}
			catch (Exception ex)
			{
				exceptions.Enqueue(new Exception($"Error while processing matches for {item.ToString()}", ex));
			}
		}
	}
}
