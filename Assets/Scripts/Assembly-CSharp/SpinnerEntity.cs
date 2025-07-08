using UnityEngine;

public class SpinnerEntity : TransformEntity
{
	public Vector3 angularVelocity;

	public override void Initialise()
	{
		base.Initialise();
	}

	public override void WriteToStream(ref BitStream stream)
	{
		base.WriteToStream(ref stream);
	}

	public override void ReadFromStream(ref BitStream stream)
	{
		base.ReadFromStream(ref stream);
	}

	public override int GetBitLength()
	{
		return base.GetBitLength();
	}

	public override void ReadFromStreamPartial(ref BitStream stream)
	{
		base.ReadFromStreamPartial(ref stream);
	}

	public override void WriteToStreamPartial(ref BitStream stream)
	{
		base.WriteToStreamPartial(ref stream);
	}

	public override int GetBitLengthPartial()
	{
		return base.GetBitLengthPartial();
	}

	public override void Tick()
	{
		base.transform.Rotate(angularVelocity * Time.fixedDeltaTime);
		position.vector = base.transform.position;
		rotation.quaternion = base.transform.rotation;
		if (position.vector.x != previousPosition.x)
		{
			dirtyFlag |= 1;
		}
		if (position.vector.y != previousPosition.y)
		{
			dirtyFlag |= 2;
		}
		if (position.vector.z != previousPosition.z)
		{
			dirtyFlag |= 4;
		}
		if (rotation.quaternion.x != previousRotation.x)
		{
			dirtyFlag |= 8;
			dirtyFlag |= 64;
		}
		if (rotation.quaternion.y != previousRotation.y)
		{
			dirtyFlag |= 16;
			dirtyFlag |= 64;
		}
		if (rotation.quaternion.z != previousRotation.z)
		{
			dirtyFlag |= 32;
			dirtyFlag |= 64;
		}
		if (rotation.quaternion.w != previousRotation.w)
		{
			dirtyFlag |= 64;
		}
		previousPosition = position.vector;
		previousRotation = rotation.quaternion;
	}
}
