using System.Collections.Generic;
using UnityEngine;

public class ProjectileEntity : TransformEntity
{
	public EntityManager entityManager;

	public uint ownerId;

	public float radius = 0.05f;

	public Vector3 velocity = Vector3.zero;

	public float speed = 5f;

	public float lifetime = 5f;

	public float explosionRadius = 3.25f;

	public float knockback;

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

	public void SimulateExplosion()
	{
		List<PlayerEntity> list = new List<PlayerEntity>();
		int count = entityManager.entities.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerEntity playerEntity = entityManager.entities[i] as PlayerEntity;
			if (!(playerEntity == null))
			{
				list.Add(playerEntity);
			}
		}
		foreach (PlayerEntity item in list)
		{
			Vector3 vector = item.transform.position - base.transform.position;
			float magnitude = vector.magnitude;
			if (!(magnitude > explosionRadius))
			{
				float num = magnitude / explosionRadius;
				num = 1f - num;
				float num2 = Mathf.Lerp(0.5f, 1f, num);
				item.noGroundTimer = 0.2f;
				item.airTimer = 0.3f;
				if (magnitude > 0f)
				{
					Vector3 vector2 = vector / magnitude;
					item.body.velocity += knockback * num2 * vector2;
				}
			}
		}
	}

	public override void Tick()
	{
		bool flag = false;
		Vector3 vector = velocity * Time.fixedDeltaTime;
		float magnitude = vector.magnitude;
		RaycastHit[] array = Physics.SphereCastAll(base.transform.position, radius, vector / magnitude, magnitude);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			Entity component = raycastHit.transform.GetComponent<Entity>();
			if (component == null)
			{
				if (!flag)
				{
					entityManager.DestroyServer(id);
					flag = true;
				}
				base.transform.position = raycastHit.point + raycastHit.normal * radius;
				SimulateExplosion();
				break;
			}
			if (component.id != ownerId)
			{
				if (!flag)
				{
					entityManager.DestroyServer(id);
					flag = true;
				}
				base.transform.position = raycastHit.point + raycastHit.normal * radius;
				SimulateExplosion();
				break;
			}
		}
		base.transform.position += velocity * Time.fixedDeltaTime;
		lifetime -= Time.fixedDeltaTime;
		if (lifetime <= 0f)
		{
			lifetime = 0f;
			if (!flag)
			{
				entityManager.DestroyServer(id);
				flag = true;
			}
		}
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
