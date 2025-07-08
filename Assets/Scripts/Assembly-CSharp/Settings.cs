using System;
using UnityEngine;

public class Settings
{
	public enum EBuildType
	{
		SERVER = 0,
		CLIENT = 1
	}

	public enum EPlatformType
	{
		UNDEFINED = 0,
		WINDOWS = 1,
		MAC = 2,
		LINUX = 3,
		ANDROID = 4,
		IOS = 5,
		VR = 6,
		BOT = 7
	}

	public enum EMessageType
	{
		UNRELIABLE = 0,
		RELIABLE_UNORDERED = 1,
		RELIABLE_ACK = 2,
		UNRELIABLE_ORDERED = 3,
		RAW = 4
	}

	public enum ELinkingStateType
	{
		HOST = 0,
		CONNECT = 1,
		SUCCESS = 2,
		FAIL = 3,
		LINK = 4,
		LOCAL_IP_DETECTED = 5
	}

	public enum Endian
	{
		LITTLE = 0,
		BIG = 1
	}

	public const int VERSION = 7;

	public static EPlatformType platformType;

	public const int BUFFER_LIMIT = 900;

	public const int BUFFER_OVERFLOW = 5;

	public const int BUFFER_SIZE = 1300;

	public const int UDP_OVERHEAD = 28;

	public const Endian STREAM_ENDIANESS = Endian.LITTLE;

	public static Endian PLATFORM_ENDIANNESS;

	public const int MAX_ACK_NUMBER = 16384;

	public const float RELIABLE_TIMEOUT = 1f;

	public const int ACK_FIELD_SIZE = 32;

	public const int MAX_INDEX_NUMBER = 64;

	public const float CLIENT_LINKER_TIMEOUT = 1f;

	public const float SERVER_LINKER_TIMEOUT = 0.1f;

	public const float TIMEOUT = 10f;

	public const int PARTIAL_TO_FULL_RATIO = 20;

	public const int MAX_ENTITY_INDEX = 65536;

	public const int MAX_INPUT_INDEX = 65536;

	public const int MAX_TICKET_INDEX = 256;

	public const int MINIMUM_ENTITY_LIMIT = 8;

	public const float ENTITY_LIMIT_STEP = 0.05f;

	public static int MAX_ENTITY_BITS;

	public static int MAX_TYPE_BITS;

	public static int MAX_FUNCTION_TYPE_BITS;

	public static int MAX_TICKET_BITS;

	public const float WORLD_MIN_X = -500f;

	public const float WORLD_MIN_Y = -100f;

	public const float WORLD_MIN_Z = -500f;

	public const float WORLD_MAX_X = 500f;

	public const float WORLD_MAX_Y = 100f;

	public const float WORLD_MAX_Z = 500f;

	public const int MAX_MESSAGE_QUEUE_SIZE = 96;

	public const int MAX_CLIENT_SIDE_PREDICTION_SIZE = 40;

	public const float INTERPOLATION_PERIOD = 0.07f;

	public const float MAX_INTERPOLATION_DISTANCE = 6f;

	public const float ENTITY_TIMEOUT_TIME = 2f;

	public static void Initialise(EBuildType buildType, ref EPlatformType platformType)
	{
		Settings.platformType = platformType;
		MAX_ENTITY_BITS = MathExtension.RequiredBits(65536);
		MAX_TYPE_BITS = MathExtension.RequiredBits(9);
		MAX_FUNCTION_TYPE_BITS = MathExtension.RequiredBits(2);
		MAX_TICKET_BITS = MathExtension.RequiredBits(256);
		if (!BitConverter.IsLittleEndian)
		{
			PLATFORM_ENDIANNESS = Endian.BIG;
		}
		if (buildType == EBuildType.SERVER)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 300;
		}
		else if (platformType == EPlatformType.WINDOWS)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 300;
		}
		else if (platformType == EPlatformType.MAC)
		{
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = 60;
		}
		else if (platformType == EPlatformType.LINUX)
		{
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = 60;
		}
		else if (platformType == EPlatformType.ANDROID)
		{
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = 60;
		}
		else if (platformType == EPlatformType.IOS)
		{
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = 60;
		}
		else if (platformType == EPlatformType.VR)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 300;
		}
		else if (platformType == EPlatformType.BOT)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 20;
			platformType = EPlatformType.WINDOWS;
			Settings.platformType = EPlatformType.WINDOWS;
		}
	}
}
