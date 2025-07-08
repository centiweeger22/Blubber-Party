using System;

public class EliasRunLengthEncoder
{
	public void EncodeLength(ref BitStream stream, int length)
	{
		if (length == 1)
		{
			stream.WriteInt(1, 1);
			return;
		}
		int bitCount = MathExtension.RequiredBits(length);
		stream.WriteBits(0, bitCount);
		stream.WriteBits(1, 1);
		stream.WriteInt(length, bitCount);
	}

	public int DecodeLength(ref BitStream stream)
	{
		int num = 0;
		if (stream.IsEnd())
		{
			return -1;
		}
		bool flag = stream.ReadBool();
		while (flag)
		{
			if (stream.IsEnd())
			{
				return -1;
			}
			num++;
			stream.ReadBool();
		}
		if (num == 0)
		{
			return 1;
		}
		int num2 = 1 << num;
		int num3 = stream.buffer.Length * 8;
		if (stream.bitIndex <= num3 - num)
		{
			return -1;
		}
		int num4 = stream.ReadInt(num);
		return num2 + num4;
	}

	public void WriteCompressedBytes(ref BitStream stream, byte[] buffer)
	{
		RunLengthData runLengthData = new RunLengthData();
		BitStream bitStream = new BitStream(buffer);
		while (!bitStream.IsEnd())
		{
			bool state = bitStream.ReadBool();
			runLengthData.Update(state);
		}
		int count = runLengthData.lengths.Count;
		for (int i = 0; i < count; i++)
		{
			bool data = runLengthData.states[i];
			int length = runLengthData.lengths[i];
			stream.WriteBool(data);
			EncodeLength(ref stream, length);
		}
	}

	public byte[] ReadCompressedBytes(ref BitStream stream)
	{
		RunLengthData runLengthData = new RunLengthData();
		int bufferSize = MaxDecompressionSize(stream.buffer.Length);
		while (!stream.IsEnd())
		{
			bool state = stream.ReadBool();
			int num = DecodeLength(ref stream);
			if (num < 0)
			{
				break;
			}
			runLengthData.AddState(state, num);
		}
		BitStream bitStream = new BitStream(bufferSize);
		int count = runLengthData.lengths.Count;
		for (int i = 0; i < count; i++)
		{
			bool flag = runLengthData.states[i];
			int bitCount = runLengthData.lengths[i];
			uint data = (flag ? 255u : 0u);
			bitStream.WriteUint(data, bitCount);
		}
		int num2 = stream.bitIndex & 7;
		int num3 = stream.bitIndex >> 3;
		if (num2 > 0)
		{
			num3++;
		}
		byte[] array = new byte[num2];
		Buffer.BlockCopy(bitStream.buffer, 0, array, 0, num3);
		return array;
	}

	public int MaxCompressionSize(int byteLength)
	{
		int num = new RunLengthData().bitLength + 1;
		return byteLength * num;
	}

	public int MaxDecompressionSize(int byteLength)
	{
		RunLengthData runLengthData = new RunLengthData();
		int num = (1 << runLengthData.bitLength - 1) - 1;
		int num2 = runLengthData.bitLength + 1;
		return byteLength * (num - num2);
	}
}
