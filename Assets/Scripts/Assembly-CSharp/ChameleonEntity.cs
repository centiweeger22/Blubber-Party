using UnityEngine;

public class ChameleonEntity : TransformEntity
{
	public MeshRenderer meshRenderer;

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

	public void ChangeColor(float r, float g, float b)
	{
		float a = meshRenderer.material.color.a;
		meshRenderer.material.color = new Color(r, g, b, a);
	}

	public override void Tick()
	{
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
