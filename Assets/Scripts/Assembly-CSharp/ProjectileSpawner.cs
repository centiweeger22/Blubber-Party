using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner
{
	public EntityManager entityManager;

	public ObjectRegistry objectRegistry;

	public PlayerEntity playerEntity;

	public float forwardOffset = 0.199999f;

	public void Tick()
	{
		if (playerEntity.isServer && playerEntity.input.fire.state == EButtonState.ON_PRESS)
		{
			Vector3 vector = -playerEntity.input.GetLookVector();
			objectRegistry.CreateProjectileServer(playerEntity.transform.position + vector * forwardOffset, playerEntity.transform.rotation);
			List<Entity> entities = entityManager.entities;
			ProjectileEntity obj = entities[entities.Count - 1] as ProjectileEntity;
			obj.ownerId = playerEntity.id;
			obj.velocity = obj.speed * vector;
		}
	}
}
