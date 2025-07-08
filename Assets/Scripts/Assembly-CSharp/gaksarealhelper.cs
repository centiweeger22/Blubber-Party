using System.Collections.Generic;
using System.Threading;


public class SimpleThreadSafeQueue<T>
{
	private readonly Queue<T> queue = new Queue<T>();
	private readonly object lockObj = new object();

	public void Enqueue(T item)
	{
		lock (lockObj)
		{
			queue.Enqueue(item);
		}
	}

	public bool TryDequeue(out T result)
	{
		lock (lockObj)
		{
			if (queue.Count > 0)
			{
				result = queue.Dequeue();
				return true;
			}
			result = default(T);
			return false;
		}
	}

	public void Clear()
	{
		lock (lockObj)
		{
			queue.Clear();
		}
	}

	public int Count
	{
		get
		{
			lock (lockObj)
			{
				return queue.Count;
			}
		}
	}
}
