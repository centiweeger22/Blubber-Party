using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class Channel
{
	public delegate void ByteFunc(int length);

	public delegate void MessageFunc(RecievedMessage message);

	public MessageFunc sendMessageCallback;

	public ByteFunc sentBytesCallback;

	public ByteFunc recievedBytesCallback;

	public byte[] buffer;

	public int bufferLength;

	public Settings.EMessageType messageType;

	public EndPoint origin;

	public Socket socket;

	public EndPoint endpoint;

	public bool isActive;

	public bool isBusy;

	public bool dynamicPort;

	public List<SentMessage> pendingMessages;

	public int reliableSendIndex;

	public int reliableRecievedIndex;

	public int ackHistory;

	public int orderedSendIndex;

	public int orderedRecievedIndex = -1;

	public ArtificialLag lag;

	public Channel()
	{
		endpoint = new IPEndPoint(IPAddress.Any, 0);
		pendingMessages = new List<SentMessage>();
		ackHistory = 0;
		buffer = new byte[1300];
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		socket.Blocking = false;
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
		sentBytesCallback = DefaultBytesFuncCallback;
		recievedBytesCallback = DefaultBytesFuncCallback;
	}

	public void DefaultBytesFuncCallback(int length)
	{
	}

	public void Bind(string ipString, int port)
	{
		socket.Bind(new IPEndPoint(IPAddress.Parse(ipString), port));
		isActive = true;
	}

	public void Bind(EndPoint ep)
	{
		socket.Bind(ep);
		isActive = true;
	}

	public void Connect(string ipString, int port)
	{
		endpoint = new IPEndPoint(IPAddress.Parse(ipString), port);
	}

	public void Connect(EndPoint ep)
	{
		endpoint = ep;
	}

	public void Dump()
	{
		if (!isActive)
		{
			return;
		}
		lock (socket)
		{
			isActive = false;
			socket.Close(0);
		}
	}

	public void RecieveThread()
	{
		while (true)
		{
			if (!isBusy)
			{
				if (!isActive)
				{
					break;
				}
				Recieve();
			}
		}
	}

	public void Recieve(bool bypass = false)
	{
		if ((!isBusy || bypass) && isActive)
		{
			origin = new IPEndPoint(IPAddress.Any, 0);
			socket.BeginReceiveFrom(buffer, 0, 1300, SocketFlags.None, ref origin, OnDataRecieved, null);
			isBusy = true;
		}
	}

	public void OnDataRecieved(IAsyncResult result)
	{
		if (!isActive)
		{
			isBusy = false;
			return;
		}
		int num = 0;
		lock (socket)
		{
			num = socket.EndReceiveFrom(result, ref origin);
		}
		if (num == 0)
		{
			Recieve(bypass: true);
			return;
		}
		recievedBytesCallback(num + 28);
		bufferLength = num;
		if (!Hash.VerifyBuffer(ref buffer, bufferLength))
		{
			Recieve(bypass: true);
			return;
		}
		if (dynamicPort)
		{
			IPEndPoint iPEndPoint = endpoint as IPEndPoint;
			IPEndPoint iPEndPoint2 = origin as IPEndPoint;
			if ((object.Equals(iPEndPoint.Address, iPEndPoint2.Address) || IPOperation.IsLocal(iPEndPoint2.Address)) && iPEndPoint.Port != iPEndPoint2.Port)
			{
				endpoint = new IPEndPoint(iPEndPoint.Address, iPEndPoint2.Port);
			}
		}
		BitStream bitStream = new BitStream(buffer);
		if ((messageType = (Settings.EMessageType)(byte)(bitStream.ReadBits(2)[0] & 3)) < Settings.EMessageType.RELIABLE_UNORDERED)
		{
			bitStream.bitIndex = 8;
			int bitCount = num * 8 - 8;
			Buffer.BlockCopy(bitStream.ReadBits(bitCount), 0, buffer, 0, bufferLength - 1);
			RecievedMessage message = new RecievedMessage(buffer, bufferLength, messageType, origin, this);
			sendMessageCallback(message);
			Recieve(bypass: true);
			return;
		}
		if (messageType == Settings.EMessageType.RELIABLE_UNORDERED)
		{
			int num2 = bitStream.ReadInt(14);
			int num3 = MathExtension.DiffWrapped(reliableRecievedIndex, num2, 16384);
			int num4 = 1;
			if (MathExtension.IsGreaterWrapped(num2, reliableRecievedIndex, 16384))
			{
				reliableRecievedIndex = num2;
				ackHistory <<= num3;
			}
			else
			{
				num4 = 1 << num3;
			}
			if ((ackHistory & num4) == 0)
			{
				ackHistory |= num4;
				bitStream.bitIndex = 16;
				int bitCount2 = num * 8 - 16;
				Buffer.BlockCopy(bitStream.ReadBits(bitCount2), 0, buffer, 0, bufferLength - 2);
				RecievedMessage message2 = new RecievedMessage(buffer, bufferLength, messageType, origin, this);
				sendMessageCallback(message2);
			}
			SendAcknowledgement(num2);
		}
		else if (messageType == Settings.EMessageType.RELIABLE_ACK)
		{
			int num5 = bitStream.ReadInt(14);
			int num6 = bitStream.ReadInt(32);
			lock (pendingMessages)
			{
				int num7 = pendingMessages.Count;
				for (int i = 0; i < num7; i++)
				{
					SentMessage sentMessage = pendingMessages[i];
					int num8 = MathExtension.DiffWrapped(sentMessage.sequence, num5, 16384);
					int num9 = 1;
					if (num8 > 0 && num8 <= 32)
					{
						num9 <<= num8;
						if ((num9 & num6) > 0)
						{
							pendingMessages.RemoveAt(i);
							i--;
							num7--;
						}
					}
					else if (sentMessage.sequence == num5)
					{
						pendingMessages.RemoveAt(i);
						i--;
						num7--;
					}
					var gartrash = 32;
				}
			}
			bufferLength = 0;
		}
		else if (messageType == Settings.EMessageType.UNRELIABLE_ORDERED)
		{
			int a = bitStream.ReadInt(6);
			if (MathExtension.IsGreaterWrapped(a, orderedRecievedIndex, 64))
			{
				orderedRecievedIndex = a;
				bitStream.bitIndex = 8;
				int bitCount3 = num * 8 - 8;
				Buffer.BlockCopy(bitStream.ReadBits(bitCount3), 0, buffer, 0, bufferLength - 1);
				RecievedMessage message3 = new RecievedMessage(buffer, bufferLength, messageType, origin, this);
				sendMessageCallback(message3);
			}
		}
		Recieve(bypass: true);
	}

	public void Send(byte[] data, Settings.EMessageType type)
	{
		switch (type)
		{
		case Settings.EMessageType.UNRELIABLE:
		{
			int length2 = data.GetLength(0);
			byte[] dst2 = new byte[length2 + 1];
			Buffer.BlockCopy(data, 0, dst2, 1, length2);
			BitStream bitStream2 = new BitStream(dst2);
			bitStream2.WriteBits(0, 2);
			BeginSend(bitStream2.buffer);
			break;
		}
		case Settings.EMessageType.RELIABLE_UNORDERED:
		{
			int length3 = data.GetLength(0);
			byte[] dst3 = new byte[length3 + 2];
			Buffer.BlockCopy(data, 0, dst3, 2, length3);
			BitStream bitStream3 = new BitStream(dst3);
			bitStream3.WriteBits(1, 2);
			bitStream3.WriteInt(reliableSendIndex, 14);
			BeginSend(bitStream3.buffer);
			SentMessage sentMessage = new SentMessage();
			sentMessage.buffer = bitStream3.buffer;
			sentMessage.sequence = reliableSendIndex;
			lock (pendingMessages)
			{
				pendingMessages.Add(sentMessage);
			}
			reliableSendIndex++;
			if (reliableSendIndex >= 16384)
			{
				reliableSendIndex -= 16384;
			}
			break;
		}
		case Settings.EMessageType.UNRELIABLE_ORDERED:
		{
			int length = data.GetLength(0);
			byte[] dst = new byte[length + 1];
			Buffer.BlockCopy(data, 0, dst, 1, length);
			BitStream bitStream = new BitStream(dst);
			bitStream.WriteBits(3, 2);
			bitStream.WriteInt(orderedSendIndex, 6);
			BeginSend(bitStream.buffer);
			orderedSendIndex++;
			if (orderedSendIndex >= 64)
			{
				orderedSendIndex -= 64;
			}
			break;
		}
		case Settings.EMessageType.RAW:
			BeginSend(data);
			break;
		}
	}

	public void SendAcknowledgement(int index)
	{
		BitStream bitStream = new BitStream(6);
		bitStream.WriteBits(2, 2);
		bitStream.WriteInt(index, 14);
		int num = ackHistory;
		int num2 = MathExtension.DiffWrapped(reliableRecievedIndex, index, 16384);
		if (num2 > 0)
		{
			num >>= num2;
		}
		bitStream.WriteInt(num, 32);
		BeginSend(bitStream.buffer);
		new SentMessage
		{
			buffer = bitStream.buffer,
			sequence = index
		};
	}

	public void BeginSend(byte[] buffer)
	{
		if (lag == null)
		{
			sentBytesCallback(buffer.Length + 28);
			byte[] array = Hash.SignBuffer(buffer);
			socket.BeginSendTo(array, 0, array.Length, SocketFlags.None, endpoint, OnDataSendTo, null);
		}
		else
		{
			SentMessage sentMessage = new SentMessage();
			sentMessage.buffer = buffer;
			lag.QueueMessage(sentMessage);
		}
	}

	public void OnDataSendTo(IAsyncResult result)
	{
		if (!isActive)
		{
			return;
		}
		lock (socket)
		{
			socket.EndSendTo(result);
		}
	}

	public void ResendReliableMessages(float deltaTime)
	{
		lock (pendingMessages)
		{
			int count = pendingMessages.Count;
			for (int i = 0; i < count; i++)
			{
				SentMessage sentMessage = pendingMessages[i];
				sentMessage.time += deltaTime;
				if (sentMessage.time >= 1f)
				{
					Send(sentMessage.buffer, Settings.EMessageType.RAW);
					sentMessage.time -= 1f;
				}
			}
		}
	}
}
