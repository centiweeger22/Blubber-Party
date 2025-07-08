using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class ArtificialLag
{
	public Random rng;

	public Channel channel;

	public List<SentMessage> pendingMessages;

	public List<float> times;

	public float packetLoss = 0.95f;

	public float tripTime = 3.5f;

	public float additionalTripTime = 1f;

	public ArtificialLag()
	{
		rng = new Random();
		pendingMessages = new List<SentMessage>();
		times = new List<float>();
	}

	public void QueueMessage(SentMessage message)
	{
		float num = (float)rng.NextDouble();
		float num2 = (float)rng.NextDouble();
		if (!(num > packetLoss))
		{
			pendingMessages.Add(message);
			times.Add(tripTime + additionalTripTime * num2);
		}
	}

	public void Update(float deltaTime)
	{
		int num = pendingMessages.Count;
		for (int i = 0; i < num; i++)
		{
			SentMessage sentMessage = pendingMessages[i];
			times[i] -= deltaTime;
			if (times[i] <= 0f)
			{
				byte[] array = Hash.SignBuffer(sentMessage.buffer);
				channel.socket.BeginSendTo(array, 0, array.Length, SocketFlags.None, channel.endpoint, channel.OnDataSendTo, null);
				pendingMessages.RemoveAt(i);
				times.RemoveAt(i);
				i--;
				num--;
			}
		}
	}
}
