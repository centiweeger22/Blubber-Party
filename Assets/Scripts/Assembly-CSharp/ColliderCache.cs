using UnityEngine;

public class ColliderCache
{
	public bool isEmpty;

	public SphereCollider[] spheres;

	public BoxCollider[] boxes;

	public CapsuleCollider[] capsules;

	public Vector3[] sphereCentres;

	public float[] sphereRadiuses;

	public Vector3[] boxCentres;

	public Vector3[] boxSizes;

	public Vector3[] capsuleCentres;

	public float[] capsuleRadiuses;

	public float[] capsuleHeights;

	public float boundingRadius;

	public void Generate(GameObject gameObject)
	{
		spheres = gameObject.GetComponentsInChildren<SphereCollider>();
		boxes = gameObject.GetComponentsInChildren<BoxCollider>();
		capsules = gameObject.GetComponentsInChildren<CapsuleCollider>();
		isEmpty = spheres.Length == 0 && boxes.Length == 0 && capsules.Length == 0;
		float num = 0f;
		sphereCentres = new Vector3[spheres.Length];
		sphereRadiuses = new float[spheres.Length];
		for (int i = 0; i < spheres.Length; i++)
		{
			sphereCentres[i] = spheres[i].center;
			sphereRadiuses[i] = spheres[i].radius;
			float num2 = sphereCentres[i].magnitude + sphereRadiuses[i];
			if (num2 > num)
			{
				num = num2;
			}
		}
		boxCentres = new Vector3[boxes.Length];
		boxSizes = new Vector3[boxes.Length];
		for (int j = 0; j < boxes.Length; j++)
		{
			boxCentres[j] = boxes[j].center;
			boxSizes[j] = boxes[j].size;
			float num3 = boxCentres[j].magnitude + Mathf.Max(Mathf.Max(boxSizes[j].x, boxSizes[j].y), boxSizes[j].z);
			if (num3 > num)
			{
				num = num3;
			}
		}
		capsuleCentres = new Vector3[capsules.Length];
		capsuleRadiuses = new float[capsules.Length];
		capsuleHeights = new float[capsules.Length];
		for (int k = 0; k < capsules.Length; k++)
		{
			capsuleCentres[k] = capsules[k].center;
			capsuleRadiuses[k] = capsules[k].radius;
			capsuleHeights[k] = capsules[k].height;
			float num4 = capsuleCentres[k].magnitude + capsuleRadiuses[k] + capsuleHeights[k];
			if (num4 > num)
			{
				num = num4;
			}
		}
		boundingRadius = num;
	}
}
