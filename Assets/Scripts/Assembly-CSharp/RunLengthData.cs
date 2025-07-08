using System.Collections.Generic;

public class RunLengthData
{
	public List<bool> states;

	public List<int> lengths;

	public int maxLength = 64;

	public int bitLength;

	public RunLengthData()
	{
		states = new List<bool>();
		lengths = new List<int>();
		bitLength = MathExtension.RequiredBits(maxLength);
	}

	public void Update(bool state)
	{
		int count = states.Count;
		int index = count - 1;
		if (count <= 0)
		{
			states.Add(state);
			lengths.Add(1);
		}
		else if (state == states[index])
		{
			if (lengths[index] >= maxLength - 1)
			{
				states.Add(state);
				lengths.Add(1);
			}
			else
			{
				lengths[index]++;
			}
		}
		else
		{
			states.Add(state);
			lengths.Add(1);
		}
	}

	public void AddState(bool state, int length)
	{
		states.Add(state);
		lengths.Add(length);
	}
}
