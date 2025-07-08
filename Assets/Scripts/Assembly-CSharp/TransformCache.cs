using UnityEngine;

public class TransformCache
{
	public Vector3 position;

	public Quaternion rotation;

	public Vector3 scale;

	public TransformCache(Vector3 _position, Quaternion _rotation, Vector3 _scale)
	{
		position = _position;
		rotation = _rotation;
		scale = _scale;
	}

	public Vector3 TransformPoint(Vector3 point)
	{
		point = new Vector3(point.x * scale.x, point.y * scale.y, point.z * scale.z);
		point = rotation * point;
		point += position;
		return point;
	}

	public Vector3 TransformDirection(Vector3 direction)
	{
		direction = rotation * direction;
		return direction;
	}
}
