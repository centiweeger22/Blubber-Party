using System.Collections.Generic;
using UnityEngine;

public class GameClient : MonoBehaviour
{
	public Client client;

	public PlanetFinder finder;

	public List<ShipEntity> activeShips;

	public void Initialise()
	{
		activeShips = new List<ShipEntity>();
	}

	public void Tick()
	{
		int count = client.players.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerEntity playerEntity = client.players[i];
			if (playerEntity.parentId < 0)
			{
				if (playerEntity.transform.parent != null)
				{
					Quaternion frame = playerEntity.frame;
					playerEntity.transform.SetParent(null);
					playerEntity.frame = Quaternion.identity;
					if (playerEntity.animator != null)
					{
						playerEntity.animator.interpolationFilter.previousPosition = playerEntity.animator.transform.parent.TransformPoint(playerEntity.animator.interpolationFilter.previousPosition);
						playerEntity.animator.interpolationFilter.previousRotation = playerEntity.animator.interpolationFilter.previousRotation * playerEntity.animator.transform.parent.rotation;
						playerEntity.animator.interpolationFilter.currentPosition = playerEntity.animator.transform.parent.TransformPoint(playerEntity.animator.interpolationFilter.currentPosition);
						playerEntity.animator.interpolationFilter.currentRotation = playerEntity.animator.interpolationFilter.previousRotation * playerEntity.animator.transform.parent.rotation;
						playerEntity.animator.interpolationFilter.Apply(playerEntity.transform);
						playerEntity.animator.transform.SetParent(null);
					}
					playerEntity.cameraCorrectionCallback(frame, playerEntity.frame);
				}
				continue;
			}
			Entity entityFromId = client.entityManager.GetEntityFromId((uint)playerEntity.parentId);
			if (entityFromId == null)
			{
				if (playerEntity.transform.parent != null)
				{
					Quaternion frame2 = playerEntity.frame;
					playerEntity.transform.SetParent(null);
					playerEntity.frame = Quaternion.identity;
					if (playerEntity.animator != null)
					{
						playerEntity.animator.interpolationFilter.previousPosition = playerEntity.animator.transform.parent.TransformPoint(playerEntity.animator.interpolationFilter.previousPosition);
						playerEntity.animator.interpolationFilter.previousRotation = playerEntity.animator.interpolationFilter.previousRotation * playerEntity.animator.transform.parent.rotation;
						playerEntity.animator.interpolationFilter.currentPosition = playerEntity.animator.transform.parent.TransformPoint(playerEntity.animator.interpolationFilter.currentPosition);
						playerEntity.animator.interpolationFilter.currentRotation = playerEntity.animator.interpolationFilter.previousRotation * playerEntity.animator.transform.parent.rotation;
						playerEntity.animator.interpolationFilter.Apply(playerEntity.transform);
						playerEntity.animator.transform.SetParent(null);
					}
					playerEntity.cameraCorrectionCallback(frame2, playerEntity.frame);
				}
			}
			else if (playerEntity.transform.parent == null)
			{
				Quaternion frame3 = playerEntity.frame;
				playerEntity.transform.SetParent(entityFromId.transform);
				playerEntity.frame = entityFromId.transform.rotation;
				if (playerEntity.animator != null)
				{
					playerEntity.animator.transform.SetParent(entityFromId.transform);
					playerEntity.animator.interpolationFilter.previousPosition = playerEntity.animator.transform.parent.InverseTransformPoint(playerEntity.animator.interpolationFilter.previousPosition);
					playerEntity.animator.interpolationFilter.previousRotation = Quaternion.Inverse(playerEntity.animator.transform.parent.rotation) * playerEntity.animator.interpolationFilter.previousRotation;
					playerEntity.animator.interpolationFilter.currentPosition = playerEntity.animator.transform.parent.InverseTransformPoint(playerEntity.animator.interpolationFilter.currentPosition);
					playerEntity.animator.interpolationFilter.currentRotation = Quaternion.Inverse(playerEntity.animator.transform.parent.rotation) * playerEntity.animator.interpolationFilter.previousRotation;
					playerEntity.animator.interpolationFilter.Apply(playerEntity.transform);
				}
				playerEntity.cameraCorrectionCallback(frame3, playerEntity.frame);
			}
			else
			{
				playerEntity.frame = entityFromId.transform.rotation;
			}
		}
		if (!(client.proxy == null))
		{
			client.inputManager.cameraController.isRouted = client.proxy.isRouted;
		}
	}

	public void OnShipSpawned(Entity entity)
	{
		activeShips.Add(entity as ShipEntity);
	}

	public void OnShipDestroyed(Entity entity)
	{
		activeShips.Remove(entity as ShipEntity);
	}
}
