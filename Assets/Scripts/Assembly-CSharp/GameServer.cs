using System.Collections.Generic;
using UnityEngine;

public class GameServer : MonoBehaviour
{
	public Server server;

	public AudioSource source;

	public PlanetFinder finder;

	public static float DEATH_Y = -80f;

	public Transform[] spawns;

	public List<PlayerEntity> activePlayers;

	public List<ShipEntity> activeShips;

	public List<ClientInfo> activeInfos;

	public Transform statue;

	public float STATUE_DISTANCE = 1f;

	public float musicTimer;

	public float downtime = 5f;

	public List<Vector3> playerSpeeds;

	public List<Vector3> playerAngulars;

	public List<Vector3> shipSpeeds;

	public List<Vector3> shipAngulars;

	public void Initialise()
	{
		activePlayers = new List<PlayerEntity>();
		activeShips = new List<ShipEntity>();
		playerSpeeds = new List<Vector3>();
		playerAngulars = new List<Vector3>();
		shipSpeeds = new List<Vector3>();
		shipAngulars = new List<Vector3>();
	}

	public void Tick()
	{
		musicTimer -= Time.fixedDeltaTime;
		if (musicTimer < 0f)
		{
			int num = Random.Range(2, 12);
			AudioClip audioClip = server.rpcManager.clips[num];
			musicTimer += audioClip.length + downtime;
			server.rpcManager.PlaySoundServer(num);
		}
		List<PlayerEntity> list = new List<PlayerEntity>();
		int count = activePlayers.Count;
		int count2 = activeShips.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerEntity playerEntity = activePlayers[i];
			if ((statue.position - playerEntity.transform.position).sqrMagnitude <= STATUE_DISTANCE * STATUE_DISTANCE)
			{
				list.Add(playerEntity);
			}
			if (playerEntity.transform.position.y < DEATH_Y || playerEntity.transform.position.y > 0f - DEATH_Y)
			{
				playerEntity.transform.position = spawns[playerEntity.cosmetics.race].position;
				playerEntity.body.velocity = Vector3.zero;
				if (playerEntity.parentId >= 0)
				{
					ShipEntity shipEntity = server.entityManager.GetEntityFromId((uint)playerEntity.parentId) as ShipEntity;
					if (shipEntity != null && shipEntity.controller == playerEntity)
					{
						shipEntity.controller = null;
					}
					playerEntity.parentId = -1;
					playerEntity.frame = Quaternion.identity;
					playerEntity.isRouted = false;
					playerEntity.transform.SetParent(null);
					continue;
				}
			}
			if (playerEntity.parentId < 0)
			{
				for (int j = 0; j < count2; j++)
				{
					ShipEntity shipEntity2 = activeShips[j];
					Vector3 centre = shipEntity2.transform.TransformPoint(shipEntity2.inside.center);
					if (MathExtension.PointInsideBox(size: new Vector3(shipEntity2.transform.localScale.x * shipEntity2.inside.size.x, shipEntity2.transform.localScale.y * shipEntity2.inside.size.y, shipEntity2.transform.localScale.z * shipEntity2.inside.size.z), point: playerEntity.transform.position, centre: centre, rotation: shipEntity2.transform.rotation) && (shipEntity2.type != EObject.SMALL_SHIP || (shipEntity2.type == EObject.SMALL_SHIP && playerEntity.cosmetics.isGold)))
					{
						playerEntity.parentId = (int)shipEntity2.id;
						playerEntity.transform.SetParent(shipEntity2.tail.transform);
						playerEntity.frame = shipEntity2.transform.rotation;
						playerEntity.BaseTick();
					}
				}
				continue;
			}
			ShipEntity shipEntity3 = server.entityManager.GetEntityFromId((uint)playerEntity.parentId) as ShipEntity;
			if (shipEntity3 == null)
			{
				playerEntity.parentId = -1;
				playerEntity.transform.SetParent(null);
				playerEntity.frame = Quaternion.identity;
				playerEntity.isRouted = false;
				playerEntity.BaseTick();
				continue;
			}
			Vector3 centre2 = shipEntity3.transform.TransformPoint(shipEntity3.inside.center);
			if (!MathExtension.PointInsideBox(size: new Vector3(shipEntity3.transform.localScale.x * shipEntity3.inside.size.x, shipEntity3.transform.localScale.y * shipEntity3.inside.size.y, shipEntity3.transform.localScale.z * shipEntity3.inside.size.z), point: playerEntity.transform.position, centre: centre2, rotation: shipEntity3.transform.rotation))
			{
				playerEntity.parentId = -1;
				playerEntity.transform.SetParent(null);
				playerEntity.frame = Quaternion.identity;
				playerEntity.isRouted = false;
				if (shipEntity3.controller == playerEntity)
				{
					shipEntity3.controller = null;
				}
				playerEntity.BaseTick();
				continue;
			}
			playerEntity.frame = shipEntity3.transform.rotation;
			Vector3 centre3 = shipEntity3.drive.transform.TransformPoint(shipEntity3.drive.center);
			if (MathExtension.PointInsideBox(size: new Vector3(shipEntity3.drive.transform.localScale.x * shipEntity3.drive.size.x, shipEntity3.drive.transform.localScale.y * shipEntity3.drive.size.y, shipEntity3.drive.transform.localScale.z * shipEntity3.drive.size.z), point: playerEntity.transform.position, centre: centre3, rotation: shipEntity3.transform.rotation) && playerEntity.airTimer < 0.2f)
			{
				if (shipEntity3.controller == null)
				{
					playerEntity.isRouted = true;
					shipEntity3.controller = playerEntity;
				}
			}
			else if (shipEntity3.controller == playerEntity)
			{
				playerEntity.isRouted = false;
				shipEntity3.controller = null;
			}
		}
		bool[] array = new bool[10];
		for (int k = 0; k < 10; k++)
		{
			array[k] = false;
		}
		for (int l = 0; l < list.Count; l++)
		{
			array[list[l].cosmetics.race] = true;
		}
		bool flag = true;
		for (int m = 0; m < 10; m++)
		{
			if (!array[m])
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			for (int n = 0; n < list.Count; n++)
			{
				list[n].cosmetics.isGold = true;
			}
		}
	}

	public void RememberPlayerState()
	{
		playerSpeeds.Clear();
		playerAngulars.Clear();
		int count = activePlayers.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerEntity playerEntity = activePlayers[i];
			playerSpeeds.Add(playerEntity.body.velocity);
			playerAngulars.Add(playerEntity.body.angularVelocity);
		}
	}

	public void RestorePlayerState()
	{
		int count = activePlayers.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerEntity playerEntity = activePlayers[i];
			playerEntity.body.velocity = playerSpeeds[i];
			playerEntity.body.angularVelocity = playerAngulars[i];
		}
		playerSpeeds.Clear();
		playerAngulars.Clear();
	}

	public void RememberShipState()
	{
		shipSpeeds.Clear();
		shipAngulars.Clear();
		int count = activeShips.Count;
		for (int i = 0; i < count; i++)
		{
			ShipEntity shipEntity = activeShips[i];
			shipSpeeds.Add(shipEntity.body.velocity);
			shipAngulars.Add(shipEntity.body.angularVelocity);
		}
	}

	public void RestoreShipState()
	{
		int count = activeShips.Count;
		for (int i = 0; i < count; i++)
		{
			ShipEntity shipEntity = activeShips[i];
			shipEntity.body.velocity = shipSpeeds[i];
			shipEntity.body.angularVelocity = shipAngulars[i];
		}
		shipSpeeds.Clear();
		shipAngulars.Clear();
	}

	public void SwitchPhysicsMode(bool enablePlayers, bool enableShips)
	{
		foreach (PlayerEntity activePlayer in activePlayers)
		{
			activePlayer.body.isKinematic = !enablePlayers;
			activePlayer.body.detectCollisions = enablePlayers;
		}
		foreach (ShipEntity activeShip in activeShips)
		{
			activeShip.body.isKinematic = !enableShips;
		}
	}

	public void UpdateTails()
	{
		foreach (ShipEntity activeShip in activeShips)
		{
			activeShip.tailScript.Tick();
		}
	}

	public void OnPlayerSpawned(Entity entity)
	{
		PlayerEntity playerEntity = entity as PlayerEntity;
		activePlayers.Add(playerEntity);
		foreach (MiddleInfo middle in server.middles)
		{
			foreach (ClientInfo client in middle.clients)
			{
				if (client.proxy.id == entity.id)
				{
					activeInfos.Add(client);
				}
			}
		}
		playerEntity.cosmetics.race = Random.Range(0, 10);
		playerEntity.transform.position = spawns[playerEntity.cosmetics.race].position;
	}

	public void OnPlayerDestroyed(Entity entity)
	{
		PlayerEntity playerEntity = entity as PlayerEntity;
		if (playerEntity.parentId >= 0)
		{
			ShipEntity shipEntity = server.entityManager.GetEntityFromId((uint)playerEntity.parentId) as ShipEntity;
			if (shipEntity != null && shipEntity.controller == playerEntity)
			{
				shipEntity.controller = null;
			}
			playerEntity.parentId = -1;
			playerEntity.frame = Quaternion.identity;
			playerEntity.isRouted = false;
			playerEntity.transform.SetParent(null);
		}
		activePlayers.Remove(playerEntity);
		foreach (MiddleInfo middle in server.middles)
		{
			foreach (ClientInfo client in middle.clients)
			{
				if (client == null)
				{
					activeInfos.Remove(client);
				}
			}
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
