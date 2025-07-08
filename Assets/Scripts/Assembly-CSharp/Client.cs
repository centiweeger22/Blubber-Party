using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Client : MonoBehaviour
{
	public Settings.EPlatformType platformType;

	public int proxyId = -1;

	public PlayerEntity proxy;

	public Quaternion proxyFrame = Quaternion.identity;

	public List<RecievedMessage> messageBuffer;

	public SimpleEntropyEncoder entropyEncoder;

	public float idleTime;

	public Channel channel;

	public ClientLinker linker;

	public int bindingPort = 5005;

	private int baseBindingPort;

	public string linkerAddress = "1.157.72.236";

	public int linkerPort = 7000;

	public Preferences preferences;

	public UpdateManager updateManager;

	public ObjectRegistry objectRegistry;

	public EntityManager entityManager;

	public InputManager inputManager;

	public ClientSidePrediction clientSidePrediction;

	public Statistics statistics;

	public GameClient gameClient;

	public List<PlayerEntity> players;

	public bool isConfirmed;

	private void Start()
	{
		Settings.Initialise(Settings.EBuildType.CLIENT, ref platformType);
		baseBindingPort = bindingPort;
		bindingPort = baseBindingPort - UnityEngine.Random.Range(0, 5000);
		IPAddress remoteIp = IPOperation.ResolveDomainName(linkerAddress);
		linker = new ClientLinker(LinkingCompleted, remoteIp, linkerPort, bindingPort);
		linker.Initialise();
		linker.SendConnectClientRequest();
		channel = new Channel();
		channel.dynamicPort = true;
		channel.sendMessageCallback = StoreRecievedMessage;
		channel.sentBytesCallback = statistics.OnBytesSent;
		channel.recievedBytesCallback = statistics.OnBytesRecieved;
		messageBuffer = new List<RecievedMessage>();
		entropyEncoder = new SimpleEntropyEncoder();
		preferences = new Preferences();
		preferences.platformType = platformType;
// Disable automatic physics simulation
		// Physics.autoSimulation = false;

		// Use reflection to get and call the hidden Simulate method
		// MethodInfo simulateMethod = typeof(Physics).GetMethod("Simulate", BindingFlags.Static | BindingFlags.NonPublic);
		// if (simulateMethod == null)
		// {
		// 	Debug.LogError("Physics.Simulate not found! This version of Unity might not support it.");
		// }
		// else
		// {
		// 	// Store simulateMethod somewhere and call like:
		// 	simulateMethod.Invoke(null, new object[] { Time.fixedDeltaTime });
		// }

		objectRegistry.Initialise();
		updateManager.Initialise();
		UpdateManager instance = UpdateManager.instance;
		instance.managerFunction = (UpdateManager.TickFunction)Delegate.Combine(instance.managerFunction, new UpdateManager.TickFunction(Tick));
		entityManager.Initialise();
		inputManager.Initialise();
		players = new List<PlayerEntity>();
		ObjectRegistry obj = objectRegistry;
		obj.spawnPlayerCallback = (ObjectRegistry.PlayerFunc)Delegate.Combine(obj.spawnPlayerCallback, new ObjectRegistry.PlayerFunc(OnPlayerSpawned));
		ObjectRegistry obj2 = objectRegistry;
		obj2.destroyPlayerCallback = (ObjectRegistry.PlayerFunc)Delegate.Combine(obj2.destroyPlayerCallback, new ObjectRegistry.PlayerFunc(OnPlayerDestroyed));
		ObjectRegistry obj3 = objectRegistry;
		obj3.spawnShipCallback = (ObjectRegistry.PlayerFunc)Delegate.Combine(obj3.spawnShipCallback, new ObjectRegistry.PlayerFunc(gameClient.OnShipSpawned));
		ObjectRegistry obj4 = objectRegistry;
		obj4.destroyShipCallback = (ObjectRegistry.PlayerFunc)Delegate.Combine(obj4.destroyShipCallback, new ObjectRegistry.PlayerFunc(gameClient.OnShipDestroyed));
		gameClient.Initialise();
	}

	public void LinkingCompleted(int bindingPort, IPAddress remoteAddress, int remotePort)
	{
		channel.Bind(new IPEndPoint(IPAddress.Any, bindingPort));
		channel.Connect(new IPEndPoint(remoteAddress, remotePort));
		idleTime = 0f;
		preferences.Reset();
		channel.Recieve();
	}

	public void UpdateTimeout(float deltaTime)
	{
		idleTime += deltaTime;
		if (idleTime >= 10f)
		{
			channel.Dump();
			channel = new Channel();
			channel.sendMessageCallback = StoreRecievedMessage;
			channel.sentBytesCallback = statistics.OnBytesSent;
			channel.recievedBytesCallback = statistics.OnBytesRecieved;
			bindingPort = baseBindingPort - UnityEngine.Random.Range(0, 5000);
			messageBuffer = new List<RecievedMessage>();
			idleTime = 0f;
			if (proxy != null)
			{
				objectRegistry.RemovePlayerAnimator(proxy);
			}
			entityManager.Dump();
			proxyId = -1;
			proxy = null;
			inputManager.cameraController.focus = null;
			clientSidePrediction.unacknowledgedInputs = new List<InputSample>();
			clientSidePrediction.proxy = null;
			isConfirmed = false;
			inputManager.Initialise();
			IPAddress remoteIp = IPOperation.ResolveDomainName(linkerAddress);
			linker.Reset(remoteIp, linkerPort, bindingPort);
			linker.Initialise();
			linker.SendConnectClientRequest();
		}
	}

	public void FindProxy()
	{
		if (proxy != null || proxyId < 0)
		{
			return;
		}
		Entity entityFromId = entityManager.GetEntityFromId((uint)proxyId);
		if (entityFromId != null)
		{
			proxy = entityFromId as PlayerEntity;
			proxy.isLocal = true;
			proxy.accept = false;
			proxy.body.isKinematic = false;
			objectRegistry.AddPlayerAnimator(proxy);
			entityManager.proxyId = proxyId;
			inputManager.cameraController.focus = proxy.animator.transform;
			if (inputManager.cameraController.isThirdPerson)
			{
				inputManager.cameraController.TogglePerspective();
			}
			clientSidePrediction.proxy = proxy;
			PlayerEntity playerEntity = proxy;
			playerEntity.cameraCorrectionCallback = (PlayerEntity.CameraCorrectionFunc)Delegate.Combine(playerEntity.cameraCorrectionCallback, new PlayerEntity.CameraCorrectionFunc(inputManager.cameraController.CameraCorrection));
			PlayerEntity playerEntity2 = proxy;
			playerEntity2.setCameraCallback = (PlayerEntity.SetCameraFunc)Delegate.Combine(playerEntity2.setCameraCallback, new PlayerEntity.SetCameraFunc(inputManager.cameraController.SetCamera));
		}
	}

	public void OnPlayerSpawned(Entity entity)
	{
		PlayerEntity item = entity as PlayerEntity;
		players.Add(item);
	}

	public void OnPlayerDestroyed(Entity entity)
	{
		PlayerEntity item = entity as PlayerEntity;
		players.Remove(item);
	}

	private void Update()
	{
		if (linker.state != ClientLinker.ELinkerState.READY)
		{
			linker.Update(Time.deltaTime);
			return;
		}
		UpdateTimeout(Time.deltaTime);
		if (proxy != null)
		{
			proxyFrame = Quaternion.identity;
			if (proxy.transform.parent != null)
			{
				proxyFrame = proxy.transform.parent.rotation;
			}
		}
		inputManager.cameraController.frame = proxyFrame;
		inputManager.PerFrameUpdate();
		if (proxy != null)
		{
			InputSample inputSample = inputManager.GetInputSample();
			proxy.animator.transform.rotation = proxyFrame * Quaternion.LookRotation(-inputSample.GetLookVector());
		}
	}

	private void LateUpdate()
	{
		if (!(proxy != null))
		{
			return;
		}
		foreach (PlayerEntity player in players)
		{
			Vector3 forward = inputManager.cameraController.transform.position - player.nameTag.transform.position;
			player.nameTag.transform.position = player.transform.position + player.nameTagLocalPosition;
			player.nameTag.transform.rotation = Quaternion.LookRotation(forward, inputManager.cameraController.transform.up);
		}
	}

	public void Tick()
	{
		if (linker.state != ClientLinker.ELinkerState.READY)
		{
			return;
		}
		FindProxy();
		ProcessNetworkMessages();
		if (isConfirmed)
		{
			SendInput();
			PreUpdateProxy();
			gameClient.Tick();
			if (proxy != null && !proxy.isRouted)
			{
				PhysicsStepper.instance.Step();
			}
			PostUpdateProxy();
			InputSample inputSample = inputManager.GetInputSample();
			clientSidePrediction.StoreInput(inputSample);
		}
		else
		{
			SendHello();
		}
		inputManager.Tick();
		foreach (Entity entity in entityManager.entities)
		{
			bool active = !entity.TickTimeout();
			entity.SetActive(active);
		}
		channel.ResendReliableMessages(Time.fixedDeltaTime);
	}

	public byte[] CompressData(ref byte[] buffer, int bytesWritten)
	{
		BitStream stream = new BitStream(entropyEncoder.MaxCompressionSize(bytesWritten));
		entropyEncoder.WriteCompressedBytes(ref stream, buffer);
		int num = (int)Math.Ceiling((float)stream.bitIndex / 8f);
		byte[] result = new byte[num];
		Buffer.BlockCopy(buffer, 0, stream.buffer, 0, num);
		return result;
	}

	public byte[] DecompressData(ref BitStream stream)
	{
		return entropyEncoder.ReadCompressedBytes(ref stream);
	}

	public void SendHello()
	{
		BitStream bitStream = new BitStream(1);
		bitStream.WriteInt(0, 3);
		bitStream.WriteInt((int)platformType, 3);
		channel.Send(bitStream.buffer, Settings.EMessageType.UNRELIABLE);
	}

	public void SendPing()
	{
		BitStream bitStream = new BitStream(1);
		bitStream.WriteInt(1, 3);
		channel.Send(bitStream.buffer, Settings.EMessageType.UNRELIABLE);
	}

	public void SendPreferences(Preferences preferences)
	{
		BitStream stream = new BitStream(8);
		stream.WriteInt(2, 3);
		preferences.WriteToStream(ref stream);
		channel.Send(stream.buffer, Settings.EMessageType.RELIABLE_UNORDERED);
	}

	public void SendInput()
	{
		BitStream stream = new BitStream(17);
		stream.WriteInt(3, 3);
		inputManager.GetInputSample().WriteToStream(ref stream);
		channel.Send(stream.buffer, Settings.EMessageType.UNRELIABLE);
	}

	public void SendDisconnect()
	{
		BitStream bitStream = new BitStream(1);
		bitStream.WriteInt(4, 3);
		channel.Send(bitStream.buffer, Settings.EMessageType.RELIABLE_UNORDERED);
	}

	public void PreUpdateProxy()
	{
		if (proxy != null)
		{
			InputSample inputSample = inputManager.GetInputSample();
			proxy.input = inputSample.Clone();
			proxy.ManualTick();
			proxy.animator.interpolationFilter.SetPreviousState(proxy.animator.transform.localPosition, proxy.animator.transform.localRotation);
		}
	}

	public void PostUpdateProxy()
	{
		if (proxy != null)
		{
			Vector3 position = proxy.body.position;
			if (proxy.parentId >= 0)
			{
				position = proxy.transform.parent.InverseTransformPoint(proxy.body.position);
			}
			Quaternion rotation = proxy.body.rotation;
			if (proxy.parentId >= 0)
			{
				rotation = Quaternion.Inverse(proxy.transform.parent.rotation) * proxy.body.rotation;
			}
			proxy.animator.interpolationFilter.SetCurrentState(position, rotation);
		}
	}

	public void Recieve()
	{
		if (!channel.isBusy)
		{
			channel.Recieve();
		}
	}

	public void ProcessNetworkMessages()
	{
		lock (messageBuffer)
		{
			if (messageBuffer.Count > 0)
			{
				idleTime = 0f;
			}
			foreach (RecievedMessage item in messageBuffer)
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
							isConfirmed = true;
							proxyId = stream3.ReadInt(Settings.MAX_ENTITY_BITS);
							FindProxy();
							SendPreferences(preferences);
							break;
						case EServerPacket.REPLICATION:
							if (stream3.ReadInt(2) == 2)
							{
								stream3.ReadInt(Settings.MAX_TICKET_BITS);
							}
							entityManager.ReadReplicationData(ref stream3);
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
							entityManager.ReadReplicationData(ref stream5);
							break;
						case EServerPacket.PROXY:
							{
								stream5.ReadInt(Settings.MAX_TICKET_BITS);
								int num = -1;
								bool flag = stream5.ReadBool();
								if (flag)
								{
									num = stream5.ReadInt(16);
									InputSample inputSample = inputManager.GetInputSample();
									float time = (float)MathExtension.DiffWrapped(num, inputSample.timestamp, 65536) * Time.fixedDeltaTime;
									statistics.OnPingRecieved(time);
								}
								FindProxy();
								if (proxy != null && flag)
								{
									proxy.ReadPlayerFromStream(ref stream5);
									clientSidePrediction.ReconcileWithServer(num);
								}
								break;
							}
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
			messageBuffer.Add(message);
			int count = messageBuffer.Count;
			if (count <= 96)
			{
				return;
			}
			for (int i = 0; i < count; i++)
			{
				if (messageBuffer[i].type == Settings.EMessageType.UNRELIABLE)
				{
					messageBuffer.RemoveAt(i);
					break;
				}
			}
		}
	}
}
