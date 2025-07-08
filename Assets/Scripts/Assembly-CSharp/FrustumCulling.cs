using System;
using System.Collections.Generic;
using UnityEngine;

public class FrustumCulling : Culling
{
	public delegate LookupTable<TransformCache> GetTransformCacheFunc();

	public static float FIELD_OF_VIEW = 60f;

	public static float ASPECT_RATIO = 1.7777778f;

	public static float INFLATION = 10f;

	public float fovH;

	public GetTransformCacheFunc getTransformCacheCallback;

	public LookupTable<TransformCache> transformCacheLookup;

	public void CalculateHorizontalFieldOfView()
	{
		float num = FIELD_OF_VIEW * ((float)Math.PI / 180f);
		fovH = 2f * (float)Math.Atan((float)Math.Tan(num / 2f) * ASPECT_RATIO) * 57.29578f;
	}

	public override bool ApplyCulling(Entity entity, List<PlayerEntity> group)
	{
		transformCacheLookup = getTransformCacheCallback();
		TransformEntity transformEntity = entity as TransformEntity;
		if (transformEntity == null)
		{
			return true;
		}
		ColliderCache colliderCache = GetColliderCache(transformEntity);
		if (colliderCache.isEmpty)
		{
			return true;
		}
		int count = group.Count;
		if (count == 0)
		{
			return true;
		}
		TransformCache transformCache = transformCacheLookup.Grab((int)transformEntity.id);
		for (int i = 0; i < count; i++)
		{
			PlayerEntity player = group[i];
			Plane[] frustumPlanes = GetFrustumPlanes(player);
			int num = 0;
			for (int j = 0; j < 4; j++)
			{
				Plane plane = frustumPlanes[j];
				bool flag = false;
				Vector3 vector = transformCache.TransformPoint(Vector3.zero);
				float num2 = transformCache.scale.x * colliderCache.boundingRadius;
				Debug.DrawLine(vector, vector + Vector3.up * num2, Color.red);
				Debug.DrawLine(vector, vector + Vector3.down * num2, Color.red);
				Debug.DrawLine(vector, vector + Vector3.left * num2, Color.red);
				Debug.DrawLine(vector, vector + Vector3.right * num2, Color.red);
				Debug.DrawLine(vector, vector + Vector3.forward * num2, Color.red);
				Debug.DrawLine(vector, vector + Vector3.back * num2, Color.red);
				if (MathExtension.SphereInsidePlane(plane, vector, num2))
				{
					flag = true;
					num++;
				}
				else if (!flag)
				{
					break;
				}
			}
			if (num >= 4)
			{
				return true;
			}
		}
		return false;
	}

	public Plane[] GetFrustumPlanes(PlayerEntity player)
	{
		transformCacheLookup = getTransformCacheCallback();
		TransformCache transformCache = transformCacheLookup.Grab((int)player.id);
		Plane[] array = new Plane[4];
		Vector3 vector = transformCache.rotation * Vector3.forward;
		for (int i = 0; i < 2; i++)
		{
			int num = (i & 1) * 2 - 1;
			float num2 = -90f * (float)num;
			Vector3 normal = MathExtension.RotateWithYawPitch(vector, (fovH * 0.5f + INFLATION) * (float)num + num2, 0f);
			Plane plane = new Plane(transformCache.position, normal);
			array[i] = plane;
		}
		for (int j = 0; j < 2; j++)
		{
			int num3 = (j & 1) * 2 - 1;
			float num4 = -90f * (float)num3;
			Vector3 normal2 = MathExtension.RotateWithYawPitch(vector, 0f, (FIELD_OF_VIEW * 0.5f + INFLATION) * (float)num3 + num4);
			Plane plane2 = new Plane(transformCache.position, normal2);
			array[j + 2] = plane2;
		}
		return array;
	}

	public ColliderCache GetColliderCache(TransformEntity entity)
	{
		if (entity.colliderCache == null)
		{
			entity.colliderCache = new ColliderCache();
			entity.colliderCache.Generate(entity.gameObject);
		}
		return entity.colliderCache;
	}

	public override Culling Clone()
	{
		return new FrustumCulling
		{
			mode = mode,
			fovH = fovH,
			getTransformCacheCallback = getTransformCacheCallback
		};
	}
}
