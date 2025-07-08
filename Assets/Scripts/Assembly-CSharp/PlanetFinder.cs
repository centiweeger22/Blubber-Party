using UnityEngine;

public class PlanetFinder : MonoBehaviour
{
	public static PlanetFinder instance;

	public Client client;

	public Transform[] planets;

	public Material dualSkybox;

	public Material[] skyboxes;

	public int currentIndex;

	public float radius = 10f;

	public float height = 5f;

	public float transition = 5f;

	public Transform[] gravitySources;

	public float[] strengths;

	public float fieldDistance = 20f;

	private void Start()
	{
		if (instance == null)
		{
			instance = this;
		}
	}

	private void Update()
	{
		if (client == null || client.proxy == null)
		{
			return;
		}
		Vector3 position = client.proxy.animator.transform.position;
		int num = planets.Length;
		for (int i = 0; i < num; i++)
		{
			Transform transform = planets[i];
			if (MathExtension.PointInsideCylinder(position, transform.position, radius, height))
			{
				SetSkybox(skyboxes[i], i);
				dualSkybox.SetFloat("_Lerp", 1f);
				break;
			}
			float magnitude = (MathExtension.ClosestPointOnCylinder(position, transform.position, radius, height) - position).magnitude;
			if (magnitude < transition)
			{
				float value = 1f - magnitude / transition;
				SetSkybox(skyboxes[i], i);
				dualSkybox.SetFloat("_Lerp", value);
				break;
			}
		}
	}

	public float GetGravity(PlayerEntity player)
	{
		int num = gravitySources.Length;
		for (int i = 0; i < num; i++)
		{
			Transform transform = gravitySources[i];
			if (MathExtension.PointInsideCylinder(player.transform.position, transform.position, radius, height))
			{
				return strengths[i];
			}
			if ((MathExtension.ClosestPointOnCylinder(player.transform.position, transform.position, radius, height) - player.transform.position).magnitude < fieldDistance)
			{
				return strengths[i];
			}
		}
		return 0f;
	}

	public void SetSkybox(Material skybox, int index)
	{
		if (index != currentIndex)
		{
			Color color = skybox.GetColor("_BottomColor");
			Color color2 = skybox.GetColor("_MiddleColor");
			Color color3 = skybox.GetColor("_TopColor");
			float value = skybox.GetFloat("_Split1");
			float value2 = skybox.GetFloat("_Split2");
			float value3 = skybox.GetFloat("_Split3");
			dualSkybox.SetColor("_BottomColor2", color);
			dualSkybox.SetColor("_MiddleColor2", color2);
			dualSkybox.SetColor("_TopColor2", color3);
			dualSkybox.SetFloat("_Split12", value);
			dualSkybox.SetFloat("_Split22", value2);
			dualSkybox.SetFloat("_Split32", value3);
			currentIndex = index;
		}
	}
}
