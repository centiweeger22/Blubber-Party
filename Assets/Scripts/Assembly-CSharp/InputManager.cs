using UnityEngine;

public class InputManager : MonoBehaviour
{
	public CameraController cameraController;

	public bool menuOverride;

	public bool fuzz;

	public virtual void Initialise()
	{
	}

	public virtual InputSample GetInputSample()
	{
		return null;
	}

	public virtual void PerFrameUpdate()
	{
	}

	public virtual void Tick()
	{
	}
}
