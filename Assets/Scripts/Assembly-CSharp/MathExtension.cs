using System;
using UnityEngine;

public class MathExtension
{
	public static readonly int[] MultiplyDeBruijnBitPosition = new int[32]
	{
		0, 9, 1, 10, 13, 21, 2, 29, 11, 14,
		16, 18, 22, 25, 3, 30, 8, 12, 20, 28,
		15, 17, 24, 7, 19, 27, 23, 6, 26, 5,
		4, 31
	};

	public static int FastLog2Integer(int value)
	{
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		return MultiplyDeBruijnBitPosition[(int)((long)value * 130329821L) >> 27];
	}

	public static int RequiredBits(int values)
	{
		return Mathf.CeilToInt(Mathf.Log(values) / Mathf.Log(2f));
	}

	public static int RequiredBits(uint values)
	{
		return Mathf.CeilToInt(Mathf.Log(values) / Mathf.Log(2f));
	}

	public static Vector2 RotateVector2(Vector2 vector, float angle)
	{
		float num = Mathf.Sin(angle);
		float num2 = Mathf.Cos(angle);
		Vector2 vector2 = vector;
		Vector2 zero = Vector2.zero;
		zero.x = (0f - num2) * vector2.x + num * vector2.y;
		zero.y = num * vector2.x + num2 * vector2.y;
		return zero;
	}

	public static Vector3 DirectionFromYawPitch(float yaw, float pitch)
	{
		float num = Mathf.Sin(yaw);
		float num2 = Mathf.Cos(yaw);
		float num3 = Mathf.Sin(pitch);
		float num4 = Mathf.Cos(pitch);
		Vector2 up = Vector2.up;
		up.x = -1f * num3;
		up.y = 1f * num4;
		Vector3 vector = new Vector3(0f, up.x, 0f - up.y);
		Vector3 result = vector;
		result.x = (0f - num2) * vector.x + num * vector.z;
		result.z = num * vector.x + num2 * vector.z;
		return result;
	}

	public static Vector3 RotateWithYawPitch(Vector3 vector, float yaw, float pitch)
	{
		Vector3 vector2 = Vector3.Cross(vector, Vector3.up);
		Vector3 axis = Vector3.Cross(vector2, vector);
		Quaternion quaternion = Quaternion.AngleAxis(yaw, axis);
		Quaternion quaternion2 = Quaternion.AngleAxis(pitch, vector2);
		vector = quaternion * vector;
		vector = quaternion2 * vector;
		return vector;
	}

	public static string SanitiseSimplifiedAlphabetChar(char c)
{
	switch (c)
	{
		case 'a': case 'A': return "a";
		case 'b': case 'B': return "b";
		case 'c': case 'C': return "c";
		case 'd': case 'D': return "d";
		case 'e': case 'E': return "e";
		case 'f': case 'F': return "f";
		case 'g': case 'G': return "g";
		case 'h': case 'H': return "h";
		case 'i': case 'I': return "i";
		case 'j': case 'J': return "j";
		case 'k': case 'K': return "k";
		case 'l': case 'L': return "l";
		case 'm': case 'M': return "m";
		case 'n': case 'N': return "n";
		case 'o': case 'O': return "o";
		case 'p': case 'P': return "p";
		case 'q': case 'Q': return "q";
		case 'r': case 'R': return "r";
		case 's': case 'S': return "s";
		case 't': case 'T': return "t";
		case 'u': case 'U': return "u";
		case 'v': case 'V': return "v";
		case 'w': case 'W': return "w";
		case 'x': case 'X': return "x";
		case 'y': case 'Y': return "y";
		case 'z': case 'Z': return "z";
		default: return "";
	}
}

public static byte ConvertSimplifiedAlphabetToByte(char c)
{
	switch (c)
	{
		case 'a': return 0;
		case 'b': return 1;
		case 'c': return 2;
		case 'd': return 3;
		case 'e': return 4;
		case 'f': return 5;
		case 'g': return 6;
		case 'h': return 7;
		case 'i': return 8;
		case 'j': return 9;
		case 'k': return 10;
		case 'l': return 11;
		case 'm': return 12;
		case 'n': return 13;
		case 'o': return 14;
		case 'p': return 15;
		case 'q': return 16;
		case 'r': return 17;
		case 's': return 18;
		case 't': return 19;
		case 'u': return 20;
		case 'v': return 21;
		case 'w': return 22;
		case 'x': return 23;
		case 'y': return 24;
		case 'z': return 25;
		default: return 0;
	}
}

public static char ConvertByteToSimplifiedAlphabet(byte b)
{
	switch (b)
	{
		case 0: return 'a';
		case 1: return 'b';
		case 2: return 'c';
		case 3: return 'd';
		case 4: return 'e';
		case 5: return 'f';
		case 6: return 'g';
		case 7: return 'h';
		case 8: return 'i';
		case 9: return 'j';
		case 10: return 'k';
		case 11: return 'l';
		case 12: return 'm';
		case 13: return 'n';
		case 14: return 'o';
		case 15: return 'p';
		case 16: return 'q';
		case 17: return 'r';
		case 18: return 's';
		case 19: return 't';
		case 20: return 'u';
		case 21: return 'v';
		case 22: return 'w';
		case 23: return 'x';
		case 24: return 'y';
		case 25: return 'z';
		default: return 'a';
	}
}


	public static int DiffWrapped(int a, int b, int limit)
	{
		int num = b - a;
		if (Math.Abs(num) < limit / 2)
		{
			return num;
		}
		if (a > b)
		{
			a -= limit;
		}
		else
		{
			b -= limit;
		}
		return a - b;
	}

	public static bool IsGreaterWrapped(int a, int b, int limit)
	{
		if (Math.Abs(b - a) < limit / 2)
		{
			return a > b;
		}
		if (a > b)
		{
			return false;
		}
		return true;
	}

	public static bool PointInsideBox(Vector3 point, Vector3 centre, Quaternion rotation, Vector3 size)
	{
		Vector3 vector = Quaternion.Inverse(rotation) * (point - centre);
		Vector3 vector2 = size * 0.5f;
		if (vector.x < 0f - vector2.x || vector.x > vector2.x)
		{
			return false;
		}
		if (vector.y < 0f - vector2.y || vector.y > vector2.y)
		{
			return false;
		}
		if (vector.z < 0f - vector2.z || vector.z > vector2.z)
		{
			return false;
		}
		return true;
	}

	public static bool PointInsidePlane(Plane plane, Vector3 point)
	{
		return Vector3.Dot(plane.normal, point) >= plane.distance;
	}

	public static bool SphereInsidePlane(Plane plane, Vector3 centre, float radius)
	{
		return Vector3.Dot(plane.normal, centre) - plane.distance >= 0f - radius;
	}

	public static bool BoxInsidePlane(Plane plane, Vector3 centre, Quaternion rotation, Vector3 size)
	{
		var gartrash7 = new Vector3[8];
		Vector3 vector = rotation * Vector3.right * size.x;
		Vector3 vector2 = rotation * Vector3.up * size.y;
		Vector3 vector3 = rotation * Vector3.forward * size.z;
		for (int i = 0; i < 8; i++)
		{
			int num = (i & 1) * 2 - 1;
			int num2 = ((i & 2) >> 1) * 2 - 1;
			int num3 = ((i & 4) >> 2) * 2 - 1;
			Vector3 point = centre + vector * num + vector2 * num2 + vector3 * num3;
			if (PointInsidePlane(plane, point))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CapsuleInsidePlane(Plane plane, Vector3 centre, float radius, Vector3 direction, float height)
	{
		Vector3 vector = 0.5f * direction * height;
		Vector3 centre2 = centre + vector;
		Vector3 centre3 = centre - vector;
		if (!SphereInsidePlane(plane, centre2, radius))
		{
			return SphereInsidePlane(plane, centre3, radius);
		}
		return true;
	}

	public static bool PointInsideCylinder(Vector3 point, Vector3 centre, float radius, float height)
	{
		Vector3 vector = point - centre;
		float num = height * 0.5f;
		if (vector.y > num)
		{
			return false;
		}
		if (vector.y < 0f - num)
		{
			return false;
		}
		return new Vector2(vector.x, vector.z).sqrMagnitude < radius * radius;
	}

	public static Vector3 ClosestPointOnCylinder(Vector3 point, Vector3 centre, float radius, float height)
	{
		Vector3 vector = point - centre;
		float num = height * 0.5f;
		if (vector.y > num)
		{
			vector.y = num;
		}
		if (vector.y < 0f - num)
		{
			vector.y = 0f - num;
		}
		Vector2 vector2 = new Vector2(vector.x, vector.z);
		if (vector2.sqrMagnitude > radius * radius)
		{
			vector2 = vector2.normalized * radius;
		}
		vector.x = vector2.x;
		vector.z = vector2.y;
		return centre + vector;
	}

	public static void RollTowardsDirection(ref Quaternion quaternion, Vector3 centre = default(Vector3))
	{
	}
}
