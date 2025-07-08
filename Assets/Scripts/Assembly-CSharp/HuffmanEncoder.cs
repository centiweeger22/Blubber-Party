using System;
using System.Collections.Generic;
using UnityEngine;

public class HuffmanEncoder : MonoBehaviour
{
	public HuffmanCode[] hashTable;

	public HuffmanTree tree;

	public HuffmanEncoder()
	{
		hashTable = new HuffmanCode[256];
	}

	public void LoadFromFrequencyTable(string path)
	{
		FrequencyTable frequencyTable = new FrequencyTable();
		frequencyTable.ReadFromFile(path);
		tree = new HuffmanTree();
		tree.GenerateTree(frequencyTable);
		tree.GenerateCodes();
		List<HuffmanNode> leafs = tree.GetLeafs();
		int count = leafs.Count;
		for (int i = 0; i < count; i++)
		{
			HuffmanNode huffmanNode = leafs[i];
			hashTable[huffmanNode.value] = new HuffmanCode(huffmanNode.value, huffmanNode.codeStream);
		}
	}

	public void WriteToStream(ref BitStream stream, byte uncompressed)
	{
		HuffmanCode huffmanCode = hashTable[uncompressed];
		stream.WriteBytes(huffmanCode.codeStream.buffer, huffmanCode.codeStream.bitIndex);
	}

	public void WriteCompressedBytes(ref BitStream stream, byte[] buffer)
	{
		for (int i = 0; i < buffer.Length; i++)
		{
			WriteToStream(ref stream, buffer[i]);
		}
	}

	public byte[] ReadCompressedBytes(ref BitStream stream)
	{
		BitStream bitStream = new BitStream(MaxDecompressionSize(stream.buffer.Length));
		bool flag = true;
		HuffmanNode huffmanNode = null;
		while (huffmanNode != null || flag)
		{
			flag = false;
			huffmanNode = tree.GetLeafFromCode(ref stream);
			if (huffmanNode != null)
			{
				bitStream.WriteBits(huffmanNode.value, 8);
			}
		}
		int num = bitStream.bitIndex & 7;
		int num2 = bitStream.bitIndex >> 3;
		if (num > 0)
		{
			num2++;
		}
		byte[] array = new byte[num2];
		Buffer.BlockCopy(bitStream.buffer, 0, array, 0, num2);
		return array;
	}

	public int MaxCompressionSize(int byteLength)
	{
		return byteLength * 32;
	}

	public int MaxDecompressionSize(int byteLength)
	{
		return byteLength * 8;
	}
}
