using System;
using UnityEngine;

public class Entity : MonoBehaviour, IComparable<Entity>
{
	public bool isNew = true;

	public int tick;

	public float priority;

	public bool accept = true;

	public uint id;

	public EObject type;

	public int dirtyFlag;

	public int dirtyFlagLength;

	public float idleTime;

	public virtual void Initialise()
	{
		UpdateManager instance = UpdateManager.instance;
		instance.entityFunction = (UpdateManager.TickFunction)Delegate.Combine(instance.entityFunction, new UpdateManager.TickFunction(Tick));
		dirtyFlagLength = 0;
	}

	public virtual void SetActive(bool state)
	{
		if (base.gameObject.activeSelf != state)
		{
			base.gameObject.SetActive(state);
		}
	}

	public virtual void WriteToStream(ref BitStream stream)
	{
		stream.WriteUint(id, Settings.MAX_ENTITY_BITS);
		stream.WriteInt((int)type, Settings.MAX_TYPE_BITS);
	}

	public virtual void ReadFromStream(ref BitStream stream)
	{
		id = stream.ReadUint(Settings.MAX_ENTITY_BITS);
		type = (EObject)stream.ReadInt(Settings.MAX_TYPE_BITS);
		idleTime = 0f;
	}

	public virtual int GetBitLength()
	{
		return Settings.MAX_ENTITY_BITS + Settings.MAX_TYPE_BITS + 1;
	}

	public virtual void WriteToStreamPartial(ref BitStream stream)
	{
		stream.WriteUint(id, Settings.MAX_ENTITY_BITS);
		stream.WriteInt((int)type, Settings.MAX_TYPE_BITS);
		if (dirtyFlagLength > 0)
		{
			stream.WriteInt(dirtyFlag, dirtyFlagLength);
		}
	}

	public virtual void ReadFromStreamPartial(ref BitStream stream)
	{
		id = stream.ReadUint(Settings.MAX_ENTITY_BITS);
		type = (EObject)stream.ReadInt(Settings.MAX_TYPE_BITS);
		if (dirtyFlagLength > 0)
		{
			dirtyFlag = stream.ReadInt(dirtyFlagLength);
		}
		idleTime = 0f;
	}

	public virtual int GetBitLengthPartial()
	{
		return Settings.MAX_ENTITY_BITS + Settings.MAX_TYPE_BITS + dirtyFlagLength;
	}

	public virtual void Tick()
	{
	}

	public bool TickTimeout()
	{
		idleTime += Time.fixedDeltaTime;
		return idleTime > 2f;
	}

	public virtual void SetPriority(PlayerEntity player)
	{
		priority = 0f;
	}

	public int CompareTo(Entity other)
	{
		if (other == null)
		{
			return 1;
		}
		return -priority.CompareTo(other.priority);
	}
}
