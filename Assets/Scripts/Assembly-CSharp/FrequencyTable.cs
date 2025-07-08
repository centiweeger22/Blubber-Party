using System.IO;

public class FrequencyTable
{
	public byte[] values;

	public uint[] occurences;

	public float[] probabilities;

	public FrequencyTable()
	{
		values = new byte[256];
		occurences = new uint[256];
		probabilities = new float[256];
		for (int i = 0; i < 256; i++)
		{
			values[i] = (byte)i;
			occurences[i] = 0u;
			probabilities[i] = 1f;
		}
	}

	public void AddValue(byte value)
	{
		occurences[value]++;
	}

	public void GenerateProbabilities()
	{
		float num = 0f;
		for (int i = 0; i < 256; i++)
		{
			num += (float)occurences[i];
		}
		for (int j = 0; j < 256; j++)
		{
			float num2 = (float)occurences[j] / num;
			probabilities[j] = num2;
		}
	}

	public void WriteToFile(string path)
	{
		StreamWriter streamWriter = new StreamWriter(path);
		for (int i = 0; i < 256; i++)
		{
			streamWriter.Write(values[i].ToString());
			streamWriter.Write(',');
			streamWriter.Write(occurences[i].ToString());
			streamWriter.Write(',');
			streamWriter.Write(probabilities[i].ToString());
			streamWriter.Write('\n');
		}
		streamWriter.Close();
	}

	public void ReadFromFile(string path)
	{
		StreamReader streamReader = new StreamReader(path);
		for (int i = 0; i < 256; i++)
		{
			string[] array = streamReader.ReadLine().Split(',');
			values[i] = byte.Parse(array[0]);
			occurences[i] = uint.Parse(array[1]);
			probabilities[i] = float.Parse(array[2]);
		}
		streamReader.Close();
	}
}
