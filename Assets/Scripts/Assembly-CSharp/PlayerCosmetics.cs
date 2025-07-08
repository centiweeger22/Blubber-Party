using UnityEngine;

public class PlayerCosmetics : MonoBehaviour
{
	public MeshRenderer leftBranding;

	public MeshRenderer rightBranding;

	public Material[] brandings;

	public Settings.EPlatformType previousBranding;

	public Settings.EPlatformType branding;

	public MeshRenderer body;

	public Material[] bodyColours;

	public Material gold;

	public GameObject[] bodyCostumes;

	public int previousRace;

	public int race;

	public bool previousIsGold;

	public bool isGold;

	public void WriteToStream(ref BitStream stream)
	{
		stream.WriteInt((int)branding, 3);
		stream.WriteInt(race, 5);
		stream.WriteBool(isGold);
	}

	public void ReadFromStream(ref BitStream stream)
	{
		Settings.EPlatformType ePlatformType = (Settings.EPlatformType)stream.ReadInt(3);
		if (branding != ePlatformType)
		{
			branding = ePlatformType;
			SetBranding(branding);
		}
		int num = stream.ReadInt(5);
		if (race != num)
		{
			race = num;
			SetRace(race);
		}
		bool flag = stream.ReadBool();
		if (isGold != flag)
		{
			isGold = flag;
			SetGold(isGold);
		}
	}

	public int GetBitLength()
	{
		return 9;
	}

	public void WriteToStreamPartial(ref BitStream stream, int dirtyFlag)
	{
		if ((dirtyFlag & 0x100) > 0)
		{
			stream.WriteInt((int)branding, 3);
		}
		if ((dirtyFlag & 0x200) > 0)
		{
			stream.WriteInt(race, 5);
		}
		if ((dirtyFlag & 0x400) > 0)
		{
			stream.WriteBool(isGold);
		}
	}

	public void ReadFromStreamPartial(ref BitStream stream, int dirtyFlag)
	{
		if ((dirtyFlag & 0x100) > 0)
		{
			Settings.EPlatformType ePlatformType = (Settings.EPlatformType)stream.ReadInt(3);
			if (branding != ePlatformType)
			{
				branding = ePlatformType;
				SetBranding(branding);
			}
		}
		if ((dirtyFlag & 0x200) > 0)
		{
			int num = stream.ReadInt(5);
			if (race != num)
			{
				race = num;
				SetRace(race);
			}
		}
		if ((dirtyFlag & 0x400) > 0)
		{
			bool flag = stream.ReadBool();
			if (isGold != flag)
			{
				isGold = flag;
				SetGold(isGold);
			}
		}
	}

	public int GetBitLengthPartial(int dirtyFlag)
	{
		int num = 0;
		if ((dirtyFlag & 0x100) > 0)
		{
			num += 3;
		}
		if ((dirtyFlag & 0x200) > 0)
		{
			num += 5;
		}
		if ((dirtyFlag & 0x400) > 0)
		{
			num++;
		}
		return num;
	}

	public void SetBranding(Settings.EPlatformType branding)
	{
		if (branding != Settings.EPlatformType.UNDEFINED)
		{
			leftBranding.material = brandings[(int)(branding - 1)];
			rightBranding.material = brandings[(int)(branding - 1)];
		}
	}

	public void SetRace(int _race)
	{
		for (int i = 0; i < bodyCostumes.Length; i++)
		{
			bodyCostumes[i].SetActive(value: false);
		}
		if (!isGold)
		{
			body.material = bodyColours[_race];
		}
		bodyCostumes[_race].SetActive(value: true);
	}

	public void SetGold(bool _state)
	{
		if (isGold)
		{
			body.material = gold;
		}
	}
}
