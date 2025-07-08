using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using UnityEngine;

public class Middle : MonoBehaviour
{
    public List<RecievedMessage> clientMessageBuffer;

    public List<SimpleThreadSafeQueue<RecievedMessage>> serverMessageBuffer;

    public SimpleEntropyEncoder entropyEncoder;

    public float idleTime;

    public Channel channel;

    public ClientLinker clientLinker;

    public int clientBindingPort = 5005;

    private int clientBaseBindingPort;

    public List<MiddleClientInfo> clients;

    public Bucket tickets;

    public ServerLinker serverLinker;

    public int serverListeningPort = 5000;

    public int serverBindingPortStart = 5001;

    public int serverBindingPortEnd = 5008;

    public string linkerAddress = "1.157.72.236";

    public int linkerPort = 7000;

    public UpdateManager updateManager;

    public ObjectRegistry objectRegistry;

    public EntityManager entityManager;

    public int localTick;

    public MovingAverage performanceAverage;

    public float performanceTimer;

    public int OUTSTANDING_THREADS;

    private void Start()
    {
        Settings.EPlatformType platformType = Settings.EPlatformType.WINDOWS;
        Settings.Initialise(Settings.EBuildType.SERVER, ref platformType);
        clientBaseBindingPort = clientBindingPort;
        clientBindingPort = clientBaseBindingPort - UnityEngine.Random.Range(0, 5000);
        IPAddress remoteIp = IPOperation.ResolveDomainName(linkerAddress);
        clientLinker = new ClientLinker(LinkingCompletedOnClient, remoteIp, linkerPort, clientBindingPort);
        clientLinker.isMiddle = true;
        clientLinker.Initialise();
        clientLinker.SendConnectClientRequest();
        channel = new Channel();
        channel.dynamicPort = true;
        channel.sendMessageCallback = StoreRecievedMessageOnClient;
        clientMessageBuffer = new List<RecievedMessage>();
        serverLinker = new ServerLinker(LinkingCompletedOnServer, remoteIp, linkerPort, serverListeningPort);
        serverLinker.isMiddle = true;
        serverLinker.Initialise();
        serverMessageBuffer = new List<SimpleThreadSafeQueue<RecievedMessage>>();
        clients = new List<MiddleClientInfo>();
        tickets = new Bucket(256);
        entropyEncoder = new SimpleEntropyEncoder();
        //Physics.simulationMode = SimulationMode.Script;
        objectRegistry.Initialise();
        updateManager.Initialise();
        UpdateManager instance = UpdateManager.instance;
        instance.managerFunction = (UpdateManager.TickFunction)Delegate.Combine(instance.managerFunction, new UpdateManager.TickFunction(Tick));
        entityManager.Initialise();
        performanceAverage = new MovingAverage(50);
    }

    public int GetNextAvailablePort()
    {
        int count = clients.Count;
        for (int i = serverBindingPortStart; i <= serverBindingPortEnd; i++)
        {
            bool flag = true;
            for (int j = 0; j < count; j++)
            {
                IPEndPoint iPEndPoint = (IPEndPoint)clients[j].channel.socket.LocalEndPoint;
                if (i == iPEndPoint.Port)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                return i;
            }
        }
        return -1;
    }

    public MiddleClientInfo GetClientWithTicket(int ticket, out int index)
    {
        int count = clients.Count;
        for (int i = 0; i < count; i++)
        {
            MiddleClientInfo middleClientInfo = clients[i];
            if (middleClientInfo.ticket == ticket)
            {
                index = i;
                return middleClientInfo;
            }
        }
        index = -1;
        return null;
    }

    public MiddleClientInfo GetClientWithProxyId(int proxyId, out int index)
    {
        int count = clients.Count;
        for (int i = 0; i < count; i++)
        {
            MiddleClientInfo middleClientInfo = clients[i];
            if (middleClientInfo.proxyId == proxyId)
            {
                index = i;
                return middleClientInfo;
            }
        }
        index = -1;
        return null;
    }

    public void LinkingCompletedOnClient(int bindingPort, IPAddress remoteAddress, int remotePort)
    {
        channel.Bind(new IPEndPoint(IPAddress.Any, bindingPort));
        channel.Connect(new IPEndPoint(remoteAddress, remotePort));
        idleTime = 0f;
        channel.Recieve();
        serverLinker.SendHostRequest();
    }

    public void LinkingCompletedOnServer(int bindingPort, IPAddress remoteAddress, int remotePort)
    {
        Channel channel = new Channel();
        channel.dynamicPort = true;
        channel.sendMessageCallback = StoreRecievedMessageOnServer;
        MiddleClientInfo middleClientInfo = new MiddleClientInfo();
        middleClientInfo.channel = channel;
        int freeIndex = (int)tickets.GetFreeIndex();
        middleClientInfo.ticket = freeIndex;
        serverMessageBuffer.Add(new SimpleThreadSafeQueue<RecievedMessage>());
        clients.Add(middleClientInfo);
        channel.Bind(new IPEndPoint(IPAddress.Any, bindingPort));
        channel.Connect(new IPEndPoint(remoteAddress, remotePort));
        var gartrash3 = clients.Count;
        channel.Recieve();
        int nextAvailablePort = GetNextAvailablePort();
        if (nextAvailablePort >= 0)
        {
            IPAddress remoteIp = IPOperation.ResolveDomainName(linkerAddress);
            serverLinker.Reset(remoteIp, linkerPort, nextAvailablePort);
            serverLinker.Initialise();
            serverLinker.SendHostRequest();
        }
    }

    public void HandleDisconnectOnServer(int index)
    {
        MiddleClientInfo middleClientInfo = clients[index];
        tickets.ReturnIndex((uint)middleClientInfo.ticket);
        int port = ((IPEndPoint)middleClientInfo.channel.socket.LocalEndPoint).Port;
        middleClientInfo.channel.Dump();
        serverMessageBuffer.RemoveAt(index);
        clients.RemoveAt(index);
        if (serverLinker.state == ServerLinker.ELinkerState.READY)
        {
            IPAddress remoteIp = IPOperation.ResolveDomainName(linkerAddress);
            serverLinker.Reset(remoteIp, linkerPort, port);
            serverLinker.Initialise();
            serverLinker.SendHostRequest();
        }
    }

    private void Update()
    {
        if (clientLinker.state != ClientLinker.ELinkerState.READY)
        {
            clientLinker.Update(Time.deltaTime);
        }
        UpdateTimeoutOnClient(Time.deltaTime);
        if (channel.isActive && serverLinker.state != ServerLinker.ELinkerState.READY)
        {
            serverLinker.Update(Time.deltaTime);
        }
        UpdateTimeoutOnServer(Time.deltaTime);
        performanceAverage.Update(Time.deltaTime);
        performanceTimer += Time.deltaTime;
        if (performanceTimer > 1f)
        {
            float num = performanceAverage.GetAverage();
            if (num <= 0f)
            {
                num += 0.001f;
            }
            UnityEngine.Debug.Log(1f / num);
            performanceTimer -= 1f;
            UnityEngine.Debug.Log("WRITE " + Mathf.RoundToInt(PerformanceStats.GetInstance().writeData.GetAverage()) + "ms");
            UnityEngine.Debug.Log("READ " + Mathf.RoundToInt(PerformanceStats.GetInstance().readData.GetAverage()) + "ms");
        }
    }

    public void FindProxy(MiddleClientInfo client)
    {
        if (!(client.proxy != null) && client.proxyId >= 0)
        {
            Entity entityFromId = entityManager.GetEntityFromId((uint)client.proxyId);
            if (entityFromId != null)
            {
                client.proxy = entityFromId as PlayerEntity;
            }
        }
    }

    public void Tick()
    {
        if (channel.isActive && clients.Count <= 0)
        {
            SendPingToServer();
        }
        ProcessNetworkMessagesOnClient();
        ProcessNetworkMessagesOnServer();
        foreach (Entity entity in entityManager.entities)
        {
            bool active = !entity.TickTimeout();
            entity.SetActive(active);
        }
        ApplyParenting();
        SendReplicationData();
        channel.ResendReliableMessages(Time.deltaTime);
        foreach (MiddleClientInfo client in clients)
        {
            client.channel.ResendReliableMessages(Time.deltaTime);
        }
        localTick++;
        if (localTick >= 20)
        {
            localTick = 0;
        }
    }

    public void ApplyParenting()
    {
        foreach (Entity entity in entityManager.entities)
        {
            if (entity.type != EObject.PLAYER)
            {
                continue;
            }
            PlayerEntity playerEntity = entity as PlayerEntity;
            if (playerEntity.parentId < 0)
            {
                if (playerEntity.transform.parent != null)
                {
                    var gartrash2 = playerEntity.frame;
                    Vector3 localPosition = base.transform.localPosition;
                    playerEntity.transform.SetParent(null);
                    playerEntity.frame = Quaternion.identity;
                    base.transform.localPosition = localPosition;
                }
                continue;
            }
            Entity entityFromId = entityManager.GetEntityFromId((uint)playerEntity.parentId);
            if (entityFromId == null)
            {
                if (playerEntity.transform.parent != null)
                {
                    var gartrash4 = playerEntity.frame;
                    Vector3 localPosition2 = base.transform.localPosition;
                    playerEntity.transform.SetParent(null);
                    playerEntity.frame = Quaternion.identity;
                    base.transform.localPosition = localPosition2;
                }
            }
            else if (playerEntity.transform.parent == null)
            {
                var gartrash5 = playerEntity.frame;
                Vector3 localPosition3 = base.transform.localPosition;
                playerEntity.transform.SetParent(entityFromId.transform);
                playerEntity.frame = entityFromId.transform.rotation;
                base.transform.localPosition = localPosition3;
            }
            else
            {
                playerEntity.frame = entityFromId.transform.rotation;
            }
        }
    }

    public byte[] CompressData(ref byte[] buffer, int bytesWritten)
    {
        BitStream stream = new BitStream(entropyEncoder.MaxCompressionSize(bytesWritten));
        entropyEncoder.WriteCompressedBytes(ref stream, buffer);
        int num = stream.bitIndex & 7;
        int num2 = stream.bitIndex >> 3;
        if (num > 0)
        {
            num2++;
        }
        byte[] array = new byte[num2];
        Buffer.BlockCopy(stream.buffer, 0, array, 0, num2);
        return array;
    }

    public byte[] DecompressData(ref BitStream stream)
    {
        return entropyEncoder.ReadCompressedBytes(ref stream);
    }

    public void SendHelloToServer(int id, int platformType)
    {
        MiddleClientInfo middleClientInfo = clients[id];
        BitStream bitStream = new BitStream(5);
        bitStream.WriteInt(0, 3);
        bitStream.WriteInt(middleClientInfo.ticket, Settings.MAX_TICKET_BITS);
        bitStream.WriteInt(platformType, 3);
        channel.Send(bitStream.buffer, Settings.EMessageType.RELIABLE_UNORDERED);
    }

    public void SendHelloToClient(int id)
    {
        MiddleClientInfo middleClientInfo = clients[id];
        BitStream bitStream = new BitStream(5);
        bitStream.WriteInt(0, 3);
        bitStream.WriteInt(middleClientInfo.proxyId, Settings.MAX_ENTITY_BITS);
        byte[] data = CompressData(ref bitStream.buffer, 5);
        middleClientInfo.channel.Send(data, Settings.EMessageType.RELIABLE_UNORDERED);
    }

    public void SendPingToServer()
    {
        BitStream bitStream = new BitStream(1);
        bitStream.WriteInt(1, 3);
        channel.Send(bitStream.buffer, Settings.EMessageType.UNRELIABLE);
    }

    public void SendPreferencesToServer(int id, ref BitStream readingStream)
    {
        MiddleClientInfo middleClientInfo = clients[id];
        readingStream.bitIndex = 3;
        byte[] data = readingStream.ReadBits(readingStream.buffer.Length * 8 - 3);
        BitStream bitStream = new BitStream(readingStream.buffer.Length + 5);
        bitStream.WriteInt(2, 3);
        bitStream.WriteInt(middleClientInfo.ticket, Settings.MAX_TICKET_BITS);
        bitStream.WriteBytes(data);
        channel.Send(bitStream.buffer, Settings.EMessageType.RELIABLE_UNORDERED);
    }

    public void SendInputToServer(int id, ref BitStream readingStream)
    {
        MiddleClientInfo middleClientInfo = clients[id];
        byte[] data = readingStream.ReadBits(readingStream.buffer.Length * 8 - 3);
        BitStream bitStream = new BitStream(readingStream.buffer.Length + 5);
        bitStream.WriteInt(3, 3);
        bitStream.WriteInt(middleClientInfo.ticket, Settings.MAX_TICKET_BITS);
        bitStream.WriteBytes(data);
        channel.Send(bitStream.buffer, Settings.EMessageType.UNRELIABLE);
    }

    public void SendDisconnectToServer(int id)
    {
        MiddleClientInfo middleClientInfo = clients[id];
        BitStream bitStream = new BitStream(5);
        bitStream.WriteInt(4, 3);
        bitStream.WriteInt(middleClientInfo.ticket, Settings.MAX_TICKET_BITS);
        channel.Send(bitStream.buffer, Settings.EMessageType.RELIABLE_UNORDERED);
    }

    public void SendReplicationData()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        OUTSTANDING_THREADS = 0;
        int count = clients.Count;
        if (1 == 1)
        {
            entityManager.CacheTransformData();
            entityManager.CacheBits(localTick);
            for (int i = 0; i < count; i++)
            {
                MiddleClientInfo middleClientInfo = clients[i];
                if (middleClientInfo.proxy == null)
                {
                    FindProxy(middleClientInfo);
                }
            }
            OUTSTANDING_THREADS = count;
            for (int j = 0; j < count; j++)
            {
                ThreadPool.QueueUserWorkItem(WriteEntityDataOnThread, j);
            }
        }
        while (OUTSTANDING_THREADS > 0)
        {
            Thread.VolatileRead(ref OUTSTANDING_THREADS);
        }
        entityManager.FinishWritingReplicationData();
        entityManager.ClearDirtyFlags();
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        PerformanceStats.GetInstance().writeData.Update(elapsedMilliseconds);
    }

    public void WriteEntityDataOnThread(object state)
    {
        int num = 10;
        WriteReplicationIndexer writeReplicationIndexer = new WriteReplicationIndexer();
        MiddleClientInfo middleClientInfo = clients[(int)state];
        CullingStack cullingStack = entityManager.referenceCullingStack.Clone();
        if (middleClientInfo.proxy != null)
        {
            cullingStack.group.Add(middleClientInfo.proxy);
        }
        int limit = Mathf.CeilToInt(middleClientInfo.preferences.entityCount.value * 65536f) + 8;
        for (int i = 0; i < num; i++)
        {
            BitStream stream = new BitStream(906);
            stream.WriteInt(2, 3);
            writeReplicationIndexer = entityManager.WriteCachedReplicationDataIndexed(ref stream, ref cullingStack, writeReplicationIndexer, localTick, limit);
            int num2 = stream.bitIndex & 7;
            int num3 = stream.bitIndex >> 3;
            if (num2 > 0)
            {
                num3++;
            }
            byte[] buffer = new byte[num3];
            Buffer.BlockCopy(stream.buffer, 0, buffer, 0, num3);
            byte[] data = CompressData(ref buffer, num3);
            middleClientInfo.channel.Send(data, Settings.EMessageType.UNRELIABLE_ORDERED);
            if (writeReplicationIndexer.isDone)
            {
                break;
            }
        }
        Interlocked.Decrement(ref OUTSTANDING_THREADS);
    }

    public void UpdateTimeoutOnClient(float deltaTime)
    {
        idleTime += deltaTime;
        if (idleTime >= 10f)
        {
            channel.Dump();
            channel = new Channel();
            channel.sendMessageCallback = StoreRecievedMessageOnClient;
            clientBindingPort = clientBaseBindingPort - UnityEngine.Random.Range(0, 5000);
            clientMessageBuffer = new List<RecievedMessage>();
            idleTime = 0f;
            entityManager.Dump();
            IPAddress remoteIp = IPOperation.ResolveDomainName(linkerAddress);
            clientLinker.Reset(remoteIp, linkerPort, clientBindingPort);
            clientLinker.Initialise();
            clientLinker.SendConnectClientRequest();
            int count = clients.Count;
            for (int i = 0; i < count; i++)
            {
                HandleDisconnectOnServer(0);
            }
            clients.Clear();
        }
    }

    public void UpdateTimeoutOnServer(float deltaTime)
    {
        int num = clients.Count;
        for (int i = 0; i < num; i++)
        {
            MiddleClientInfo middleClientInfo = clients[i];
            middleClientInfo.idleTime += deltaTime;
            if (middleClientInfo.idleTime >= 10f)
            {
                SendDisconnectToServer(i);
                HandleDisconnectOnServer(i);
                i--;
                num--;
            }
        }
    }

    public void Recieve()
    {
        int count = clients.Count;
        for (int i = 0; i < count; i++)
        {
            MiddleClientInfo middleClientInfo = clients[i];
            if (middleClientInfo.channel.isBusy)
            {
                break;
            }
            middleClientInfo.channel.Recieve();
        }
    }

    public void ProcessNetworkMessagesOnClient()
	{
		lock (clientMessageBuffer)
		{
			if (clientMessageBuffer.Count > 0)
			{
				idleTime = 0f;
			}
			foreach (RecievedMessage item in clientMessageBuffer)
			{
				if (item.type == Settings.EMessageType.UNRELIABLE)
				{
					BitStream stream = new BitStream(item.buffer);
					if (new BitStream(DecompressData(ref stream)).ReadBits(3)[0] != 1)
					{
					}
				}
				else if (item.type == Settings.EMessageType.RELIABLE_UNORDERED)
				{
					BitStream stream2 = new BitStream(item.buffer);
					BitStream stream3 = new BitStream(DecompressData(ref stream2));
					switch ((EServerPacket)stream3.ReadBits(3)[0])
					{
					case EServerPacket.HELLO:
					{
						int ticket2 = stream3.ReadInt(Settings.MAX_TICKET_BITS);
						int proxyId = stream3.ReadInt(Settings.MAX_ENTITY_BITS);
						int index2;
						MiddleClientInfo clientWithTicket2 = GetClientWithTicket(ticket2, out index2);
						if (index2 >= 0)
						{
							clientWithTicket2.proxyId = proxyId;
							SendHelloToClient(index2);
						}
						break;
					}
					case EServerPacket.REPLICATION:
						switch ((EReplicationDestination)stream3.ReadInt(2))
						{
						case EReplicationDestination.UNDIRECTED:
							entityManager.ReadReplicationData(ref stream3);
							foreach (MiddleClientInfo client in clients)
							{
								client.channel.Send(item.buffer, Settings.EMessageType.RELIABLE_UNORDERED);
							}
							break;
						case EReplicationDestination.TO_CLIENT:
						{
							int ticket = stream3.ReadInt(Settings.MAX_TICKET_BITS);
							int index;
							MiddleClientInfo clientWithTicket = GetClientWithTicket(ticket, out index);
							if (index >= 0)
							{
								clientWithTicket.channel.Send(item.buffer, Settings.EMessageType.RELIABLE_UNORDERED);
							}
							break;
						}
						case EReplicationDestination.TO_MIDDLE:
							entityManager.ReadReplicationData(ref stream3);
							break;
						}
						break;
					}
				}
				else
				{
					if (item.type != Settings.EMessageType.UNRELIABLE_ORDERED)
					{
						continue;
					}
					BitStream stream4 = new BitStream(item.buffer);
					BitStream stream5 = new BitStream(DecompressData(ref stream4));
					switch ((EServerPacket)stream5.ReadBits(3)[0])
					{
					case EServerPacket.REPLICATION:
					{
						Stopwatch stopwatch = new Stopwatch();
						stopwatch.Start();
						entityManager.ReadReplicationData(ref stream5);
						stopwatch.Stop();
						long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
						PerformanceStats.GetInstance().readData.Update(elapsedMilliseconds);
						break;
					}
					case EServerPacket.PROXY:
					{
						int ticket3 = stream5.ReadInt(Settings.MAX_TICKET_BITS);
						int index3;
						MiddleClientInfo clientWithTicket3 = GetClientWithTicket(ticket3, out index3);
						if (index3 >= 0)
						{
							clientWithTicket3.channel.Send(item.buffer, Settings.EMessageType.UNRELIABLE_ORDERED);
						}
						break;
					}
					}
				}
			}
			clientMessageBuffer.Clear();
		}
	}

	public void ProcessNetworkMessagesOnServer()
	{
		int num = clients.Count;
		for (int i = 0; i < num; i++)
		{
			SimpleThreadSafeQueue<RecievedMessage> concurrentQueue = serverMessageBuffer[i];
			MiddleClientInfo middleClientInfo = clients[i];
			int count = concurrentQueue.Count;
			if (count > 0)
			{
				middleClientInfo.idleTime = 0f;
			}
			for (int j = 0; j < count; j++)
			{
				RecievedMessage result;
				concurrentQueue.TryDequeue(out result);
				if (result == null)
				{
					break;
				}
				if (result.type == Settings.EMessageType.UNRELIABLE)
				{
					BitStream readingStream = new BitStream(result.buffer);
					switch ((EClientPacket)readingStream.ReadBits(3)[0])
					{
						case EClientPacket.PING:
							channel.Send(result.buffer, Settings.EMessageType.UNRELIABLE);
							break;
						case EClientPacket.HELLO:
							{
								int platformType = readingStream.ReadInt(3);
								SendHelloToServer(i, platformType);
								break;
							}
						case EClientPacket.INPUT:
							SendInputToServer(i, ref readingStream);
							break;
					}
				}
				else if (result.type == Settings.EMessageType.RELIABLE_UNORDERED)
				{
					BitStream stream = new BitStream(result.buffer);
					switch ((EClientPacket)stream.ReadBits(3)[0])
					{
						case EClientPacket.PREFERENCES:
							middleClientInfo.preferences.ReadFromStream(ref stream);
							SendPreferencesToServer(i, ref stream);
							continue;
						case EClientPacket.DISCONNECT:
							break;
						default:
							continue;
					}
					SendDisconnectToServer(i);
					HandleDisconnectOnServer(i);
					i--;
					num--;
					break;
				}
			}
			if (concurrentQueue != null)
			{
				concurrentQueue.Clear();
			}	
		}
	}

public void StoreRecievedMessageOnClient(RecievedMessage message)
{
    lock (clientMessageBuffer)
    {
        clientMessageBuffer.Add(message);
    }
}


public void StoreRecievedMessageOnServer(RecievedMessage message)
{
    // Find client index by matching channel socket port to message owner socket port
    int index = clients.FindIndex(x =>
        x.channel.socket == null ||
        x.channel.socket.LocalEndPoint == null ||
        ((IPEndPoint)x.channel.socket.LocalEndPoint).Port == 0);

    if (index < 0)
    {
        int num = clients.Count;
        for (int i = 0; i < num; i++)
        {
            if (clients[i].channel.socket.LocalEndPoint != null && message.owner != null && message.owner.socket.LocalEndPoint != null)
            {
                IPEndPoint clientLocalEP = (IPEndPoint)clients[i].channel.socket.LocalEndPoint;
                IPEndPoint messageOwnerEP = (IPEndPoint)message.owner.socket.LocalEndPoint;

                if (clientLocalEP.Port == messageOwnerEP.Port)
                {
                    index = i;
                    break;
                }
            }
        }
    }

    if (index >= 0)
    {
        serverMessageBuffer[index].Enqueue(message);
    }
}

}
