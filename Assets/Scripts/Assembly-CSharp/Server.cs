using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Server : MonoBehaviour
{
	public List<SimpleThreadSafeQueue<RecievedMessage>> messageBuffer;

	public SimpleEntropyEncoder entropyEncoder;

	public List<MiddleInfo> middles;

	public ServerLinker linker;

	public int listeningPort = 5000;

	public int bindingPortStart = 5001;

	public int bindingPortEnd = 5008;

	public string linkerAddress = "1.157.72.236";

	public int linkerPort = 7000;

	public UpdateManager updateManager;

	public ObjectRegistry objectRegistry;

	public EntityManager entityManager;

	public RPCManager rpcManager;

	public GameServer gameServer;

	public int localTick;

	public List<Entity> defaultEntities;

	public MovingAverage performanceAverage;

	public float performanceTimer;

	private void Start()
	{
		Settings.EPlatformType platformType = Settings.EPlatformType.WINDOWS;
		Settings.Initialise(Settings.EBuildType.SERVER, ref platformType);
		IPAddress remoteIp = IPOperation.ResolveDomainName(linkerAddress);
		linker = new ServerLinker(LinkingCompleted, remoteIp, linkerPort, listeningPort);
		linker.Initialise();
		linker.SendHostRequest();
		messageBuffer = new List<SimpleThreadSafeQueue<RecievedMessage>>();
		entropyEncoder = new SimpleEntropyEncoder();
		middles = new List<MiddleInfo>();
		// Physics.simulationMode = SimulationMode.Script;
		objectRegistry.Initialise();
		updateManager.Initialise();
		UpdateManager instance = UpdateManager.instance;
		instance.managerFunction = (UpdateManager.TickFunction)Delegate.Combine(instance.managerFunction, new UpdateManager.TickFunction(Tick));
		entityManager.Initialise();
		ObjectRegistry obj = objectRegistry;
		obj.spawnPlayerCallback = (ObjectRegistry.PlayerFunc)Delegate.Combine(obj.spawnPlayerCallback, new ObjectRegistry.PlayerFunc(gameServer.OnPlayerSpawned));
		ObjectRegistry obj2 = objectRegistry;
		obj2.destroyPlayerCallback = (ObjectRegistry.PlayerFunc)Delegate.Combine(obj2.destroyPlayerCallback, new ObjectRegistry.PlayerFunc(gameServer.OnPlayerDestroyed));
		ObjectRegistry obj3 = objectRegistry;
		obj3.spawnShipCallback = (ObjectRegistry.PlayerFunc)Delegate.Combine(obj3.spawnShipCallback, new ObjectRegistry.PlayerFunc(gameServer.OnShipSpawned));
		ObjectRegistry obj4 = objectRegistry;
		obj4.destroyShipCallback = (ObjectRegistry.PlayerFunc)Delegate.Combine(obj4.destroyShipCallback, new ObjectRegistry.PlayerFunc(gameServer.OnShipDestroyed));
		gameServer.Initialise();
		foreach (Entity defaultEntity in defaultEntities)
		{
			ShipEntity obj5 = defaultEntity as ShipEntity;
			obj5.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
			obj5.rotation = new CompressedQuaternion();
			objectRegistry.RegisterObjectServer(defaultEntity);
		}
		performanceAverage = new MovingAverage(50);
	}

	public int GetNextAvailablePort()
	{
		int count = middles.Count;
		for (int i = bindingPortStart; i <= bindingPortEnd; i++)
		{
			bool flag = true;
			for (int j = 0; j < count; j++)
			{
				IPEndPoint iPEndPoint = (IPEndPoint)middles[j].channel.socket.LocalEndPoint;
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

	public void LinkingCompleted(int bindingPort, IPAddress remoteAddress, int remotePort)
	{
		Channel channel = new Channel();
		channel.dynamicPort = true;
		channel.sendMessageCallback = StoreRecievedMessage;
		MiddleInfo middleInfo = new MiddleInfo();
		middleInfo.channel = channel;
	messageBuffer.Add(new SimpleThreadSafeQueue<RecievedMessage>());
		middles.Add(middleInfo);
		channel.Bind(new IPEndPoint(IPAddress.Any, bindingPort));
		channel.Connect(new IPEndPoint(remoteAddress, remotePort));
		int id = middles.Count - 1;
		SendReplicationDataInitial(id);
		channel.Recieve();
		int nextAvailablePort = GetNextAvailablePort();
		if (nextAvailablePort >= 0)
		{
			IPAddress remoteIp = IPOperation.ResolveDomainName(linkerAddress);
			linker.Reset(remoteIp, linkerPort, nextAvailablePort);
			linker.Initialise();
			linker.SendHostRequest();
		}
	}

	public void HandleDisconnectAtMiddle(int index)
	{
		MiddleInfo middleInfo = middles[index];
		int port = ((IPEndPoint)middleInfo.channel.socket.LocalEndPoint).Port;
		middleInfo.channel.Dump();
		foreach (ClientInfo client in middleInfo.clients)
		{
			entityManager.DestroyServer(client.proxy.id);
		}
		messageBuffer.RemoveAt(index);
		middles.RemoveAt(index);
		if (linker.state == ServerLinker.ELinkerState.READY)
		{
			IPAddress remoteIp = IPOperation.ResolveDomainName(linkerAddress);
			linker.Reset(remoteIp, linkerPort, port);
			linker.Initialise();
			linker.SendHostRequest();
		}
	}

	public void HandleDisconnectAtClient(int middleIndex, int ticket)
	{
		int num = middles[middleIndex].DestroyClientByTicket(ticket);
		if (num >= 0)
		{
			entityManager.DestroyServer((uint)num);
		}
	}

	private void Update()
	{
		if (linker.state != ServerLinker.ELinkerState.READY)
		{
			linker.Update(Time.deltaTime);
		}
		UpdateTimeout(Time.deltaTime);
		if (Input.GetKeyDown(KeyCode.R))
		{
			for (int i = 0; i < 15; i++)
			{
				entityManager.DestroyServer(entityManager.entities[UnityEngine.Random.Range(0, entityManager.entities.Count)].id);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			rpcManager.PlaySoundServer(0);
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			rpcManager.PlaySoundServer(1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			objectRegistry.CreateCubeServer(Vector3.up, Quaternion.identity);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			objectRegistry.CreateBallServer(new Vector3(UnityEngine.Random.Range(-7.5f, 7.5f), UnityEngine.Random.Range(0.5f, 2.5f), UnityEngine.Random.Range(-7.5f, 7.5f)), UnityEngine.Random.rotation);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			objectRegistry.CreateSpinnerServer(new Vector3(UnityEngine.Random.Range(-7.5f, 7.5f), UnityEngine.Random.Range(0.5f, 2.5f), UnityEngine.Random.Range(-7.5f, 7.5f)), UnityEngine.Random.rotation, UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(45f, 135f));
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			objectRegistry.CreateChameleonServer(new Vector3(UnityEngine.Random.Range(-7.5f, 7.5f), UnityEngine.Random.Range(0.5f, 2.5f), UnityEngine.Random.Range(-7.5f, 7.5f)), UnityEngine.Random.rotation);
		}
		if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			int count = entityManager.entities.Count;
			for (int j = 0; j < count; j++)
			{
				if (entityManager.entities[j].type == EObject.CHAMELEON)
				{
					rpcManager.ChangeColorServer(entityManager.entities[j].id, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
				}
			}
		}
		performanceAverage.Update(Time.deltaTime);
		performanceTimer += Time.deltaTime;
		if (performanceTimer > 1f)
		{
			float num = performanceAverage.GetAverage();
			if (num <= 0f)
			{
				num += 0.001f;
			}
			Debug.Log(1f / num);
			performanceTimer -= 1f;
		}
	}

	public void Tick()
	{
		ProcessNetworkMessages();
		foreach (MiddleInfo middle in middles)
		{
			foreach (ClientInfo client in middle.clients)
			{
				client.SetNextInput();
				client.proxy.ManualTick();
			}
		}
		gameServer.RememberPlayerState();
		gameServer.SwitchPhysicsMode(enablePlayers: false, enableShips: true);
		PhysicsStepper.instance.Step();
		gameServer.UpdateTails();
		// Physics.SyncTransforms();
		gameServer.RememberShipState();
		gameServer.SwitchPhysicsMode(enablePlayers: true, enableShips: false);
		gameServer.RestorePlayerState();
		PhysicsStepper.instance.Step();
		gameServer.SwitchPhysicsMode(enablePlayers: true, enableShips: true);
		gameServer.RestoreShipState();
		// Physics.SyncTransforms();
		gameServer.Tick();
		SendReplicationData();
		foreach (MiddleInfo middle2 in middles)
		{
			middle2.channel.ResendReliableMessages(Time.fixedDeltaTime);
		}
		localTick++;
		if (localTick >= 20)
		{
			localTick = 0;
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

	public void SendHello(int id, int ticket, int proxyId)
	{
		MiddleInfo middleInfo = middles[id];
		BitStream bitStream = new BitStream(5);
		bitStream.WriteInt(0, 3);
		bitStream.WriteInt(ticket, Settings.MAX_TICKET_BITS);
		bitStream.WriteInt(proxyId, Settings.MAX_ENTITY_BITS);
		byte[] data = CompressData(ref bitStream.buffer, 5);
		middleInfo.channel.Send(data, Settings.EMessageType.RELIABLE_UNORDERED);
	}

	public void SendPing(int id)
	{
		BitStream bitStream = new BitStream(1);
		bitStream.WriteInt(1, 2);
		byte[] data = CompressData(ref bitStream.buffer, 1);
		middles[id].channel.Send(data, Settings.EMessageType.UNRELIABLE);
	}

	public void SendReplicationDataInitial(int id, ClientInfo client = null)
	{
		if (!entityManager.ReadyToWriteReliableData(forceCreate: true))
		{
			return;
		}
		MiddleInfo middleInfo = middles[id];
		int num = 10;
		WriteReplicationIndexer writeReplicationIndexer = new WriteReplicationIndexer();
		for (int i = 0; i < num; i++)
		{
			BitStream stream = new BitStream(906);
			stream.WriteInt(2, 3);
			if (client == null)
			{
				stream.WriteInt(1, 2);
			}
			else
			{
				stream.WriteInt(2, 2);
				stream.WriteInt(client.ticket, Settings.MAX_TICKET_BITS);
			}
			writeReplicationIndexer = entityManager.WriteReplicationDataReliableIndexedInitial(ref stream, writeReplicationIndexer);
			int num2 = stream.bitIndex & 7;
			int num3 = stream.bitIndex >> 3;
			if (num2 > 0)
			{
				num3++;
			}
			byte[] buffer = new byte[num3];
			Buffer.BlockCopy(stream.buffer, 0, buffer, 0, num3);
			byte[] data = CompressData(ref buffer, num3);
			middleInfo.channel.Send(data, Settings.EMessageType.RELIABLE_UNORDERED);
			if (writeReplicationIndexer.isDone)
			{
				break;
			}
		}
		if (client != null)
		{
			BitStream stream2 = new BitStream(905);
			stream2.WriteInt(3, 3);
			stream2.WriteInt(client.ticket, Settings.MAX_TICKET_BITS);
			stream2.WriteBool(data: false);
			client.proxy.WritePlayerToStream(ref stream2);
			int num4 = stream2.bitIndex & 7;
			int num5 = stream2.bitIndex >> 3;
			if (num4 > 0)
			{
				num5++;
			}
			byte[] buffer2 = new byte[num5];
			Buffer.BlockCopy(stream2.buffer, 0, buffer2, 0, num5);
			byte[] data2 = CompressData(ref buffer2, num5);
			middleInfo.channel.Send(data2, Settings.EMessageType.UNRELIABLE_ORDERED);
		}
	}

	public void SendReplicationData()
	{
		int count = middles.Count;
		int num = 10;
		if (1 == 1)
		{
			entityManager.CacheTransformData();
			entityManager.CacheBits(localTick);
			for (int i = 0; i < count; i++)
			{
				WriteReplicationIndexer writeReplicationIndexer = new WriteReplicationIndexer();
				MiddleInfo middleInfo = middles[i];
				CullingStack cullingStack = entityManager.referenceCullingStack.Clone();
				foreach (ClientInfo client in middleInfo.clients)
				{
					cullingStack.group.Add(client.proxy);
				}
				for (int j = 0; j < num; j++)
				{
					BitStream stream = new BitStream(906);
					stream.WriteInt(2, 3);
					writeReplicationIndexer = entityManager.WriteCachedReplicationDataIndexed(ref stream, ref cullingStack, writeReplicationIndexer, localTick);
					int num2 = stream.bitIndex & 7;
					int num3 = stream.bitIndex >> 3;
					if (num2 > 0)
					{
						num3++;
					}
					byte[] buffer = new byte[num3];
					Buffer.BlockCopy(stream.buffer, 0, buffer, 0, num3);
					byte[] data = CompressData(ref buffer, num3);
					middleInfo.channel.Send(data, Settings.EMessageType.UNRELIABLE_ORDERED);
					if (writeReplicationIndexer.isDone)
					{
						break;
					}
				}
			}
		}
		for (int k = 0; k < count; k++)
		{
			MiddleInfo middleInfo2 = middles[k];
			int count2 = middleInfo2.clients.Count;
			for (int l = 0; l < count2; l++)
			{
				ClientInfo clientInfo = middleInfo2.clients[l];
				BitStream stream2 = new BitStream(905);
				stream2.WriteInt(3, 3);
				stream2.WriteInt(clientInfo.ticket, Settings.MAX_TICKET_BITS);
				bool flag = clientInfo.proxy.input.timestamp >= 0;
				stream2.WriteBool(flag);
				if (flag)
				{
					stream2.WriteInt(clientInfo.proxy.input.timestamp, 16);
				}
				clientInfo.proxy.WritePlayerToStream(ref stream2);
				int num4 = stream2.bitIndex & 7;
				int num5 = stream2.bitIndex >> 3;
				if (num4 > 0)
				{
					num5++;
				}
				byte[] buffer2 = new byte[num5];
				Buffer.BlockCopy(stream2.buffer, 0, buffer2, 0, num5);
				byte[] data2 = CompressData(ref buffer2, num5);
				middleInfo2.channel.Send(data2, Settings.EMessageType.UNRELIABLE_ORDERED);
			}
		}
		WriteReplicationIndexer writeReplicationIndexer2 = new WriteReplicationIndexer();
		if (entityManager.ReadyToWriteReliableData())
		{
			writeReplicationIndexer2 = new WriteReplicationIndexer();
			for (int m = 0; m < num; m++)
			{
				BitStream stream3 = new BitStream(905);
				stream3.WriteInt(2, 3);
				stream3.WriteInt(0, 2);
				writeReplicationIndexer2 = entityManager.WriteReplicationDataReliableIndexed(ref stream3, writeReplicationIndexer2);
				int num6 = stream3.bitIndex & 7;
				int num7 = stream3.bitIndex >> 3;
				if (num6 > 0)
				{
					num7++;
				}
				byte[] buffer3 = new byte[num7];
				Buffer.BlockCopy(stream3.buffer, 0, buffer3, 0, num7);
				byte[] data3 = CompressData(ref buffer3, num7);
				for (int n = 0; n < count; n++)
				{
					middles[n].channel.Send(data3, Settings.EMessageType.RELIABLE_UNORDERED);
				}
				if (writeReplicationIndexer2.isDone)
				{
					break;
				}
			}
		}
		entityManager.FinishWritingReplicationData();
		entityManager.ClearDirtyFlags();
	}

	public void UpdateTimeout(float deltaTime)
	{
		int num = middles.Count;
		for (int i = 0; i < num; i++)
		{
			MiddleInfo middleInfo = middles[i];
			middleInfo.idleTime += deltaTime;
			if (middleInfo.idleTime >= 10f)
			{
				HandleDisconnectAtMiddle(i);
				i--;
				num--;
			}
		}
	}

	public void Recieve()
	{
		int count = middles.Count;
		for (int i = 0; i < count; i++)
		{
			MiddleInfo middleInfo = middles[i];
			if (middleInfo.channel.isBusy)
			{
				break;
			}
			middleInfo.channel.Recieve();
		}
	}

public void ProcessNetworkMessages()
	{
		int num = middles.Count;
		for (int i = 0; i < num; i++)
		{
			SimpleThreadSafeQueue<RecievedMessage> concurrentQueue = messageBuffer[i];
			MiddleInfo middleInfo = middles[i];
			int count = concurrentQueue.Count;
			if (count > 0)
			{
				middleInfo.idleTime = 0f;
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
					BitStream stream = new BitStream(result.buffer);
					switch ((EClientPacket)stream.ReadBits(3)[0])
					{
					case EClientPacket.PING:
						SendPing(i);
						break;
					case EClientPacket.INPUT:
					{
						int ticket = stream.ReadInt(Settings.MAX_TICKET_BITS);
						InputSample inputSample = InputSample.ConstructFromStream(ref stream);
						int index;
						ClientInfo clientByTicket = middleInfo.GetClientByTicket(ticket, out index);
						if (index >= 0)
						{
							clientByTicket.unprocessedInputs.Add(inputSample);
							clientByTicket.proxy.transform.rotation = clientByTicket.proxy.frame * Quaternion.LookRotation(-inputSample.GetLookVector());
						}
						break;
					}
					}
				}
				else
				{
					if (result.type != Settings.EMessageType.RELIABLE_UNORDERED)
					{
						continue;
					}
					BitStream stream2 = new BitStream(result.buffer);
					switch ((EClientPacket)stream2.ReadBits(3)[0])
					{
					case EClientPacket.HELLO:
					{
						int ticket3 = stream2.ReadInt(Settings.MAX_TICKET_BITS);
						stream2.ReadInt(3);
						int index3;
						ClientInfo clientByTicket3 = middleInfo.GetClientByTicket(ticket3, out index3);
						if (index3 < 0)
						{
							clientByTicket3 = new ClientInfo();
							clientByTicket3.ticket = ticket3;
							objectRegistry.CreatePlayerServer(Vector3.up * 2f, Quaternion.identity);
							ClientInfo clientInfo = clientByTicket3;
							List<Entity> entities = entityManager.entities;
							clientInfo.proxy = entities[entities.Count - 1] as PlayerEntity;
							middleInfo.clients.Add(clientByTicket3);
							SendHello(i, ticket3, (int)clientByTicket3.proxy.id);
							SendReplicationDataInitial(i, clientByTicket3);
						}
						continue;
					}
					case EClientPacket.PREFERENCES:
					{
						int ticket2 = stream2.ReadInt(Settings.MAX_TICKET_BITS);
						int index2;
						ClientInfo clientByTicket2 = middleInfo.GetClientByTicket(ticket2, out index2);
						if (index2 >= 0)
						{
							clientByTicket2.preferences.ReadFromStream(ref stream2);
							clientByTicket2.proxy.cosmetics.branding = clientByTicket2.preferences.platformType;
							clientByTicket2.proxy.username = clientByTicket2.preferences.username;
						}
						continue;
					}
					case EClientPacket.DISCONNECT:
						break;
					default:
						continue;
					}
					int ticket4 = stream2.ReadInt(Settings.MAX_TICKET_BITS);
					HandleDisconnectAtClient(i, ticket4);
					i--;
					num--;
					break;
				}
			}
			if (concurrentQueue!=null){
				concurrentQueue.Clear();
			}
		}
	}


	public void StoreRecievedMessage(RecievedMessage message)
	{
		int count = messageBuffer.Count;
		for (int i = 0; i < count; i++)
		{
			if (middles[i].channel == message.owner)
			{
				SimpleThreadSafeQueue<RecievedMessage> concurrentQueue = messageBuffer[i];
				if (concurrentQueue != null && concurrentQueue.Count < 96)
				{
					concurrentQueue.Enqueue(message);
				}
				break;
			}
		}
	}
}
