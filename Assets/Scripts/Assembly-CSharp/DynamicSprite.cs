using UnityEngine;

public class DynamicSprite : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (!(Camera.main == null))
		{
			Vector3 forward = base.transform.position - Camera.main.transform.position;
			forward.y = 0f;
			forward.Normalize();
			base.transform.rotation = Quaternion.LookRotation(forward);
		}
	}
}
