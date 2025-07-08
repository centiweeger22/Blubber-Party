using UnityEngine;

public class TransformEntity : Entity
{
	public InterpolationFilter interpolationFilter;

	public Vector3 previousPosition;

	public Quaternion previousRotation;

	public FixedPoint3 position;

	public CompressedQuaternion rotation;

	public ColliderCache colliderCache;

	public override void Initialise()
	{
		base.Initialise();
		dirtyFlagLength += 7;
		if (colliderCache == null)
		{
			colliderCache = new ColliderCache();
			colliderCache.Generate(base.gameObject);
		}
	}

	public override void SetActive(bool state)
	{
		bool activeSelf = base.gameObject.activeSelf;
		base.SetActive(state);
		if (!activeSelf && base.gameObject.activeSelf && interpolationFilter != null)
		{
			interpolationFilter.SetPreviousState(position.vector, rotation.quaternion);
			interpolationFilter.SetCurrentState(position.vector, rotation.quaternion);
			interpolationFilter.Update(base.transform, 0f);
		}
	}

	public override void WriteToStream(ref BitStream stream)
	{
		base.WriteToStream(ref stream);
		position.WriteFixedPoint(ref stream);
		rotation.WriteToStream(ref stream);
	}

	public override void ReadFromStream(ref BitStream stream)
	{
		if (interpolationFilter != null)
		{
			interpolationFilter.SetPreviousState(base.transform.localPosition, base.transform.localRotation);
		}
		base.ReadFromStream(ref stream);
		position.x.ReadFixedPoint(ref stream);
		position.y.ReadFixedPoint(ref stream);
		position.z.ReadFixedPoint(ref stream);
		float value = position.x.value;
		float value2 = position.y.value;
		float value3 = position.z.value;
		if (accept)
		{
			position.vector = new Vector3(value, value2, value3);
		}
		Quaternion quaternion = rotation.quaternion;
		rotation.ReadFromStream(ref stream);
		if (!accept)
		{
			rotation.quaternion = quaternion;
		}
		if (interpolationFilter != null)
		{
			interpolationFilter.SetCurrentState(position.vector, rotation.quaternion);
			return;
		}
		base.transform.localPosition = position.vector;
		base.transform.localRotation = rotation.quaternion;
	}

	public override int GetBitLength()
	{
		return base.GetBitLength() + position.GetBitLength() + rotation.GetBitLength();
	}

	public override void WriteToStreamPartial(ref BitStream stream)
	{
		base.WriteToStreamPartial(ref stream);
		if ((dirtyFlag & 1) > 0)
		{
			position.x.value = position.vector.x;
			position.x.WriteFixedPoint(ref stream);
		}
		if ((dirtyFlag & 2) > 0)
		{
			position.y.value = position.vector.y;
			position.y.WriteFixedPoint(ref stream);
		}
		if ((dirtyFlag & 4) > 0)
		{
			position.z.value = position.vector.z;
			position.z.WriteFixedPoint(ref stream);
		}
		if ((dirtyFlag & 8) > 0)
		{
			rotation.x.value = rotation.quaternion.x;
			rotation.x.WriteFixedPoint(ref stream);
		}
		if ((dirtyFlag & 0x10) > 0)
		{
			rotation.y.value = rotation.quaternion.y;
			rotation.y.WriteFixedPoint(ref stream);
		}
		if ((dirtyFlag & 0x20) > 0)
		{
			rotation.z.value = rotation.quaternion.z;
			rotation.z.WriteFixedPoint(ref stream);
		}
		if ((dirtyFlag & 0x40) > 0)
		{
			stream.WriteBool(rotation.quaternion.w > 0f);
		}
	}

	public override void ReadFromStreamPartial(ref BitStream stream)
	{
		if (interpolationFilter != null)
		{
			interpolationFilter.SetPreviousState(base.transform.localPosition, base.transform.localRotation);
		}
		base.ReadFromStreamPartial(ref stream);
		if ((dirtyFlag & 1) > 0)
		{
			position.x.ReadFixedPoint(ref stream);
			if (accept)
			{
				position.vector.x = position.x.value;
			}
		}
		if ((dirtyFlag & 2) > 0)
		{
			position.y.ReadFixedPoint(ref stream);
			if (accept)
			{
				position.vector.y = position.y.value;
			}
		}
		if ((dirtyFlag & 4) > 0)
		{
			position.z.ReadFixedPoint(ref stream);
			if (accept)
			{
				position.vector.z = position.z.value;
			}
		}
		if ((dirtyFlag & 8) > 0)
		{
			rotation.x.ReadFixedPoint(ref stream);
			if (accept)
			{
				rotation.quaternion.x = rotation.x.value;
			}
		}
		if ((dirtyFlag & 0x10) > 0)
		{
			rotation.y.ReadFixedPoint(ref stream);
			if (accept)
			{
				rotation.quaternion.y = rotation.y.value;
			}
		}
		if ((dirtyFlag & 0x20) > 0)
		{
			rotation.z.ReadFixedPoint(ref stream);
			if (accept)
			{
				rotation.quaternion.z = rotation.z.value;
			}
		}
		if ((dirtyFlag & 0x40) > 0)
		{
			float num = (stream.ReadBool() ? 1f : (-1f));
			if (accept)
			{
				float num2 = 1f - rotation.x.value * rotation.x.value - rotation.y.value * rotation.y.value - rotation.z.value * rotation.z.value;
				float w = 0f;
				if (num2 > 0f)
				{
					w = Mathf.Sqrt(num2) * num;
				}
				if (accept)
				{
					rotation.quaternion.w = w;
				}
			}
		}
		if (interpolationFilter != null)
		{
			interpolationFilter.SetCurrentState(position.vector, rotation.quaternion);
			return;
		}
		base.transform.localPosition = position.vector;
		base.transform.localRotation = rotation.quaternion;
	}

	public override int GetBitLengthPartial()
	{
		int num = base.GetBitLengthPartial();
		if ((dirtyFlag & 1) > 0)
		{
			num += position.x.GetBitLength();
		}
		if ((dirtyFlag & 2) > 0)
		{
			num += position.y.GetBitLength();
		}
		if ((dirtyFlag & 4) > 0)
		{
			num += position.z.GetBitLength();
		}
		if ((dirtyFlag & 8) > 0)
		{
			num += rotation.x.GetBitLength();
		}
		if ((dirtyFlag & 0x10) > 0)
		{
			num += rotation.y.GetBitLength();
		}
		if ((dirtyFlag & 0x20) > 0)
		{
			num += rotation.z.GetBitLength();
		}
		if ((dirtyFlag & 0x40) > 0)
		{
			num++;
		}
		return num;
	}

	public override void Tick()
	{
		if (interpolationFilter == null)
		{
			position.vector = base.transform.localPosition;
			rotation.quaternion = base.transform.localRotation;
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

	public void Update()
	{
		if (interpolationFilter != null)
		{
			interpolationFilter.Update(base.transform, Time.deltaTime);
		}
	}

	public override void SetPriority(PlayerEntity player)
	{
		float num = Mathf.Max((player.transform.position - base.transform.position).sqrMagnitude, 1f);
		float num2 = 1f / num;
		priority = num2;
	}
}
