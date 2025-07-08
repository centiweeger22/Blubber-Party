using System.Collections.Generic;

public class Bucket
{
	public List<uint> tickets;

	public Bucket(int count)
	{
		tickets = new List<uint>();
		for (uint num = 0u; num < count; num++)
		{
			tickets.Add(num);
		}
	}

	public bool IsAvailable()
	{
		return tickets.Count >= 1;
	}

	public void ReturnIndex(uint index)
	{
		tickets.Add(index);
	}

	public uint GetFreeIndex()
	{
		uint result = tickets[0];
		tickets.RemoveAt(0);
		return result;
	}
}
