using System.Collections.Generic;

public class DistanceCulling : Culling
{
	public delegate LookupTable<TransformCache> GetTransformCacheFunc();

	public float maxDistance = 10f;

	public float maxSqrDistance = 100f;

	public GetTransformCacheFunc getTransformCacheCallback;

	public LookupTable<TransformCache> transformCacheLookup;

	public void CalculateSquareDistance()
	{
		maxSqrDistance = maxDistance * maxDistance;
	}

	public override bool ApplyCulling(Entity entity, List<PlayerEntity> group)
	{
		transformCacheLookup = getTransformCacheCallback();
		TransformEntity transformEntity = entity as TransformEntity;
		if (transformEntity == null)
		{
			return true;
		}
		TransformCache transformCache = transformCacheLookup.Grab((int)transformEntity.id);
		int count = group.Count;
		if (count == 0)
		{
			return true;
		}
		for (int i = 0; i < count; i++)
		{
			PlayerEntity playerEntity = group[i];
			TransformCache transformCache2 = transformCacheLookup.Grab((int)playerEntity.id);
			if ((transformCache.position - transformCache2.position).sqrMagnitude < maxSqrDistance)
			{
				return true;
			}
		}
		return false;
	}

	public override Culling Clone()
	{
		return new DistanceCulling
		{
			mode = mode,
			maxDistance = maxDistance,
			maxSqrDistance = maxSqrDistance,
			getTransformCacheCallback = getTransformCacheCallback
		};
	}
}
