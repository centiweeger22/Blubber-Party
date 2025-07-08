using UnityEngine;

public class DesktopInputSample : InputSample
{
	public Button left;

	public Button right;

	public Button forward;

	public Button backward;

	public override void Initialise()
	{
		type = EInput.DESKTOP;
		base.Initialise();
		left = new Button(KeyCode.A);
		right = new Button(KeyCode.D);
		forward = new Button(KeyCode.W);
		backward = new Button(KeyCode.S);
	}

	public override Vector2 GetMovementVector()
	{
		Vector2 zero = Vector2.zero;
		if (left.state == EButtonState.PRESSED || left.state == EButtonState.ON_PRESS)
		{
			zero.x += 1f;
		}
		if (right.state == EButtonState.PRESSED || right.state == EButtonState.ON_PRESS)
		{
			zero.x += -1f;
		}
		if (forward.state == EButtonState.PRESSED || forward.state == EButtonState.ON_PRESS)
		{
			zero.y += 1f;
		}
		if (backward.state == EButtonState.PRESSED || backward.state == EButtonState.ON_PRESS)
		{
			zero.y += -1f;
		}
		if (zero.x != 0f && zero.y != 0f)
		{
			zero.Normalize();
		}
		return zero;
	}

	public override Vector3 GetLookVector()
	{
		return MathExtension.DirectionFromYawPitch(yaw, pitch);
	}

	public override void WriteToStream(ref BitStream stream)
	{
		base.WriteToStream(ref stream);
		stream.WriteInt((int)left.state, 2);
		stream.WriteInt((int)right.state, 2);
		stream.WriteInt((int)forward.state, 2);
		stream.WriteInt((int)backward.state, 2);
	}

	public override void ReadFromStream(ref BitStream stream)
	{
		base.ReadFromStream(ref stream);
		byte state = stream.ReadBits(2)[0];
		byte state2 = stream.ReadBits(2)[0];
		byte state3 = stream.ReadBits(2)[0];
		byte state4 = stream.ReadBits(2)[0];
		left.state = (EButtonState)state;
		right.state = (EButtonState)state2;
		forward.state = (EButtonState)state3;
		backward.state = (EButtonState)state4;
	}

	public override InputSample Clone()
	{
		return new DesktopInputSample
		{
			timestamp = timestamp,
			jump = jump.Clone(),
			fire = fire.Clone(),
			yaw = yaw,
			pitch = pitch,
			left = left.Clone(),
			right = right.Clone(),
			forward = forward.Clone(),
			backward = backward.Clone()
		};
	}

	public override void Print()
	{
		Debug.Log("Left: " + left.state);
		Debug.Log("Right: " + right.state);
		Debug.Log("Forward: " + forward.state);
		Debug.Log("Backward: " + backward.state);
		Debug.Log("Jump: " + jump.state);
		Debug.Log("Yaw: " + yaw);
		Debug.Log("Pitch: " + pitch);
	}
}
