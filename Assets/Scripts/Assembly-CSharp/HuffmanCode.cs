public class HuffmanCode
{
	public byte value;

	public BitStream codeStream;

	public HuffmanCode(byte _value, BitStream _codeStream)
	{
		value = _value;
		codeStream = _codeStream;
	}
}
