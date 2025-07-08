using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
	public static EntityManager instance;

	public int proxyId = -1;

	public ObjectRegistry objectRegistry;

	public RPCManager rpcManager;

	public DummyRegistry dummyRegistry;

	public UpdateManager updateManager;

	public CullingStack referenceCullingStack;

	public Bucket networkIds;

	public List<int> ticks;

	public List<Entity> entities;

	public LookupTable<Entity> lookup;

	public LookupTable<TransformCache> transformCacheLookup;

	public LookupTable<BitCache> bitCacheLookup;

	public List<uint> toDestroy;

	public List<uint> unknownToDestroy;

	public void Initialise()
	{
		if (instance == null)
		{
			instance = this;
		}
		referenceCullingStack = new CullingStack();
		networkIds = new Bucket(65536);
		ticks = new List<int>();
		entities = new List<Entity>();
		lookup = new LookupTable<Entity>(65536);
		transformCacheLookup = new LookupTable<TransformCache>(65536);
		bitCacheLookup = new LookupTable<BitCache>(65536);
		toDestroy = new List<uint>();
		unknownToDestroy = new List<uint>();
		GenerateFullTicks();
		DistanceCulling distanceCulling = new DistanceCulling();
		distanceCulling.mode = ECullingMode.REQUIREMENT;
		distanceCulling.CalculateSquareDistance();
		distanceCulling.getTransformCacheCallback = GetTransformCacheData;
		referenceCullingStack.stack.Add(distanceCulling);
		FrustumCulling frustumCulling = new FrustumCulling();
		distanceCulling.mode = ECullingMode.OPTIONAL;
		frustumCulling.CalculateHorizontalFieldOfView();
		frustumCulling.getTransformCacheCallback = GetTransformCacheData;
		referenceCullingStack.stack.Add(frustumCulling);
	}

	public void Dump()
	{
		unknownToDestroy.Clear();
		int count = entities.Count;
		for (int i = 0; i < count; i++)
		{
			Entity entity = entities[i];
			lookup.Remove((int)entity.id);
			transformCacheLookup.Remove((int)entity.id);
			bitCacheLookup.Remove((int)entity.id);
			UpdateManager obj = updateManager;
			obj.entityFunction = (UpdateManager.TickFunction)Delegate.Remove(obj.entityFunction, new UpdateManager.TickFunction(entity.Tick));
			if (entity.type == EObject.PLAYER)
			{
				objectRegistry.destroyPlayerCallback(entity);
			}
			else if (entity.type == EObject.SHIP)
			{
				objectRegistry.destroyShipCallback(entity);
			}
			else if (entity.type == EObject.BIG_SHIP)
			{
				objectRegistry.destroyShipCallback(entity);
			}
			else if (entity.type == EObject.SMALL_SHIP)
			{
				objectRegistry.destroyShipCallback(entity);
			}
			UnityEngine.Object.Destroy(entity.gameObject);
		}
		entities.Clear();
		proxyId = -1;
	}

	public void DestroyClient(uint id)
	{
		int count = entities.Count;
		for (int i = 0; i < count; i++)
		{
			Entity entity = entities[i];
			if (entity.id == id)
			{
				lookup.Remove((int)entity.id);
				transformCacheLookup.Remove((int)entity.id);
				UpdateManager obj = updateManager;
				obj.entityFunction = (UpdateManager.TickFunction)Delegate.Remove(obj.entityFunction, new UpdateManager.TickFunction(entity.Tick));
				if (entity.type == EObject.PLAYER)
				{
					objectRegistry.destroyPlayerCallback(entity);
				}
				else if (entity.type == EObject.SHIP)
				{
					objectRegistry.destroyShipCallback(entity);
				}
				else if (entity.type == EObject.BIG_SHIP)
				{
					objectRegistry.destroyShipCallback(entity);
				}
				else if (entity.type == EObject.SMALL_SHIP)
				{
					objectRegistry.destroyShipCallback(entity);
				}
				UnityEngine.Object.Destroy(entity.gameObject);
				entities.RemoveAt(i);
				return;
			}
		}
		unknownToDestroy.Add(id);
	}

	public void DestroyServer(uint id)
	{
		int count = entities.Count;
		for (int i = 0; i < count; i++)
		{
			Entity entity = entities[i];
			if (entity.id == id)
			{
				lookup.Remove((int)entity.id);
				transformCacheLookup.Remove((int)entity.id);
				bitCacheLookup.Remove((int)entity.id);
				UpdateManager obj = updateManager;
				obj.entityFunction = (UpdateManager.TickFunction)Delegate.Remove(obj.entityFunction, new UpdateManager.TickFunction(entity.Tick));
				if (entity.type == EObject.PLAYER)
				{
					objectRegistry.destroyPlayerCallback(entity);
				}
				else if (entity.type == EObject.SHIP)
				{
					objectRegistry.destroyShipCallback(entity);
				}
				else if (entity.type == EObject.BIG_SHIP)
				{
					objectRegistry.destroyShipCallback(entity);
				}
				else if (entity.type == EObject.SMALL_SHIP)
				{
					objectRegistry.destroyShipCallback(entity);
				}
				UnityEngine.Object.Destroy(entity.gameObject);
				entities.RemoveAt(i);
				networkIds.ReturnIndex(entity.id);
				toDestroy.Add(entity.id);
				break;
			}
		}
	}

	public Entity GetEntityFromId(uint id)
	{
		return lookup.Grab((int)id);
	}

	public void CheckForTimeouts()
	{
		int count = entities.Count;
		for (int i = 0; i < count; i++)
		{
			Entity entity = entities[i];
			if (entity.accept && entity.TickTimeout())
			{
				UpdateManager obj = updateManager;
				obj.entityFunction = (UpdateManager.TickFunction)Delegate.Remove(obj.entityFunction, new UpdateManager.TickFunction(entity.Tick));
				if (entity.type == EObject.PLAYER)
				{
					objectRegistry.destroyPlayerCallback(entity);
				}
				UnityEngine.Object.Destroy(entity.gameObject);
				entities.RemoveAt(i);
				break;
			}
		}
	}

	public void GenerateFullTicks()
	{
		for (int i = 0; i < 20; i++)
		{
			ticks.Add(i);
		}
	}

	public void AssignFullTick(Entity entity)
	{
		int index = UnityEngine.Random.Range(0, ticks.Count);
		int tick = ticks[index];
		ticks.RemoveAt(index);
		entity.tick = tick;
		if (ticks.Count == 0)
		{
			GenerateFullTicks();
		}
	}

	public void SetPriorities(PlayerEntity player)
	{
		int count = entities.Count;
		for (int i = 0; i < count; i++)
		{
			entities[i].SetPriority(player);
		}
		entities.Sort();
	}

	public void CacheTransformData()
	{
		int count = entities.Count;
		for (int i = 0; i < count; i++)
		{
			Entity entity = entities[i];
			TransformEntity transformEntity = entity as TransformEntity;
			if (!(transformEntity == null))
			{
				TransformCache obj = new TransformCache(transformEntity.transform.position, transformEntity.transform.rotation, transformEntity.transform.localScale);
				transformCacheLookup.Place(obj, (int)entity.id);
			}
		}
	}

	public LookupTable<TransformCache> GetTransformCacheData()
	{
		return transformCacheLookup;
	}

	public void WriteReplicationDataNaive(ref BitStream stream)
	{
		uint count = (uint)toDestroy.Count;
		stream.WriteUint(count, Settings.MAX_ENTITY_BITS);
		for (int i = 0; i < count; i++)
		{
			stream.WriteBits(2, 3);
			stream.WriteUint(toDestroy[i], Settings.MAX_ENTITY_BITS);
		}
		int data = entities.Count + rpcManager.rpcsToSend.Count;
		int count2 = entities.Count;
		stream.WriteInt(data, 32);
		for (int j = 0; j < count2; j++)
		{
			Entity entity = entities[j];
			if (entity.isNew)
			{
				stream.WriteBits(0, 3);
				entity.WriteToStream(ref stream);
			}
			else
			{
				stream.WriteBits(1, 3);
				entity.WriteToStream(ref stream);
			}
		}
		rpcManager.WriteReplicationData(ref stream);
	}

	public WriteReplicationIndexer WriteReplicationDataPartialOnlyIndexed(ref BitStream stream, ref CullingStack cullingStack, WriteReplicationIndexer previousIndexer = null)
	{
		int num = 7200;
		WriteReplicationIndexer writeReplicationIndexer = previousIndexer;
		if (writeReplicationIndexer == null)
		{
			writeReplicationIndexer = new WriteReplicationIndexer();
		}
		stream.WriteInt(0, Settings.MAX_ENTITY_BITS);
		int count = entities.Count;
		int bitIndex = stream.bitIndex;
		stream.WriteInt(0, 32);
		uint writeCount = writeReplicationIndexer.writeCount;
		for (uint num2 = writeReplicationIndexer.totalIndex; num2 < count; num2++)
		{
			Entity entity = entities[(int)num2];
			if (cullingStack.ApplyCulling(entity))
			{
				int num3 = entity.GetBitLengthPartial() + 3;
				if (stream.bitIndex + num3 > num)
				{
					int bitIndex2 = stream.bitIndex;
					stream.bitIndex = bitIndex;
					stream.WriteUint(writeReplicationIndexer.writeCount - writeCount, 32);
					stream.bitIndex = bitIndex2;
					return writeReplicationIndexer;
				}
				if (!entity.isNew && entity.dirtyFlag > 0)
				{
					stream.WriteBits(3, 3);
					entity.WriteToStreamPartial(ref stream);
					writeReplicationIndexer.writeCount++;
				}
				writeReplicationIndexer.totalIndex++;
			}
		}
		uint data = writeReplicationIndexer.writeCount - writeCount;
		int bitIndex3 = stream.bitIndex;
		stream.bitIndex = bitIndex;
		stream.WriteUint(data, 32);
		stream.bitIndex = bitIndex3;
		writeReplicationIndexer.isDone = true;
		return writeReplicationIndexer;
	}

	public void FinishWritingReplicationData()
	{
		toDestroy.Clear();
		int count = entities.Count;
		for (int i = 0; i < count; i++)
		{
			Entity entity = entities[i];
			if (entity.isNew)
			{
				entity.isNew = false;
			}
		}
		rpcManager.FinishReplicationData();
	}

	public void ReadReplicationData(ref BitStream stream)
	{
		uint num = stream.ReadUint(Settings.MAX_ENTITY_BITS);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			stream.bitIndex += 3;
			uint id = stream.ReadUint(Settings.MAX_ENTITY_BITS);
			DestroyClient(id);
		}
		int num3 = stream.ReadInt(32);
		var gartrash5 = entities.Count;
		for (int i = 0; i < num3; i++)
		{
			switch ((EReplication)stream.ReadBits(3)[0])
			{
			case EReplication.CREATE:
			{
				uint num4 = stream.ReadUint(Settings.MAX_ENTITY_BITS);
				stream.bitIndex -= Settings.MAX_ENTITY_BITS;
				bool flag3 = true;
				bool flag4 = unknownToDestroy.Contains(num4);
				if (flag4)
				{
					unknownToDestroy.Remove(num4);
				}
				else if (lookup.Grab((int)num4) != null)
				{
					flag3 = false;
				}
				if (flag3 && !flag4)
				{
					objectRegistry.RegisterObjectClient(ref stream);
					List<Entity> list = entities;
					Entity entity3 = list[list.Count - 1];
					entity3.SetActive(state: false);
					entity3.idleTime = 2f;
				}
				else
				{
					dummyRegistry.AdvanceStream(ref stream);
				}
				break;
			}
			case EReplication.UPDATE:
			{
				int index2 = stream.ReadInt(Settings.MAX_ENTITY_BITS);
				stream.bitIndex -= Settings.MAX_ENTITY_BITS;
				bool flag2 = true;
				Entity entity2 = lookup.Grab(index2);
				if (entity2 != null)
				{
					entity2.ReadFromStream(ref stream);
					flag2 = false;
				}
				if (flag2)
				{
					dummyRegistry.AdvanceStream(ref stream);
				}
				break;
			}
			case EReplication.UPDATE_PARTIAL:
			{
				int index = stream.ReadInt(Settings.MAX_ENTITY_BITS);
				stream.bitIndex -= Settings.MAX_ENTITY_BITS;
				bool flag = true;
				Entity entity = lookup.Grab(index);
				if (entity != null)
				{
					entity.ReadFromStreamPartial(ref stream);
					flag = false;
				}
				if (flag)
				{
					dummyRegistry.AdvanceStreamPartial(ref stream);
				}
				break;
			}
			case EReplication.RPC:
				rpcManager.ExecuteFunction(ref stream);
				break;
			}
		}
	}

	public WriteReplicationIndexer WriteReplicationDataIndexed(ref BitStream stream, ref CullingStack cullingStack, WriteReplicationIndexer previousIndexer = null, int tick = 0, int limit = 0)
	{
		int num = 7200;
		WriteReplicationIndexer writeReplicationIndexer = previousIndexer;
		if (writeReplicationIndexer == null)
		{
			writeReplicationIndexer = new WriteReplicationIndexer();
		}
		stream.WriteInt(0, Settings.MAX_ENTITY_BITS);
		int count = entities.Count;
		int num2 = count + 1;
		if (limit > 0)
		{
			num2 = limit;
		}
		int bitIndex = stream.bitIndex;
		stream.WriteInt(0, 32);
		uint writeCount = writeReplicationIndexer.writeCount;
		for (uint num3 = writeReplicationIndexer.totalIndex; num3 < count; num3++)
		{
			Entity entity = entities[(int)num3];
			if (!cullingStack.ApplyCulling(entity))
			{
				continue;
			}
			int num4 = entity.GetBitLength() + 3;
			if (stream.bitIndex + num4 > num)
			{
				int bitIndex2 = stream.bitIndex;
				stream.bitIndex = bitIndex;
				stream.WriteUint(writeReplicationIndexer.writeCount - writeCount, 32);
				stream.bitIndex = bitIndex2;
				return writeReplicationIndexer;
			}
			if (entity.tick != tick)
			{
				if (!entity.isNew && entity.dirtyFlag > 0)
				{
					stream.WriteBits(3, 3);
					entity.WriteToStreamPartial(ref stream);
					writeReplicationIndexer.writeCount++;
				}
			}
			else if (!entity.isNew)
			{
				stream.WriteBits(1, 3);
				entity.WriteToStream(ref stream);
				writeReplicationIndexer.writeCount++;
			}
			if (writeReplicationIndexer.writeCount >= num2)
			{
				uint data = writeReplicationIndexer.writeCount - writeCount;
				int bitIndex3 = stream.bitIndex;
				stream.bitIndex = bitIndex;
				stream.WriteUint(data, 32);
				stream.bitIndex = bitIndex3;
				writeReplicationIndexer.isDone = true;
				return writeReplicationIndexer;
			}
			writeReplicationIndexer.totalIndex++;
		}
		uint data2 = writeReplicationIndexer.writeCount - writeCount;
		int bitIndex4 = stream.bitIndex;
		stream.bitIndex = bitIndex;
		stream.WriteUint(data2, 32);
		stream.bitIndex = bitIndex4;
		writeReplicationIndexer.isDone = true;
		return writeReplicationIndexer;
	}

	public void CacheBits(int tick = 0)
	{
		int count = entities.Count;
		for (uint num = 0u; num < count; num++)
		{
			Entity entity = entities[(int)num];
			if (entity.tick != tick)
			{
				if (!entity.isNew && entity.dirtyFlag > 0)
				{
					BitStream stream = new BitStream(entity.GetBitLengthPartial());
					entity.WriteToStreamPartial(ref stream);
					BitCache bitCache = new BitCache();
					bitCache.stream = stream;
					bitCacheLookup.Place(bitCache, (int)entity.id);
				}
			}
			else if (!entity.isNew)
			{
				BitStream stream2 = new BitStream(entity.GetBitLength());
				entity.WriteToStream(ref stream2);
				BitCache bitCache2 = new BitCache();
				bitCache2.stream = stream2;
				bitCacheLookup.Place(bitCache2, (int)entity.id);
			}
		}
	}

	public WriteReplicationIndexer WriteCachedReplicationDataIndexed(ref BitStream stream, ref CullingStack cullingStack, WriteReplicationIndexer previousIndexer = null, int tick = 0, int limit = 0)
	{
		int num = 7200;
		WriteReplicationIndexer writeReplicationIndexer = previousIndexer;
		if (writeReplicationIndexer == null)
		{
			writeReplicationIndexer = new WriteReplicationIndexer();
		}
		stream.WriteInt(0, Settings.MAX_ENTITY_BITS);
		int count = entities.Count;
		int num2 = count + 1;
		if (limit > 0)
		{
			num2 = limit;
		}
		int bitIndex = stream.bitIndex;
		stream.WriteInt(0, 32);
		uint writeCount = writeReplicationIndexer.writeCount;
		for (uint num3 = writeReplicationIndexer.totalIndex; num3 < count; num3++)
		{
			Entity entity = entities[(int)num3];
			if (!cullingStack.ApplyCulling(entity))
			{
				continue;
			}
			int num4 = entity.GetBitLength() + 3;
			if (stream.bitIndex + num4 > num)
			{
				int bitIndex2 = stream.bitIndex;
				stream.bitIndex = bitIndex;
				stream.WriteUint(writeReplicationIndexer.writeCount - writeCount, 32);
				stream.bitIndex = bitIndex2;
				return writeReplicationIndexer;
			}
			if (entity.tick != tick)
			{
				if (!entity.isNew && entity.dirtyFlag > 0)
				{
					stream.WriteBits(3, 3);
					BitCache bitCache = bitCacheLookup.Grab((int)entity.id);
					stream.WriteBytes(bitCache.stream.buffer, bitCache.stream.bitIndex);
					writeReplicationIndexer.writeCount++;
				}
			}
			else if (!entity.isNew)
			{
				stream.WriteBits(1, 3);
				BitCache bitCache2 = bitCacheLookup.Grab((int)entity.id);
				stream.WriteBytes(bitCache2.stream.buffer, bitCache2.stream.bitIndex);
				writeReplicationIndexer.writeCount++;
			}
			if (writeReplicationIndexer.writeCount >= num2)
			{
				uint data = writeReplicationIndexer.writeCount - writeCount;
				int bitIndex3 = stream.bitIndex;
				stream.bitIndex = bitIndex;
				stream.WriteUint(data, 32);
				stream.bitIndex = bitIndex3;
				writeReplicationIndexer.isDone = true;
				return writeReplicationIndexer;
			}
			writeReplicationIndexer.totalIndex++;
		}
		uint data2 = writeReplicationIndexer.writeCount - writeCount;
		int bitIndex4 = stream.bitIndex;
		stream.bitIndex = bitIndex;
		stream.WriteUint(data2, 32);
		stream.bitIndex = bitIndex4;
		writeReplicationIndexer.isDone = true;
		return writeReplicationIndexer;
	}

	public bool ReadyToWriteReliableData(bool forceCreate = false)
	{
		if (toDestroy.Count > 0)
		{
			return true;
		}
		int count = entities.Count;
		if (forceCreate && count > 0)
		{
			return true;
		}
		for (int i = 0; i < count; i++)
		{
			if (entities[i].isNew)
			{
				return true;
			}
		}
		return rpcManager.ReadyToWriteReliableData();
	}

	public WriteReplicationIndexer WriteReplicationDataReliableIndexedInitial(ref BitStream stream, WriteReplicationIndexer previousIndexer = null)
	{
		return WriteReplicationDataReliableIndexed(ref stream, previousIndexer, forceCreate: true);
	}

	public WriteReplicationIndexer WriteReplicationDataReliableIndexed(ref BitStream stream, WriteReplicationIndexer previousIndexer = null, bool forceCreate = false)
	{
		int num = 7200;
		WriteReplicationIndexer indexer = previousIndexer;
		if (indexer == null)
		{
			indexer = new WriteReplicationIndexer();
		}
		uint count = (uint)toDestroy.Count;
		int bitIndex = stream.bitIndex;
		stream.WriteUint(count - indexer.destroyIndex, Settings.MAX_ENTITY_BITS);
		uint totalIndex = indexer.totalIndex;
		for (uint num2 = indexer.destroyIndex; num2 < count; num2++)
		{
			int num3 = 35;
			if (stream.bitIndex + num3 > num)
			{
				int bitIndex2 = stream.bitIndex;
				stream.bitIndex = bitIndex;
				stream.WriteUint(num2 - totalIndex, Settings.MAX_ENTITY_BITS);
				stream.bitIndex = bitIndex2;
				return indexer;
			}
			stream.WriteBits(2, 3);
			stream.WriteUint(toDestroy[(int)num2], Settings.MAX_ENTITY_BITS);
			indexer.destroyIndex++;
		}
		uint count2 = (uint)entities.Count;
		int bitIndex3 = stream.bitIndex;
		stream.WriteInt(0, 32);
		uint writeCount = indexer.writeCount;
		for (uint num4 = indexer.totalIndex; num4 < count2; num4++)
		{
			Entity entity = entities[(int)num4];
			int num5 = entity.GetBitLength() + 3;
			if (stream.bitIndex + num5 > num)
			{
				int bitIndex4 = stream.bitIndex;
				stream.bitIndex = bitIndex3;
				stream.WriteUint(indexer.writeCount - writeCount, 32);
				stream.bitIndex = bitIndex4;
				return indexer;
			}
			if (entity.isNew || forceCreate)
			{
				bool isNew = entity.isNew;
				stream.WriteBits(0, 3);
				if (forceCreate)
				{
					entity.isNew = true;
				}
				entity.WriteToStream(ref stream);
				if (forceCreate)
				{
					entity.isNew = isNew;
				}
				indexer.writeCount++;
				indexer.createCount++;
			}
			indexer.totalIndex++;
		}
		uint written = indexer.writeCount - writeCount;
		rpcManager.WriteReplicationDataReliableIndexed(ref stream, ref indexer, bitIndex3, indexer.createCount, written);
		if (indexer.isDone)
		{
			int bitIndex5 = stream.bitIndex;
			stream.bitIndex = bitIndex3;
			stream.WriteUint(indexer.writeCount - writeCount, 32);
			stream.bitIndex = bitIndex5;
		}
		return indexer;
	}

	public void ClearDirtyFlags()
	{
		int count = entities.Count;
		for (int i = 0; i < count; i++)
		{
			entities[i].dirtyFlag = 0;
		}
	}
}
