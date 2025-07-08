using System.Collections.Generic;
using UnityEngine;

public class ClientSidePrediction : MonoBehaviour
{
	public List<InputSample> unacknowledgedInputs;

	public PlayerEntity proxy;

	private void Start()
	{
		unacknowledgedInputs = new List<InputSample>();
	}

	public void StoreInput(InputSample input)
	{
		InputSample item = input.Clone();
		unacknowledgedInputs.Add(item);
		if (unacknowledgedInputs.Count > 40)
		{
			unacknowledgedInputs.RemoveAt(0);
		}
	}

	public void ReconcileWithServer(int timestamp)
	{
		int count = unacknowledgedInputs.Count;
		int num = -1;
		for (int i = 0; i < count; i++)
		{
			if (unacknowledgedInputs[i].timestamp == timestamp)
			{
				num = i;
				break;
			}
		}
		for (int j = 0; j <= num; j++)
		{
			unacknowledgedInputs.RemoveAt(0);
		}
		count = unacknowledgedInputs.Count;
		if (!proxy.isRouted)
		{
			for (int k = 0; k < count; k++)
			{
				proxy.input = unacknowledgedInputs[k];
				proxy.ManualTick();
				PhysicsStepper.instance.Step();
				// Physics.SyncTransforms();
			}
		}
	}
}
