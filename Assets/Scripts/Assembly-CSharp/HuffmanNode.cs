using System;
using System.Collections.Generic;

public class HuffmanNode : IComparable
{
	public byte value;

	public bool isIntermediate;

	public uint frequency;

	public HuffmanNode leftChild;

	public HuffmanNode rightChild;

	public BitStream codeStream;

	public HuffmanNode()
	{
		codeStream = new BitStream(32);
	}

public int CompareTo(object obj)
{
	if (obj == null)
	{
		return 1;
	}
	
	HuffmanNode huffmanNode = obj as HuffmanNode;
	if (huffmanNode != null)
	{
		return frequency.CompareTo(huffmanNode.frequency);
	}
	
	throw new ArgumentException("Object is not a HuffmanNode");
}

	public void Insert(HuffmanNode node)
	{
		if (node.frequency < frequency)
		{
			if (leftChild == null)
			{
				leftChild = node;
			}
			else
			{
				leftChild.Insert(node);
			}
		}
		else if (rightChild == null)
		{
			rightChild = node;
		}
		else
		{
			rightChild.Insert(node);
		}
	}

	public void GenerateCodes(BitStream _stream)
	{
		if (leftChild != null)
		{
			byte[] array = new byte[32];
			Buffer.BlockCopy(_stream.buffer, 0, array, 0, 32);
			BitStream bitStream = new BitStream(array);
			bitStream.bitIndex = _stream.bitIndex;
			bitStream.WriteBits(0, 1);
			leftChild.GenerateCodes(bitStream);
		}
		if (rightChild != null)
		{
			byte[] array2 = new byte[32];
			Buffer.BlockCopy(_stream.buffer, 0, array2, 0, 32);
			BitStream bitStream2 = new BitStream(array2);
			bitStream2.bitIndex = _stream.bitIndex;
			bitStream2.WriteBits(1, 1);
			rightChild.GenerateCodes(bitStream2);
		}
		if (leftChild == null && rightChild == null)
		{
			byte[] array3 = new byte[32];
			Buffer.BlockCopy(_stream.buffer, 0, array3, 0, 32);
			codeStream.buffer = array3;
			codeStream.bitIndex = _stream.bitIndex;
		}
	}

	public void GetLeafs(ref List<HuffmanNode> list)
	{
		if (leftChild != null)
		{
			leftChild.GetLeafs(ref list);
		}
		if (rightChild != null)
		{
			rightChild.GetLeafs(ref list);
		}
		if (leftChild == null && rightChild == null)
		{
			list.Add(this);
		}
	}

	public HuffmanNode GetLeafFromCode(ref BitStream stream)
	{
		if (leftChild == null && rightChild == null)
		{
			return this;
		}
		if (stream.bitIndex > stream.buffer.Length * 8)
		{
			return null;
		}
		if (stream.ReadBool())
		{
			return rightChild.GetLeafFromCode(ref stream);
		}
		return leftChild.GetLeafFromCode(ref stream);
	}
}
