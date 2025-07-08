public class PlaySoundParams : RPCParams
{
	public int soundIndex;

	public PlaySoundParams()
	{
		type = EFunction.PLAY_SOUND;
	}

	public override void WriteToStream(ref BitStream stream)
	{
		base.WriteToStream(ref stream);
		stream.WriteInt(soundIndex, 4);
	}

	public override void ReadFromStream(ref BitStream stream)
	{
		base.ReadFromStream(ref stream);
		soundIndex = stream.ReadInt(4);
	}

	public override int GetBitLength()
	{
		return base.GetBitLength() + 4;
	}
}
