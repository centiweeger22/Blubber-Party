using UnityEngine;

public class Plane
{
	public Vector3 normal;

	public float distance;

	public Plane(Vector3 point, Vector3 normal)
	{
		this.normal = normal.normalized;
		distance = Vector3.Dot(normal, point);
	}
}
