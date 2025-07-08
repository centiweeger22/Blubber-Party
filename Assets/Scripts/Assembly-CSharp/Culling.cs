using System.Collections.Generic;

public class Culling
{
	public ECullingMode mode;

	public virtual bool ApplyCulling(Entity entity, List<PlayerEntity> group)
	{
		return true;
	}

	public virtual Culling Clone()
	{
		return new Culling();
	}
}
