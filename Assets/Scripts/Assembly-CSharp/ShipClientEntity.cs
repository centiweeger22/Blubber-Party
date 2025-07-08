using UnityEngine;

public class ShipClientEntity : TransformEntity
{
	public float battery;

	public Transform[] batteryTransforms;

	public override void Initialise()
	{
		base.Initialise();
	}

	public override void WriteToStream(ref BitStream stream)
	{
		base.WriteToStream(ref stream);
		stream.WriteFloat(battery, 32);
	}

	public override void ReadFromStream(ref BitStream stream)
	{
		base.ReadFromStream(ref stream);
		battery = stream.ReadFloat(32);
	}

	public override int GetBitLength()
	{
		return base.GetBitLength() + 32;
	}

	public override void ReadFromStreamPartial(ref BitStream stream)
	{
		base.ReadFromStreamPartial(ref stream);
		battery = stream.ReadFloat(32);
	}

	public override void WriteToStreamPartial(ref BitStream stream)
	{
		base.WriteToStreamPartial(ref stream);
		stream.WriteFloat(battery, 32);
	}

	public override int GetBitLengthPartial()
	{
		return base.GetBitLengthPartial() + 32;
	}

	public override void Tick()
	{
		if (interpolationFilter == null)
		{
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

	public new void Update()
	{
		if (interpolationFilter != null)
		{
			interpolationFilter.Update(base.transform, Time.deltaTime);
		}
		Transform[] array = batteryTransforms;
		foreach (Transform transform in array)
		{
			transform.localScale = new Vector3(battery, transform.localScale.y, transform.localScale.z);
		}
	}

	public override void SetPriority(PlayerEntity player)
	{
		float num = Mathf.Max((player.transform.position - base.transform.position).sqrMagnitude, 1f);
		float num2 = 1f / num;
		priority = num2;
	}
}
