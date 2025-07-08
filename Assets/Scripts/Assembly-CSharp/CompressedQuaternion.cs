using UnityEngine;

public class CompressedQuaternion
{
	public Quaternion quaternion;

	public FixedPoint x;

	public FixedPoint y;

	public FixedPoint z;

	public float precision;

	public bool calculatedBitLength;

	public int bitLength;

	public CompressedQuaternion()
	{
		precision = 3.0518044E-05f;
		x = new FixedPoint(-1f, 1f, precision);
		y = new FixedPoint(-1f, 1f, precision);
		z = new FixedPoint(-1f, 1f, precision);
		GetBitLength();
	}

	public int GetBitLength()
	{
		if (calculatedBitLength)
		{
			return bitLength;
		}
		bitLength += x.GetBitLength();
		bitLength += y.GetBitLength();
		bitLength += z.GetBitLength();
		bitLength++;
		calculatedBitLength = true;
		return bitLength;
	}

	public void WriteToStream(ref BitStream stream)
	{
		x.value = quaternion.x;
		y.value = quaternion.y;
		z.value = quaternion.z;
		x.WriteFixedPoint(ref stream);
		y.WriteFixedPoint(ref stream);
		z.WriteFixedPoint(ref stream);
		stream.WriteBool(quaternion.w > 0f);
	}

	public void ReadFromStream(ref BitStream stream)
	{
		x.ReadFixedPoint(ref stream);
		y.ReadFixedPoint(ref stream);
		z.ReadFixedPoint(ref stream);
		float num = (stream.ReadBool() ? 1f : (-1f));
		float num2 = 1f - x.value * x.value - y.value * y.value - z.value * z.value;
		float w = 0f;
		if (num2 > 0f)
		{
			w = Mathf.Sqrt(num2) * num;
		}
		quaternion = new Quaternion(x.value, y.value, z.value, w);
	}

	public void Quantize()
	{
		float num = Mathf.Sign(quaternion.w);
		x.Quantize();
		y.Quantize();
		z.Quantize();
		float num2 = 1f - x.value * x.value - y.value * y.value - z.value * z.value;
		float w = 0f;
		if (num2 > 0f)
		{
			w = Mathf.Sqrt(num2) * num;
		}
		quaternion = new Quaternion(x.value, y.value, z.value, w);
	}
}
