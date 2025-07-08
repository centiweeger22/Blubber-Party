public class Preferences
{
	public Settings.EPlatformType platformType;

	public FixedPoint entityCount;

	public string username = "";

	public string previousUsername = "";

	public Preferences()
	{
		entityCount = new FixedPoint(0f, 1f, 0.05f);
		entityCount.value = 0.05f;
	}

	public void Reset()
	{
		previousUsername = "";
	}

	public void WriteToStream(ref BitStream stream)
	{
		stream.WriteInt((int)platformType, 3);
		entityCount.WriteFixedPoint(ref stream);
		stream.WriteBool(username != previousUsername);
		if (username != previousUsername)
		{
			byte data = MathExtension.ConvertSimplifiedAlphabetToByte(username[0]);
			byte data2 = MathExtension.ConvertSimplifiedAlphabetToByte(username[1]);
			byte data3 = MathExtension.ConvertSimplifiedAlphabetToByte(username[2]);
			stream.WriteBits(data, 5);
			stream.WriteBits(data2, 5);
			stream.WriteBits(data3, 5);
			previousUsername = username;
		}
	}

	public void ReadFromStream(ref BitStream stream)
	{
		platformType = (Settings.EPlatformType)stream.ReadInt(3);
		entityCount.ReadFixedPoint(ref stream);
		if (stream.ReadBool())
		{
			char c = MathExtension.ConvertByteToSimplifiedAlphabet(stream.ReadBits(5)[0]);
			char c2 = MathExtension.ConvertByteToSimplifiedAlphabet(stream.ReadBits(5)[0]);
			char c3 = MathExtension.ConvertByteToSimplifiedAlphabet(stream.ReadBits(5)[0]);
			char[] value = new char[3] { c, c2, c3 };
			username = new string(value);
		}
	}
}
