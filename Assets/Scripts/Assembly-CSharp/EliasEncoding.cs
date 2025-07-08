public class EliasEncoding
{
	public static void WriteToStream(ref BitStream stream, uint value)
	{
		if (value == 1)
		{
			stream.WriteInt(1, 1);
			return;
		}
		int bitCount = MathExtension.RequiredBits(value);
		stream.WriteBits(0, bitCount);
		stream.WriteBits(1, 1);
		stream.WriteUint(value, bitCount);
	}

	public static uint ReadFromStream(ref BitStream stream)
	{
		int num = 0;
		bool flag = stream.ReadBool();
		while (flag)
		{
			num++;
			stream.ReadBool();
		}
		if (num == 0)
		{
			return 1u;
		}
		int num2 = 1 << num;
		uint num3 = stream.ReadUint(num);
		return (uint)num2 + num3;
	}

	public static int SmartWriteToStream(ref BitStream stream, int value, int maxLength)
	{
		return SmartWriteToStream(ref stream, (uint)value, maxLength);
	}

	public static int SmartWriteToStream(ref BitStream stream, uint value, int maxLength)
	{
		int num = MathExtension.RequiredBits(value) * 2 + 1;
		if (num > maxLength)
		{
			stream.WriteBits(1, 1);
			stream.WriteUint(value, maxLength);
			return maxLength;
		}
		stream.WriteBits(0, 1);
		WriteToStream(ref stream, value);
		return num;
	}

	public static uint SmartReadFromStream(ref BitStream stream, int maxLength)
	{
		if (stream.ReadBool())
		{
			return ReadFromStream(ref stream);
		}
		return stream.ReadUint(maxLength);
	}
}
