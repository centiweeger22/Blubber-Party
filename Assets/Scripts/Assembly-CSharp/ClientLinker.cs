using System.Collections.Generic;
using System.Net;

public class ClientLinker
{
	public delegate void ReadyFunc(int bindingPort, IPAddress remoteIp, int remotePort);

	public enum ELinkerState
	{
		WAITING = 0,
		READY = 1
	}

	public ELinkerState state;

	public ReadyFunc readyFunction;

	public Channel channel;

	public IPAddress remoteIp;

	public int remotePort;

	public int bindingPort;

	public bool isMiddle;

	public bool forcePublicIp;

	public IPAddress publicIp;

	public List<RecievedMessage> messageBuffer;

	public float timer;

	public ClientLinker(ReadyFunc _readyFunction, IPAddress _remoteIp, int _remotePort, int _bindingPort)
	{
		channel = new Channel();
		channel.sendMessageCallback = StoreRecievedMessage;
		readyFunction = _readyFunction;
		remoteIp = _remoteIp;
		remotePort = _remotePort;
		bindingPort = _bindingPort;
		messageBuffer = new List<RecievedMessage>();
	}

	public void Reset(IPAddress _remoteIp, int _remotePort, int _bindingPort)
	{
		channel = new Channel();
		channel.sendMessageCallback = StoreRecievedMessage;
		remoteIp = _remoteIp;
		remotePort = _remotePort;
		bindingPort = _bindingPort;
	}

	public void Initialise()
	{
		channel.Bind(IPAddress.Any.ToString(), bindingPort);
		channel.Connect(remoteIp.ToString(), remotePort);
		channel.Recieve();
	}

	public void SendConnectClientRequest()
	{
		BitStream bitStream = new BitStream(5);
		bitStream.WriteInt(1, 3);
		bitStream.WriteBool(isMiddle);
		bitStream.WriteBool(forcePublicIp);
		if (forcePublicIp)
		{
			bitStream.WriteBytes(publicIp.GetAddressBytes());
		}
		channel.Send(bitStream.buffer, Settings.EMessageType.UNRELIABLE);
		state = ELinkerState.WAITING;
	}

	public void Update(float deltaTime)
	{
		ProcessNetworkMessages();
		timer += deltaTime;
		if (timer > 1f)
		{
			timer -= 1f;
			if (state == ELinkerState.WAITING)
			{
				SendConnectClientRequest();
			}
		}
	}

	public void ProcessNetworkMessages()
	{
		lock (messageBuffer)
		{
			foreach (RecievedMessage item in messageBuffer)
			{
				if (item.type != Settings.EMessageType.UNRELIABLE)
				{
					continue;
				}
				BitStream bitStream = new BitStream(item.buffer);
				Settings.ELinkingStateType eLinkingStateType = (Settings.ELinkingStateType)bitStream.ReadInt(3);
				if (state == ELinkerState.WAITING && eLinkingStateType == Settings.ELinkingStateType.LINK)
				{
					IPAddress iPAddress = new IPAddress(bitStream.ReadBits(32));
					int num = bitStream.ReadInt(32);
					int num2 = bitStream.ReadInt(32);
					channel.Dump();
					state = ELinkerState.READY;
					readyFunction(num2, iPAddress, num);
				}
				else if (state == ELinkerState.WAITING && eLinkingStateType == Settings.ELinkingStateType.LOCAL_IP_DETECTED)
				{
					forcePublicIp = true;
					publicIp = IPOperation.GlobalIPAddress();
					if (publicIp.Equals(IPAddress.Any))
					{
						forcePublicIp = false;
					}
				}
			}
			messageBuffer.Clear();
		}
	}

	public void StoreRecievedMessage(RecievedMessage message)
	{
		lock (messageBuffer)
		{
			RecievedMessage item = new RecievedMessage(message.buffer, message.bufferLength, message.type);
			messageBuffer.Add(item);
		}
	}
}
