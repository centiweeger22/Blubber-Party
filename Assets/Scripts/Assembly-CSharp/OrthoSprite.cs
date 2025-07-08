using UnityEngine;

public class OrthoSprite : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (!(Camera.main == null))
		{
			Vector3 forward = base.transform.position - Camera.main.transform.position;
			forward.Normalize();
			base.transform.rotation = Quaternion.LookRotation(forward, Camera.main.transform.up);
		}
	}
}
