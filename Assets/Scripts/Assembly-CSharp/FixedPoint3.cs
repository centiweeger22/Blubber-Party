using UnityEngine;

public class FixedPoint3
{
	public Vector3 vector;

	public FixedPoint x;

	public FixedPoint y;

	public FixedPoint z;

	public float precision;

	public bool calculatedBitLength;

	public int bitLength;

	public FixedPoint3(float minX, float minY, float minZ, float maxX, float maxY, float maxZ, float _precision)
	{
		x = new FixedPoint(minX, maxX, _precision);
		y = new FixedPoint(minY, maxY, _precision);
		z = new FixedPoint(minZ, maxZ, _precision);
		precision = _precision;
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
		calculatedBitLength = true;
		return bitLength;
	}

	public void WriteFixedPoint(ref BitStream stream)
	{
		x.value = vector.x;
		y.value = vector.y;
		z.value = vector.z;
		x.WriteFixedPoint(ref stream);
		y.WriteFixedPoint(ref stream);
		z.WriteFixedPoint(ref stream);
	}

	public void ReadFixedPoint(ref BitStream stream)
	{
		x.ReadFixedPoint(ref stream);
		y.ReadFixedPoint(ref stream);
		z.ReadFixedPoint(ref stream);
		vector = new Vector3(x.value, y.value, z.value);
	}

	public void Quantize()
	{
		x.Quantize();
		y.Quantize();
		z.Quantize();
	}
}
