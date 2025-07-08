using UnityEngine;

public class Statistics : MonoBehaviour
{
	public MovingAverage fpsAverage;

	public MovingAverage pingAverage;

	private int sentBytesCounter;

	private int recievedBytesCounter;

	public int sentBytes;

	public int recievedBytes;

	private float timer;

	private void Start()
	{
		fpsAverage = new MovingAverage(20);
		pingAverage = new MovingAverage(20);
	}

	private void Update()
	{
		float b = 1E-06f;
		float newValue = 1f / Mathf.Max(Time.deltaTime, b);
		fpsAverage.Update(newValue);
		timer += Time.deltaTime;
		if (timer >= 1f)
		{
			timer -= 1f;
			sentBytes = sentBytesCounter;
			recievedBytes = recievedBytesCounter;
			sentBytesCounter = 0;
			recievedBytesCounter = 0;
		}
	}

	public void OnPingRecieved(float time)
	{
		float newValue = time * 1000f;
		pingAverage.Update(newValue);
	}

	public void OnBytesSent(int length)
	{
		sentBytesCounter += length;
	}

	public void OnBytesRecieved(int length)
	{
		recievedBytesCounter += length;
	}
}
