using UnityEngine;

public class InterpolationFilter
{
	public Vector3 previousPosition;

	public Quaternion previousRotation;

	public Vector3 currentPosition;

	public Quaternion currentRotation;

	public float timer;

	public bool enableRotation = true;

	public void SetPreviousState(Vector3 position, Quaternion rotation)
	{
		previousPosition = position;
		if (enableRotation)
		{
			previousRotation = rotation;
		}
		timer = 0f;
	}

	public void SetCurrentState(Vector3 position, Quaternion rotation)
	{
		currentPosition = position;
		if (enableRotation)
		{
			currentRotation = rotation;
		}
		if ((currentPosition - previousPosition).sqrMagnitude > 36f)
		{
			previousPosition = position;
			if (enableRotation)
			{
				previousRotation = rotation;
			}
		}
	}

	public void Update(Transform transform, float deltaTime)
	{
		timer += deltaTime;
		if (timer > 0.07f)
		{
			timer = 0.07f;
		}
		float t = timer / 0.07f;
		transform.localPosition = Vector3.Lerp(previousPosition, currentPosition, t);
		if (enableRotation)
		{
			transform.localRotation = Quaternion.Lerp(previousRotation, currentRotation, t);
		}
	}

	public void Apply(Transform transform)
	{
		float t = timer / 0.07f;
		transform.localPosition = Vector3.Lerp(previousPosition, currentPosition, t);
		if (enableRotation)
		{
			transform.localRotation = Quaternion.Lerp(previousRotation, currentRotation, t);
		}
	}
}
