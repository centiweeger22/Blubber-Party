using System;

public class Hash
{
    public static int ComputeHash(byte[] data, int length)
    {
        int num = -2128831035;
        for (int i = 0; i < length; i++)
        {
            num = (num ^ data[i]) * 16777619;
        }
        return num;
    }

    // Manual int endian swap for Unity 5
    private static int ReverseEndianness(int value)
    {
        unchecked
        {
            return (int)(
                ((value & 0x000000FF) << 24) |
                ((value & 0x0000FF00) << 8) |
                ((value & 0x00FF0000) >> 8) |
                ((value & 0xFF000000) >> 24));
        }
    }

    public static byte[] SignBuffer(byte[] buffer)
    {
        int num = buffer.Length;
        byte[] array = new byte[num + 4];
        Buffer.BlockCopy(buffer, 0, array, 0, num);
        int num2 = ComputeHash(buffer, num);
        if (Settings.PLATFORM_ENDIANNESS != Settings.Endian.LITTLE)
        {
            num2 = ReverseEndianness(num2);
        }
        BitStream bitStream = new BitStream(array);
        bitStream.bitIndex = num * 8;
        int data = num2 ^ 7;
        bitStream.WriteInt(data, 32);
        return array;
    }

    public static bool VerifyBuffer(ref byte[] buffer, int length)
    {
        if (length <= 4)
        {
            return false;
        }
        int num = length - 4;
        int num2 = new BitStream(buffer)
        {
            bitIndex = num * 8
        }.ReadInt(32);
        int num3 = ComputeHash(buffer, num);
        if (Settings.PLATFORM_ENDIANNESS != Settings.Endian.LITTLE)
        {
            num3 = ReverseEndianness(num3);
        }
        int num4 = num3 ^ 7;
        byte[] array = new byte[1300];
        Buffer.BlockCopy(buffer, 0, array, 0, num);
        buffer = array;
        return num4 == num2;
    }
}
