using System.Collections.Generic;

public class HuffmanTree
{
	public HuffmanNode root;

	public void InsertIntoTree(HuffmanNode node)
	{
		if (root == null)
		{
			root = node;
		}
		else
		{
			root.Insert(node);
		}
	}

	public void InsertIntoArray(ref List<HuffmanNode> list, HuffmanNode node)
	{
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (node.frequency < list[i].frequency)
			{
				list.Insert(0, node);
				return;
			}
		}
		list.Add(node);
	}

	public void GenerateTree(FrequencyTable table)
	{
		List<HuffmanNode> list = new List<HuffmanNode>();
		for (int i = 0; i < 256; i++)
		{
			HuffmanNode huffmanNode = new HuffmanNode();
			huffmanNode.value = table.values[i];
			huffmanNode.frequency = table.occurences[i];
			list.Add(huffmanNode);
		}
		list.Sort();
		while (list.Count >= 2)
		{
			HuffmanNode huffmanNode2 = new HuffmanNode();
			huffmanNode2.isIntermediate = true;
			huffmanNode2.leftChild = list[0];
			huffmanNode2.rightChild = list[1];
			huffmanNode2.frequency = huffmanNode2.leftChild.frequency + huffmanNode2.rightChild.frequency;
			list.RemoveRange(0, 2);
			InsertIntoArray(ref list, huffmanNode2);
		}
		root = list[0];
	}

	public void GenerateCodes()
	{
		BitStream stream = new BitStream(32);
		root.GenerateCodes(stream);
	}

	public List<HuffmanNode> GetLeafs()
	{
		List<HuffmanNode> list = new List<HuffmanNode>();
		root.GetLeafs(ref list);
		return list;
	}

	public HuffmanNode GetLeafFromCode(ref BitStream stream)
	{
		return root.GetLeafFromCode(ref stream);
	}
}
