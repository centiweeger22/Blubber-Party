using UnityEngine;

public class Follow : MonoBehaviour
{
	public Transform target;

	private void Start()
	{
	}

	public void Tick()
	{
		base.transform.position = target.position;
		base.transform.rotation = target.rotation;
	}
}
