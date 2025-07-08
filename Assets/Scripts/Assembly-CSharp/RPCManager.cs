using System.Collections.Generic;
using UnityEngine;

public class RPCManager : MonoBehaviour
{
	public EntityManager entityManager;

	public List<RPCParams> rpcsToSend;

	public AudioSource source;

	public AudioClip[] clips;

	private void Start()
	{
		rpcsToSend = new List<RPCParams>();
	}

	public void ExecuteFunction(ref BitStream stream)
	{
		EFunction eFunction = (EFunction)stream.ReadInt(Settings.MAX_FUNCTION_TYPE_BITS);
		stream.bitIndex -= Settings.MAX_FUNCTION_TYPE_BITS;
		switch (eFunction)
		{
		case EFunction.PLAY_SOUND:
		{
			PlaySoundParams playSoundParams = new PlaySoundParams();
			playSoundParams.ReadFromStream(ref stream);
			PlaySoundClient(playSoundParams.soundIndex);
			break;
		}
		case EFunction.CHANGE_COLOR:
		{
			ChangeColorParams changeColorParams = new ChangeColorParams();
			changeColorParams.ReadFromStream(ref stream);
			ChangeColorClient(changeColorParams.id, changeColorParams.r, changeColorParams.g, changeColorParams.b);
			break;
		}
		}
	}

	public void WriteReplicationData(ref BitStream stream)
	{
		int count = rpcsToSend.Count;
		for (int i = 0; i < count; i++)
		{
			stream.WriteBits(4, 3);
			rpcsToSend[i].WriteToStream(ref stream);
		}
	}

	public bool ReadyToWriteReliableData()
	{
		return rpcsToSend.Count > 0;
	}

	public void WriteReplicationDataReliableIndexed(ref BitStream stream, ref WriteReplicationIndexer indexer, int totalBitIndex, uint offset, uint written)
	{
		int num = 7200;
		int count = rpcsToSend.Count;
		uint writeCount = indexer.writeCount;
		for (uint num2 = indexer.writeCount - offset; num2 < count; num2++)
		{
			RPCParams rPCParams = rpcsToSend[(int)num2];
			int num3 = rPCParams.GetBitLength() + 3;
			if (stream.bitIndex + num3 > num)
			{
				int bitIndex = stream.bitIndex;
				stream.bitIndex = totalBitIndex;
				stream.WriteUint(indexer.writeCount - writeCount + written, 32);
				stream.bitIndex = bitIndex;
				return;
			}
			stream.WriteBits(4, 3);
			rPCParams.WriteToStream(ref stream);
			indexer.totalIndex++;
			indexer.writeCount++;
		}
		indexer.isDone = true;
	}

	public void FinishReplicationData()
	{
		rpcsToSend.Clear();
	}

	public void PlaySoundClient(int soundIndex)
	{
		source.clip = clips[soundIndex];
		source.Play();
	}

	public void PlaySoundServer(int soundIndex)
	{
		source.clip = clips[soundIndex];
		source.Play();
		PlaySoundParams playSoundParams = new PlaySoundParams();
		playSoundParams.soundIndex = soundIndex;
		rpcsToSend.Add(playSoundParams);
	}

	public void ChangeColorClient(uint id, float r, float g, float b)
	{
		Entity entityFromId = entityManager.GetEntityFromId(id);
		if (!(entityFromId == null))
		{
			(entityFromId as ChameleonEntity).ChangeColor(r, g, b);
		}
	}

	public void ChangeColorServer(uint id, float r, float g, float b)
	{
		Entity entityFromId = entityManager.GetEntityFromId(id);
		if (!(entityFromId == null))
		{
			(entityFromId as ChameleonEntity).ChangeColor(r, g, b);
			ChangeColorParams changeColorParams = new ChangeColorParams();
			changeColorParams.id = id;
			changeColorParams.r = r;
			changeColorParams.g = g;
			changeColorParams.b = b;
			rpcsToSend.Add(changeColorParams);
		}
	}
}
