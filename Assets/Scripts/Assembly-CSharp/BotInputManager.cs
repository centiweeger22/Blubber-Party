using UnityEngine;

public class BotInputManager : InputManager
{
	public DesktopInputSample sample;

	public bool hasFocus;

	public override void Initialise()
	{
		sample = new DesktopInputSample();
		sample.Initialise();
		cameraController.inputType = EInput.DESKTOP;
	}

	public override InputSample GetInputSample()
	{
		return sample;
	}

public override void PerFrameUpdate()
{
	sample.left.PollAutomatic(false, menuOverride, fuzz);
	sample.right.PollAutomatic(false, menuOverride, fuzz);
	sample.forward.PollAutomatic(false, menuOverride, fuzz);
	sample.backward.PollAutomatic(false, menuOverride, fuzz);
	sample.jump.PollAutomatic(false, menuOverride, fuzz);
	cameraController.Poll();
	bool flag = Cursor.lockState == CursorLockMode.Locked;
	if (hasFocus != flag)
	{
		sample.fire.requireRelease = true;
	}
	hasFocus = flag;
	sample.fire.PollAutomatic(false, menuOverride, fuzz);
	sample.yaw = cameraController.yaw;
	sample.pitch = 0f;
	if (!cameraController.isThirdPerson)
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
