using UnityEngine;

public class InputSample
{
	public EInput type;

	public int timestamp;

	public float yaw;

	public float pitch;

	public Button jump;

	public Button fire;

	public virtual void Initialise()
	{
		jump = new Button(KeyCode.Space);
		fire = new Button(KeyCode.Mouse0);
	}

	public virtual Vector2 GetMovementVector()
	{
		return Vector2.zero;
	}

	public virtual Vector3 GetLookVector()
	{
		return MathExtension.DirectionFromYawPitch(yaw, pitch);
	}

	public virtual void WriteToStream(ref BitStream stream)
	{
		stream.WriteInt((int)type, 1);
		stream.WriteInt(timestamp, 16);
		stream.WriteFloat(yaw, 32);
		stream.WriteFloat(pitch, 32);
		stream.WriteInt((int)jump.state, 2);
		stream.WriteInt((int)fire.state, 2);
	}

	public virtual void ReadFromStream(ref BitStream stream)
	{
		timestamp = stream.ReadInt(16);
		yaw = stream.ReadFloat(32);
		pitch = stream.ReadFloat(32);
		byte state = stream.ReadBits(2)[0];
		jump.state = (EButtonState)state;
		byte state2 = stream.ReadBits(2)[0];
		fire.state = (EButtonState)state2;
	}

	public virtual InputSample Clone()
	{
		return new InputSample
		{
			yaw = yaw,
			pitch = pitch,
			timestamp = timestamp
		};
	}

	public virtual void Print()
	{
		Debug.Log("Yaw: " + yaw);
		Debug.Log("Pitch: " + pitch);
	}

	public static InputSample ConstructFromStream(ref BitStream stream)
	{
		switch ((EInput)stream.ReadInt(1))
		{
		case EInput.DESKTOP:
		{
			DesktopInputSample desktopInputSample = new DesktopInputSample();
			desktopInputSample.Initialise();
			desktopInputSample.ReadFromStream(ref stream);
			return desktopInputSample;
		}
		case EInput.MOBILE:
		{
			MobileInputSample mobileInputSample = new MobileInputSample();
			mobileInputSample.Initialise();
			mobileInputSample.ReadFromStream(ref stream);
			return mobileInputSample;
		}
		default:
			return null;
		}
	}
}
