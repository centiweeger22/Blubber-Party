using System.Collections.Generic;

public class CullingStack
{
	public List<Culling> stack;

	public List<PlayerEntity> group;

	public CullingStack()
	{
		stack = new List<Culling>();
		group = new List<PlayerEntity>();
	}

	public bool ApplyCulling(Entity entity)
	{
		int count = stack.Count;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			Culling culling = stack[i];
			bool flag2 = culling.ApplyCulling(entity, group);
			if (culling.mode == ECullingMode.REQUIREMENT)
			{
				flag = true;
				if (!flag2)
				{
					return false;
				}
			}
			if (flag2 && culling.mode == ECullingMode.OPTIONAL)
			{
				return true;
			}
		}
		if (!flag)
		{
			return count == 0;
		}
		return true;
	}

	public CullingStack Clone()
	{
		CullingStack cullingStack = new CullingStack();
		int count = stack.Count;
		for (int i = 0; i < count; i++)
		{
			Culling item = stack[i].Clone();
			cullingStack.stack.Add(item);
		}
		return cullingStack;
	}
}
