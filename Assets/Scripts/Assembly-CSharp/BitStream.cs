using System;

public class BitStream
{
    public byte[] buffer;
    public int bitIndex;

    public BitStream()
    {
        buffer = new byte[1300];
    }

    public BitStream(int bufferSize)
    {
        buffer = new byte[bufferSize];
    }

    public BitStream(byte[] _buffer)
    {
        buffer = _buffer;
    }

    public bool IsEnd()
    {
        return bitIndex >= buffer.Length * 8;
    }

    public void WriteBits(byte data, int bitCount)
    {
        int num = bitIndex + bitCount;
        int num2 = bitIndex >> 3;
        int num3 = bitIndex & 7;
        byte b = (byte)(~(255 << num3));
        buffer[num2] = (byte)((buffer[num2] & b) | (data << num3));
        int num4 = 8 - num3;
        if (num4 < bitCount)
        {
            byte b2 = (byte)(~b);
            int num5 = num2 + 1;
            buffer[num5] = (byte)((buffer[num5] & b2) | (data >> num4));
        }
        bitIndex = num;
    }

    public void WriteBitsSafe(byte data, int bitCount)
    {
        int num = bitIndex + bitCount;
        int num2 = bitIndex >> 3;
        int num3 = bitIndex & 7;
        byte b = (byte)(~(255 << num3));
        byte b2 = (byte)(~(255 << bitCount));
        buffer[num2] = (byte)((buffer[num2] & b) | ((data & b2) << num3));
        int num4 = 8 - num3;
        if (num4 < bitCount)
        {
            byte b3 = (byte)(~b);
            int num5 = num2 + 1;
            buffer[num5] = (byte)((buffer[num5] & b3) | (data >> num4));
        }
        bitIndex = num;
    }

    public int Min(int a, int b)
    {
        return (a > b) ? b : a;
    }

    public unsafe void WriteBits(byte* data, int bitCount)
    {
        while (bitCount > 8)
        {
            WriteBits(*data, 8);
            data++;
            bitCount -= 8;
        }
        if (bitCount > 0)
        {
            WriteBits(*data, bitCount);
        }
    }

    public unsafe void WriteBitsSafe(byte* data, int bitCount)
    {
        while (bitCount > 8)
        {
            WriteBitsSafe(*data, 8);
            data++;
            bitCount -= 8;
        }
        if (bitCount > 0)
        {
            WriteBitsSafe(*data, bitCount);
        }
    }

    public void WriteBytes(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            WriteBits(data[i], 8);
        }
    }

    public void WriteBytes(byte[] data, int length)
    {
        for (int i = 0; i < data.Length; i++)
        {
            int bitCount = Min(length, 8);
            WriteBits(data[i], bitCount);
            length -= 8;
            if (length <= 0)
            {
                break;
            }
        }
    }

    public unsafe void WriteInt(int data, int bitCount)
    {
        if (Settings.PLATFORM_ENDIANNESS != Settings.Endian.LITTLE)
        {
            data = ReverseEndianness(data);
        }
        byte* data2 = (byte*)(&data);
        WriteBits(data2, bitCount);
    }

    public unsafe void WriteUint(uint data, int bitCount)
    {
        if (Settings.PLATFORM_ENDIANNESS != Settings.Endian.LITTLE)
        {
            data = ReverseEndianness(data);
        }
        byte* data2 = (byte*)(&data);
        WriteBits(data2, bitCount);
    }

    public unsafe void WriteFloat(float data, int bitCount)
    {
        if (Settings.PLATFORM_ENDIANNESS != Settings.Endian.LITTLE)
        {
            int intData = ReverseEndianness(*(int*)(&data));
            byte* data2 = (byte*)(&intData);
            WriteBits(data2, bitCount);
        }
        else
        {
            byte* data2 = (byte*)(&data);
            WriteBits(data2, bitCount);
        }
    }

    public void WriteBool(bool data)
    {
        byte data2 = (byte)(data ? 1 : 0);
        WriteBits(data2, 1);
    }

    public byte[] ReadBits(int bitCount)
    {
        int num = bitCount >> 3;
        int num2 = bitCount & 7;
        int num3 = num;
        if (num2 > 0)
        {
            num3++;
        }
        BitStream bitStream = new BitStream(num3);
        while (bitCount > 0)
        {
            int num4 = bitIndex >> 3;
            int num5 = bitIndex & 7;
            int num6 = Min(8 - num5, bitCount);
            byte b = buffer[num4];
            b = (byte)(b >> num5);
            bitStream.WriteBitsSafe(b, num6);
            bitCount -= num6;
            bitIndex += num6;
        }
        return bitStream.buffer;
    }

    public unsafe void ReadBits(byte* data, int bitCount)
    {
        byte[] array = ReadBits(bitCount);
        int num = 0;
        while (bitCount > 8)
        {
            *data = array[num];
            data++;
            num++;
            bitCount -= 8;
        }
        if (bitCount > 0)
        {
            byte b = (byte)(~(255 << bitCount));
            *data = (byte)(array[num] & b);
        }
    }

    public unsafe int ReadInt(int bitCount)
    {
        int num = 0;
        byte* data = (byte*)(&num);
        ReadBits(data, bitCount);
        if (Settings.PLATFORM_ENDIANNESS != Settings.Endian.LITTLE)
        {
            num = ReverseEndianness(num);
        }
        return num;
    }

    public unsafe uint ReadUint(int bitCount)
    {
        uint num = 0u;
        byte* data = (byte*)(&num);
        ReadBits(data, bitCount);
        if (Settings.PLATFORM_ENDIANNESS != Settings.Endian.LITTLE)
        {
            num = ReverseEndianness(num);
        }
        return num;
    }

    public unsafe float ReadFloat(int bitCount)
    {
        float num = 0f;
        byte* data = (byte*)(&num);
        ReadBits(data, bitCount);
        if (Settings.PLATFORM_ENDIANNESS != Settings.Endian.LITTLE)
        {
            int intNum = ReverseEndianness(*(int*)(&num));
            num = *(float*)(&intNum);
        }
        return num;
    }

    public bool ReadBool()
    {
        return ReadBits(1)[0] != 0;
    }

    public ushort ByteSwap2(ushort value)
    {
        return (ushort)((value >> 8) | (value << 8));
    }

    public uint ByteSwap4(uint value)
    {
        return ((value >> 24) & 0xFF) | ((value >> 8) & 0xFF00) | ((value << 8) & 0xFF0000) | ((value << 24) & 0xFF000000u);
    }

    public ulong ByteSwap8(ulong value)
    {
        return ((value >> 56) & 0xFF) | ((value >> 40) & 0xFF00) | ((value >> 24) & 0xFF0000) | ((value >> 8) & 0xFF000000uL) |
               ((value << 8) & 0xFF00000000L) | ((value << 24) & 0xFF0000000000L) | ((value << 40) & 0xFF000000000000L) | ((value << 56) & 0xFF00000000000000L);
    }

    public int ReverseEndianness(int value)
    {
        unchecked
        {
            return (int)ByteSwap4((uint)value);
        }
    }

    public uint ReverseEndianness(uint value)
    {
        return ByteSwap4(value);
    }
}
