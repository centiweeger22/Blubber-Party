using UnityEngine;

public class DesktopInputManager : InputManager
{
	public DesktopInputSample sample;

	public bool hasFocus;

	public override void Initialise()
	{
		sample = new DesktopInputSample();
		sample.Initialise();
		cameraController.inputType = EInput.DESKTOP;
		cameraController.menuManager.inputType = EInput.DESKTOP;
	}

	public override InputSample GetInputSample()
	{
		return sample;
	}

	public override void PerFrameUpdate()
	{
		sample.left.Poll(menuOverride, fuzz);
		sample.right.Poll(menuOverride, fuzz);
		sample.forward.Poll(menuOverride, fuzz);
		sample.backward.Poll(menuOverride, fuzz);
		sample.jump.Poll(menuOverride, fuzz);
		cameraController.Poll();
		bool flag = Cursor.lockState == CursorLockMode.Locked;
		if (hasFocus != flag)
		{
			sample.fire.requireRelease = true;
		}
		hasFocus = flag;
		sample.fire.Poll(menuOverride, fuzz);
		sample.yaw = cameraController.yaw;
		sample.pitch = 0f;
		if (!cameraController.isThirdPerson || cameraController.isRouted)
		{
			sample.pitch = cameraController.pitch;
		}
	}

	public override void Tick()
	{
		sample.left.Reset();
		sample.right.Reset();
		sample.forward.Reset();
		sample.backward.Reset();
		sample.jump.Reset();
		sample.fire.Reset();
		sample.timestamp++;
		if (sample.timestamp >= 65536)
		{
			sample.timestamp -= 65536;
		}
	}
}
