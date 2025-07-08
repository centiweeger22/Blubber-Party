public class ChangeColorParams : RPCParams
{
	public uint id;

	public float r;

	public float g;

	public float b;

	public ChangeColorParams()
	{
		type = EFunction.CHANGE_COLOR;
	}

	public override void WriteToStream(ref BitStream stream)
	{
		base.WriteToStream(ref stream);
		stream.WriteUint(id, Settings.MAX_ENTITY_BITS);
		stream.WriteFloat(r, 32);
		stream.WriteFloat(g, 32);
		stream.WriteFloat(b, 32);
	}

	public override void ReadFromStream(ref BitStream stream)
	{
		base.ReadFromStream(ref stream);
		id = stream.ReadUint(Settings.MAX_ENTITY_BITS);
		r = stream.ReadFloat(32);
		g = stream.ReadFloat(32);
		b = stream.ReadFloat(32);
	}

	public override int GetBitLength()
	{
		return base.GetBitLength() + Settings.MAX_ENTITY_BITS + 96;
	}
}
