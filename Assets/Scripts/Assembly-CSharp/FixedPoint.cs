using System;

public class FixedPoint
{
	public float value;

	public float minValue;

	public float maxValue;

	public float precision;

	public bool calculatedBitLength;

	public int bitLength;

	public FixedPoint(float _minValue, float _maxValue, float _precision)
	{
		minValue = _minValue;
		maxValue = _maxValue;
		precision = _precision;
		GetBitLength();
	}

	public int TotalValues()
	{
		return (int)(float)Math.Floor((maxValue - minValue) / precision) + 1;
	}

	public int GetBitLength()
	{
		if (calculatedBitLength)
		{
			return bitLength;
		}
		int values = TotalValues();
		bitLength = MathExtension.RequiredBits(values);
		calculatedBitLength = true;
		return bitLength;
	}

	public void WriteFixedPoint(ref BitStream stream)
	{
		value = (float)Math.Min((float)Math.Max(value, minValue), maxValue);
		uint data = (uint)(int)(float)Math.Floor((value - minValue) / precision);
		stream.WriteUint(data, bitLength);
	}

	public void ReadFixedPoint(ref BitStream stream)
	{
		uint num = stream.ReadUint(bitLength);
		value = minValue + (float)num * precision;
	}

	public FixedPoint Clone()
	{
		return new FixedPoint(minValue, maxValue, precision)
		{
			value = value,
			calculatedBitLength = calculatedBitLength,
			bitLength = bitLength
		};
	}

	public void Quantize()
	{
		value = (float)Math.Min((float)Math.Max(value, minValue), maxValue);
		uint num = (uint)(int)(float)Math.Floor((value - minValue) / precision);
		value = minValue + (float)num * precision;
	}
}
