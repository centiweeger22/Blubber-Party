using System;

public class SimpleEntropyEncoder
{
	public void WriteToStream(ref BitStream stream, byte uncompressed)
	{
		if (uncompressed == 0)
		{
			stream.WriteBits(1, 1);
			return;
		}
		stream.WriteBits(0, 1);
		stream.WriteBits(uncompressed, 8);
	}

	public void WriteCompressedBytes(ref BitStream stream, byte[] buffer)
	{
		for (int i = 0; i < buffer.Length; i++)
		{
			WriteToStream(ref stream, buffer[i]);
		}
	}

	public byte[] ReadCompressedBytes(ref BitStream stream)
	{
		int num = stream.buffer.Length * 8;
		byte[] array = new byte[MaxDecompressionSize(stream.buffer.Length)];
		int num2 = 0;
		while (!stream.IsEnd())
		{
			if (stream.ReadBool())
			{
				array[num2] = 0;
			}
			else if (stream.bitIndex <= num - 8)
			{
				array[num2] = stream.ReadBits(8)[0];
			}
			num2++;
		}
		byte[] array2 = new byte[num2];
		Buffer.BlockCopy(array, 0, array2, 0, num2);
		return array2;
	}

	public int MaxCompressionSize(int byteLength)
	{
		return (int)Math.Ceiling((float)byteLength * 1.125f);
	}

	public int MaxDecompressionSize(int byteLength)
	{
		return byteLength * 8;
	}
}
