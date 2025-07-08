using UnityEngine;

public class Rotor : MonoBehaviour
{
	public Vector3 speed;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(speed * Time.deltaTime);
	}
}
