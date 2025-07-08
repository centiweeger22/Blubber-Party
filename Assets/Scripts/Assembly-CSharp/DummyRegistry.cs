using System;
using System.Collections.Generic;
using UnityEngine;

public class DummyRegistry : MonoBehaviour
{
	public bool initialised;

	public ObjectRegistry objectRegistry;

	public Transform dummyFolder;

	public List<Entity> dummyPrefabs;

	public void Initialise()
	{
		BitStream stream = new BitStream(new byte[1300]);
		stream.WriteInt(0, Settings.MAX_ENTITY_BITS);
		EObject[] array = (EObject[])Enum.GetValues(typeof(EObject));
		foreach (EObject eObject in array)
		{
			if (eObject != EObject.MAX)
			{
				stream.bitIndex = Settings.MAX_ENTITY_BITS;
				stream.WriteInt((int)eObject, Settings.MAX_TYPE_BITS);
				stream.bitIndex = 0;
				Entity entity = objectRegistry.CreateObjectClient(ref stream);
				entity.Initialise();
				entity.transform.SetParent(dummyFolder);
				dummyPrefabs.Add(entity);
			}
		}
		initialised = true;
	}

	public void AdvanceStream(ref BitStream stream)
	{
		if (!initialised)
		{
			Initialise();
		}
		stream.bitIndex += Settings.MAX_ENTITY_BITS;
		EObject index = (EObject)stream.ReadInt(Settings.MAX_TYPE_BITS);
		stream.bitIndex -= Settings.MAX_ENTITY_BITS + Settings.MAX_TYPE_BITS;
		dummyPrefabs[(int)index].ReadFromStream(ref stream);
	}

	public void AdvanceStreamPartial(ref BitStream stream)
	{
		if (!initialised)
		{
			Initialise();
		}
		stream.bitIndex += Settings.MAX_ENTITY_BITS;
		EObject index = (EObject)stream.ReadInt(Settings.MAX_TYPE_BITS);
		stream.bitIndex -= Settings.MAX_ENTITY_BITS + Settings.MAX_TYPE_BITS;
		dummyPrefabs[(int)index].ReadFromStreamPartial(ref stream);
	}
}
