using System;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
	public delegate void TickFunction();

	public static UpdateManager instance;

	public TickFunction preFunction;

	public TickFunction entityFunction;

	public TickFunction managerFunction;

	public TickFunction postFunction;

	public float timer;

	public float maxTimer = 0.2f;

	public void Initialise()
	{
		instance = this;
		preFunction = (TickFunction)Delegate.Combine(preFunction, new TickFunction(DefaultFunction));
		entityFunction = (TickFunction)Delegate.Combine(entityFunction, new TickFunction(DefaultFunction));
		managerFunction = (TickFunction)Delegate.Combine(managerFunction, new TickFunction(DefaultFunction));
		postFunction = (TickFunction)Delegate.Combine(postFunction, new TickFunction(DefaultFunction));
	}

	private void Update()
	{
		timer += Mathf.Min(Time.deltaTime, Time.fixedDeltaTime);
		timer = Mathf.Min(timer, maxTimer);
		if (timer > Time.fixedDeltaTime)
		{
			preFunction();
			// Physics.SyncTransforms();
			entityFunction();
			managerFunction();
		}
	}

	private void LateUpdate()
	{
		if (timer > Time.fixedDeltaTime)
		{
			postFunction();
			timer -= Time.fixedDeltaTime;
		}
	}

	public void DefaultFunction()
	{
	}
}
