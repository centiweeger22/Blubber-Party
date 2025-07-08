using UnityEngine;

public class ObjectRegistry : MonoBehaviour
{
	public delegate void PlayerFunc(Entity entity);

	public bool includeInterpolationFilter = true;

	public GameObject[] serverPrefabs;

	public GameObject[] clientPrefabs;

	public GameObject playerAnimator;

	public EntityManager entityManager;

	public PlayerFunc spawnPlayerCallback;

	public PlayerFunc destroyPlayerCallback;

	public PlayerFunc spawnShipCallback;

	public PlayerFunc destroyShipCallback;

	public void Initialise()
	{
		spawnPlayerCallback = DefaultPlayerFuncCallback;
		destroyPlayerCallback = DefaultPlayerFuncCallback;
		spawnShipCallback = DefaultPlayerFuncCallback;
		destroyShipCallback = DefaultPlayerFuncCallback;
	}

	public void DefaultPlayerFuncCallback(Entity entity)
	{
	}

	public void RegisterObjectClient(ref BitStream stream)
	{
		Entity entity = CreateObjectClient(ref stream);
		if (entity.type == EObject.PLAYER)
		{
			spawnPlayerCallback(entity);
		}
		else if (entity.type == EObject.SHIP)
		{
			spawnShipCallback(entity);
		}
		else if (entity.type == EObject.BIG_SHIP)
		{
			spawnShipCallback(entity);
		}
		else if (entity.type == EObject.SMALL_SHIP)
		{
			spawnShipCallback(entity);
		}
		entityManager.lookup.Place(entity, (int)entity.id);
		entity.Initialise();
		entityManager.entities.Add(entity);
	}

public Entity CreateObjectClient(ref BitStream stream)
{
	stream.bitIndex += Settings.MAX_ENTITY_BITS;
	EObject eObject = (EObject)stream.ReadInt(Settings.MAX_TYPE_BITS);
	stream.bitIndex -= Settings.MAX_ENTITY_BITS + Settings.MAX_TYPE_BITS;

	switch (eObject)
	{
		case EObject.CUBE:
			return CreateCubeClient(ref stream);
		case EObject.BALL:
			return CreateBallClient(ref stream);
		case EObject.SPINNER:
			return CreateSpinnerClient(ref stream);
		case EObject.CHAMELEON:
			return CreateChameleonClient(ref stream);
		case EObject.PLAYER:
			return CreatePlayerClient(ref stream);
		case EObject.PROJECTILE:
			return CreateProjectileClient(ref stream);
		case EObject.SHIP:
			return CreateShipClient(ref stream, EObject.SHIP);
		case EObject.BIG_SHIP:
			return CreateShipClient(ref stream, EObject.BIG_SHIP);
		case EObject.SMALL_SHIP:
			return CreateShipClient(ref stream, EObject.SMALL_SHIP);
		default:
			return null;
	}
}

	public void RegisterObjectServer(Entity entity)
	{
		if (entity.type == EObject.PLAYER)
		{
			spawnPlayerCallback(entity);
		}
		else if (entity.type == EObject.SHIP)
		{
			spawnShipCallback(entity);
		}
		else if (entity.type == EObject.BIG_SHIP)
		{
			spawnShipCallback(entity);
		}
		else if (entity.type == EObject.SMALL_SHIP)
		{
			spawnShipCallback(entity);
		}
		entity.id = entityManager.networkIds.GetFreeIndex();
		entityManager.AssignFullTick(entity);
		entityManager.lookup.Place(entity, (int)entity.id);
		entity.Initialise();
		entityManager.entities.Add(entity);
	}

	public Entity CreateCubeClient(ref BitStream stream)
	{
		GameObject obj = Object.Instantiate(clientPrefabs[0]);
		TransformEntity component = obj.GetComponent<TransformEntity>();
		if (includeInterpolationFilter)
		{
			component.interpolationFilter = new InterpolationFilter();
		}
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.ReadFromStream(ref stream);
		obj.transform.SetPositionAndRotation(component.position.vector, component.rotation.quaternion);
		if (includeInterpolationFilter)
		{
			component.interpolationFilter.timer = 0.07f;
		}
		return component;
	}

	public Entity CreateBallClient(ref BitStream stream)
	{
		GameObject obj = Object.Instantiate(clientPrefabs[1]);
		TransformEntity component = obj.GetComponent<TransformEntity>();
		if (includeInterpolationFilter)
		{
			component.interpolationFilter = new InterpolationFilter();
		}
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.ReadFromStream(ref stream);
		obj.transform.SetPositionAndRotation(component.position.vector, component.rotation.quaternion);
		if (includeInterpolationFilter)
		{
			component.interpolationFilter.timer = 0.07f;
		}
		return component;
	}

	public Entity CreateSpinnerClient(ref BitStream stream)
	{
		GameObject obj = Object.Instantiate(clientPrefabs[2]);
		TransformEntity component = obj.GetComponent<TransformEntity>();
		if (includeInterpolationFilter)
		{
			component.interpolationFilter = new InterpolationFilter();
		}
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.ReadFromStream(ref stream);
		obj.transform.SetPositionAndRotation(component.position.vector, component.rotation.quaternion);
		if (includeInterpolationFilter)
		{
			component.interpolationFilter.timer = 0.07f;
		}
		return component;
	}

	public Entity CreateChameleonClient(ref BitStream stream)
	{
		GameObject obj = Object.Instantiate(clientPrefabs[3]);
		ChameleonEntity component = obj.GetComponent<ChameleonEntity>();
		if (includeInterpolationFilter)
		{
			component.interpolationFilter = new InterpolationFilter();
		}
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.ReadFromStream(ref stream);
		obj.transform.SetPositionAndRotation(component.position.vector, component.rotation.quaternion);
		if (includeInterpolationFilter)
		{
			component.interpolationFilter.timer = 0.07f;
		}
		return component;
	}

	public Entity CreatePlayerClient(ref BitStream stream)
	{
		GameObject obj = Object.Instantiate(clientPrefabs[4]);
		PlayerEntity component = obj.GetComponent<PlayerEntity>();
		if (includeInterpolationFilter)
		{
			component.interpolationFilter = new InterpolationFilter();
		}
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.ReadFromStream(ref stream);
		obj.transform.SetPositionAndRotation(component.position.vector, component.rotation.quaternion);
		if (includeInterpolationFilter)
		{
			component.interpolationFilter.timer = 0.07f;
		}
		return component;
	}

	public Entity CreateProjectileClient(ref BitStream stream)
	{
		GameObject obj = Object.Instantiate(clientPrefabs[5]);
		TransformEntity component = obj.GetComponent<TransformEntity>();
		if (includeInterpolationFilter)
		{
			component.interpolationFilter = new InterpolationFilter();
		}
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.ReadFromStream(ref stream);
		obj.transform.SetPositionAndRotation(component.position.vector, component.rotation.quaternion);
		if (includeInterpolationFilter)
		{
			component.interpolationFilter.timer = 0.07f;
		}
		return component;
	}

	public Entity CreateShipClient(ref BitStream stream, EObject variant)
	{
		GameObject obj = Object.Instantiate(clientPrefabs[(int)variant]);
		ShipClientEntity component = obj.GetComponent<ShipClientEntity>();
		if (includeInterpolationFilter)
		{
			component.interpolationFilter = new InterpolationFilter();
		}
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.ReadFromStream(ref stream);
		obj.transform.SetPositionAndRotation(component.position.vector, component.rotation.quaternion);
		if (includeInterpolationFilter)
		{
			component.interpolationFilter.timer = 0.07f;
		}
		return component;
	}

	public void CreateCubeServer(Vector3 position, Quaternion rotation)
	{
		GameObject obj = Object.Instantiate(serverPrefabs[0]);
		obj.transform.SetPositionAndRotation(position, rotation);
		TransformEntity component = obj.GetComponent<TransformEntity>();
		component.type = EObject.CUBE;
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.position.vector = position;
		component.rotation.quaternion = rotation;
		RegisterObjectServer(component);
	}

	public void CreateBallServer(Vector3 position, Quaternion rotation)
	{
		GameObject obj = Object.Instantiate(serverPrefabs[1]);
		obj.transform.SetPositionAndRotation(position, rotation);
		TransformEntity component = obj.GetComponent<TransformEntity>();
		component.type = EObject.BALL;
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.position.vector = position;
		component.rotation.quaternion = rotation;
		RegisterObjectServer(component);
	}

	public void CreateSpinnerServer(Vector3 position, Quaternion rotation, Vector3 angularVelocity)
	{
		GameObject obj = Object.Instantiate(serverPrefabs[2]);
		obj.transform.SetPositionAndRotation(position, rotation);
		SpinnerEntity component = obj.GetComponent<SpinnerEntity>();
		component.type = EObject.SPINNER;
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.position.vector = position;
		component.rotation.quaternion = rotation;
		component.angularVelocity = angularVelocity;
		RegisterObjectServer(component);
	}

	public void CreateChameleonServer(Vector3 position, Quaternion rotation)
	{
		GameObject obj = Object.Instantiate(serverPrefabs[3]);
		obj.transform.SetPositionAndRotation(position, rotation);
		ChameleonEntity component = obj.GetComponent<ChameleonEntity>();
		component.type = EObject.CHAMELEON;
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.position.vector = position;
		component.rotation.quaternion = rotation;
		RegisterObjectServer(component);
	}

	public void CreatePlayerServer(Vector3 position, Quaternion rotation)
	{
		GameObject obj = Object.Instantiate(serverPrefabs[4]);
		obj.transform.SetPositionAndRotation(position, rotation);
		PlayerEntity component = obj.GetComponent<PlayerEntity>();
		component.type = EObject.PLAYER;
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.position.vector = position;
		component.rotation.quaternion = rotation;
		RegisterObjectServer(component);
	}

	public void AddPlayerAnimator(PlayerEntity entity)
	{
		MeshRenderer[] componentsInChildren = entity.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		entity.nameTag.SetActive(value: false);
		GameObject obj = Object.Instantiate(playerAnimator);
		obj.transform.SetPositionAndRotation(entity.position.vector, entity.rotation.quaternion);
		TransformEntity transformEntity = (entity.animator = obj.GetComponent<TransformEntity>());
		transformEntity.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		transformEntity.rotation = new CompressedQuaternion();
		transformEntity.position.vector = entity.position.vector;
		transformEntity.rotation = entity.rotation;
		transformEntity.interpolationFilter = new InterpolationFilter();
		transformEntity.interpolationFilter.enableRotation = false;
		PlayerCosmetics componentInChildren = obj.GetComponentInChildren<PlayerCosmetics>();
		componentInChildren.SetRace(entity.cosmetics.race);
		Object.Destroy(entity.cosmetics);
		entity.cosmetics = componentInChildren;
	}

	public void RemovePlayerAnimator(PlayerEntity entity)
	{
		Object.Destroy(entity.animator.gameObject);
		entity.animator = null;
	}

	public void CreateProjectileServer(Vector3 position, Quaternion rotation)
	{
		GameObject obj = Object.Instantiate(serverPrefabs[5]);
		obj.transform.SetPositionAndRotation(position, rotation);
		ProjectileEntity component = obj.GetComponent<ProjectileEntity>();
		component.type = EObject.PROJECTILE;
		component.entityManager = entityManager;
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.position.vector = position;
		component.rotation.quaternion = rotation;
		RegisterObjectServer(component);
	}

	public void CreateShipServer(Vector3 position, Quaternion rotation, Vector3 angularVelocity, EObject variant)
	{
		GameObject obj = Object.Instantiate(serverPrefabs[(int)variant]);
		obj.transform.SetPositionAndRotation(position, rotation);
		ShipEntity component = obj.GetComponent<ShipEntity>();
		component.type = variant;
		component.position = new FixedPoint3(-500f, -100f, -500f, 500f, 100f, 500f, 0.01f);
		component.rotation = new CompressedQuaternion();
		component.position.vector = position;
		component.rotation.quaternion = rotation;
		RegisterObjectServer(component);
	}
}
