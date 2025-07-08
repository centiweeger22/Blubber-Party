using UnityEngine;

public class MobileInputSample : InputSample
{
	public FixedPoint joystickX;

	public FixedPoint joystickY;

	public override void Initialise()
	{
		type = EInput.MOBILE;
		base.Initialise();
		joystickX = new FixedPoint(-1f, 1f, 0.125f);
		joystickY = new FixedPoint(-1f, 1f, 0.125f);
	}

	public override Vector2 GetMovementVector()
	{
		Vector2 result = new Vector2(joystickX.value, joystickY.value);
		if (result.sqrMagnitude > 1f)
		{
			result.Normalize();
		}
		return result;
	}

	public override Vector3 GetLookVector()
	{
		return MathExtension.DirectionFromYawPitch(yaw, pitch);
	}

	public override void WriteToStream(ref BitStream stream)
	{
		base.WriteToStream(ref stream);
		joystickX.WriteFixedPoint(ref stream);
		joystickY.WriteFixedPoint(ref stream);
	}

	public override void ReadFromStream(ref BitStream stream)
	{
		base.ReadFromStream(ref stream);
		joystickX.ReadFixedPoint(ref stream);
		joystickY.ReadFixedPoint(ref stream);
	}

	public override InputSample Clone()
	{
		return new MobileInputSample
		{
			timestamp = timestamp,
			jump = jump.Clone(),
			fire = fire.Clone(),
			yaw = yaw,
			pitch = pitch,
			joystickX = joystickX.Clone(),
			joystickY = joystickY.Clone()
		};
	}

	public override void Print()
	{
		Debug.Log("Joystick X: " + joystickX.value);
		Debug.Log("Joystick Y: " + joystickY.value);
		Debug.Log("Jump: " + jump.state);
		Debug.Log("Yaw: " + yaw);
		Debug.Log("Pitch: " + pitch);
	}
}
