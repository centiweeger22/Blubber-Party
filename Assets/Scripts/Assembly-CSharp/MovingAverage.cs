using System.Collections.Generic;

public class MovingAverage
{
	public List<float> values;

	public int maxValues = 1;

	private float sum;

	private int lossy;

	public MovingAverage(int _maxValues)
	{
		values = new List<float>();
		maxValues = _maxValues;
	}

	public void Update(float newValue)
	{
		values.Add(newValue);
		sum += newValue;
		if (values.Count > maxValues)
		{
			sum -= values[0];
			values.RemoveAt(0);
		}
	}

	public float GetAverage()
	{
		lossy++;
		if (lossy < 100)
		{
			int count = values.Count;
			if (count == 0)
			{
				return 0f;
			}
			return sum / (float)count;
		}
		lossy = 0;
		int count2 = values.Count;
		float num = 0f;
		if (count2 == 0)
		{
			return 0f;
		}
		for (int i = 0; i < count2; i++)
		{
			num += values[i];
		}
		sum = num;
		return num / (float)count2;
	}
}
