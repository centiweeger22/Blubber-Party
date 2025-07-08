using System;
using UnityEngine;

public class PlayerEntity : TransformEntity
{
	public delegate void CameraCorrectionFunc(Quaternion oldFrame, Quaternion newFrame);

	public delegate void SetCameraFunc(float yaw, float pitch);

	public static float CONTINUOUS_SWITCH = 5f;

	public Rigidbody body;

	public CapsuleCollider capsule;

	public MeshRenderer meshRenderer;

	public PlayerCosmetics cosmetics;

	public CameraCorrectionFunc cameraCorrectionCallback;

	public SetCameraFunc setCameraCallback;

	public TransformEntity animator;

	public Vector3 nameTagLocalPosition;

	public GameObject nameTag;

	public int parentId = -1;

	public bool isRouted;

	public string previousUsername = "";

	public string username = "";

	public InputSample input;

	public bool isLocal;

	public bool isServer = true;

	public bool manual;

	private const float EPSILON = 0.01f;

	public const float EXACT_EPSILON = 0.01f;

	public float gravity = -9.8f;

	public float reverseAccelBonus = 0.2f;

	public float walkAcceleration = 15f;

	public float walkFriction = 0.01f;

	public float walkAngle = 45f;

	public float airAcceleration = 2f;

	public float airFriction = 0.8f;

	public const float NO_GROUND_TIME = 0.2f;

	public const float AIR_TIME = 0.3f;

	public float jumpPower = 10f;

	public float noGroundTimer;

	public float airTimer;

	public float maxSpeed;

	public float maxAirSpeed = 0.5f;

	public float sideFriction = 0.01f;

	public float maxWalkAngle = 45f;

	public Rigidbody groundBody;

	public Vector3 currentPlane = Vector3.up;

	public bool wasGrounded;

	public Vector3 previousVelocity = Vector3.zero;

	public Vector3 previousPlane = Vector3.zero;

	public Quaternion frame = Quaternion.identity;

	public override void Initialise()
	{
		if (!isServer)
		{
			nameTagLocalPosition = new Vector3(nameTag.transform.localPosition.x * base.transform.localScale.x, nameTag.transform.localPosition.y * base.transform.localScale.y, nameTag.transform.localPosition.z * base.transform.localScale.z);
		}
		base.Initialise();
		dirtyFlagLength += 4;
		cameraCorrectionCallback = (CameraCorrectionFunc)Delegate.Combine(cameraCorrectionCallback, new CameraCorrectionFunc(DefaultFunc));
		setCameraCallback = (SetCameraFunc)Delegate.Combine(setCameraCallback, new SetCameraFunc(DefaultFunc));
	}

	public void DefaultFunc(Quaternion oldFrame, Quaternion newFrame)
	{
	}

	public void DefaultFunc(float yaw, float pitch)
	{
	}

	public override void WriteToStream(ref BitStream stream)
	{
		base.WriteToStream(ref stream);
		stream.WriteBool(parentId >= 0);
		if (parentId >= 0)
		{
			stream.WriteInt(parentId, Settings.MAX_ENTITY_BITS);
		}
		stream.WriteBool(isRouted);
		if (username.Length == 3)
		{
			stream.WriteBool(data: true);
			byte data = MathExtension.ConvertSimplifiedAlphabetToByte(username[0]);
			byte data2 = MathExtension.ConvertSimplifiedAlphabetToByte(username[1]);
			byte data3 = MathExtension.ConvertSimplifiedAlphabetToByte(username[2]);
			stream.WriteBits(data, 5);
			stream.WriteBits(data2, 5);
			stream.WriteBits(data3, 5);
		}
		else
		{
			stream.WriteBool(data: false);
		}
		cosmetics.WriteToStream(ref stream);
	}

	public override void ReadFromStream(ref BitStream stream)
	{
		base.ReadFromStream(ref stream);
		if (stream.ReadBool())
		{
			parentId = stream.ReadInt(Settings.MAX_ENTITY_BITS);
		}
		else
		{
			parentId = -1;
		}
		bool flag = isRouted;
		isRouted = stream.ReadBool();
		if (setCameraCallback != null && !flag && isRouted)
		{
			setCameraCallback(0f, 0f);
		}
		if (stream.ReadBool())
		{
			char c = MathExtension.ConvertByteToSimplifiedAlphabet(stream.ReadBits(5)[0]);
			char c2 = MathExtension.ConvertByteToSimplifiedAlphabet(stream.ReadBits(5)[0]);
			char c3 = MathExtension.ConvertByteToSimplifiedAlphabet(stream.ReadBits(5)[0]);
			char[] value = new char[3] { c, c2, c3 };
			username = new string(value);
			if (!isServer && !isLocal)
			{
				if (!nameTag.activeSelf)
				{
					nameTag.SetActive(value: true);
				}
				nameTag.GetComponentInChildren<TextMesh>().text = username;
			}
		}
		else
		{
			username = "";
			if (!isServer && !isLocal && nameTag.activeSelf)
			{
				nameTag.SetActive(value: false);
			}
		}
		cosmetics.ReadFromStream(ref stream);
	}

	public void WritePlayerToStream(ref BitStream stream)
	{
		Vector3 vector = body.position;
		if (parentId >= 0)
		{
			vector = base.transform.parent.InverseTransformPoint(body.position);
		}
		stream.WriteFloat(vector.x, 32);
		stream.WriteFloat(vector.y, 32);
		stream.WriteFloat(vector.z, 32);
		Quaternion quaternion = body.rotation;
		if (parentId >= 0)
		{
			quaternion = Quaternion.Inverse(base.transform.parent.rotation) * body.rotation;
		}
		stream.WriteFloat(quaternion.x, 32);
		stream.WriteFloat(quaternion.y, 32);
		stream.WriteFloat(quaternion.z, 32);
		stream.WriteFloat(quaternion.w, 32);
		stream.WriteFloat(body.velocity.x, 32);
		stream.WriteFloat(body.velocity.y, 32);
		stream.WriteFloat(body.velocity.z, 32);
		stream.WriteFloat(noGroundTimer, 32);
		stream.WriteFloat(airTimer, 32);
		stream.WriteBool(wasGrounded);
		stream.WriteFloat(previousVelocity.x, 32);
		stream.WriteFloat(previousVelocity.y, 32);
		stream.WriteFloat(previousVelocity.z, 32);
		stream.WriteFloat(previousPlane.x, 32);
		stream.WriteFloat(previousPlane.y, 32);
		stream.WriteFloat(previousPlane.z, 32);
		stream.WriteBool(parentId >= 0);
		if (parentId >= 0)
		{
			stream.WriteInt(parentId, Settings.MAX_ENTITY_BITS);
		}
		stream.WriteBool(isRouted);
		cosmetics.WriteToStream(ref stream);
	}

	public void ReadPlayerFromStream(ref BitStream stream)
	{
		float x = stream.ReadFloat(32);
		float y = stream.ReadFloat(32);
		float z = stream.ReadFloat(32);
		position.vector = new Vector3(x, y, z);
		float x2 = stream.ReadFloat(32);
		float y2 = stream.ReadFloat(32);
		float z2 = stream.ReadFloat(32);
		float w = stream.ReadFloat(32);
		rotation.quaternion = new Quaternion(x2, y2, z2, w);
		float x3 = stream.ReadFloat(32);
		float y3 = stream.ReadFloat(32);
		float z3 = stream.ReadFloat(32);
		body.velocity = new Vector3(x3, y3, z3);
		noGroundTimer = stream.ReadFloat(32);
		airTimer = stream.ReadFloat(32);
		wasGrounded = stream.ReadBool();
		float x4 = stream.ReadFloat(32);
		float y4 = stream.ReadFloat(32);
		float z4 = stream.ReadFloat(32);
		previousVelocity = new Vector3(x4, y4, z4);
		float x5 = stream.ReadFloat(32);
		float y5 = stream.ReadFloat(32);
		float z5 = stream.ReadFloat(32);
		previousPlane = new Vector3(x5, y5, z5);
		if (stream.ReadBool())
		{
			parentId = stream.ReadInt(Settings.MAX_ENTITY_BITS);
		}
		else
		{
			parentId = -1;
		}
		bool num = isRouted;
		isRouted = stream.ReadBool();
		if (!num && isRouted)
		{
			setCameraCallback(0f, 0f);
		}
		cosmetics.ReadFromStream(ref stream);
		if (!isServer)
		{
			base.transform.localPosition = position.vector;
			base.transform.localRotation = rotation.quaternion;
			body.position = base.transform.position;
			body.rotation = base.transform.rotation;
		}
	}

	public override int GetBitLength()
	{
		int bitLength = base.GetBitLength();
		bitLength++;
		if (parentId >= 0)
		{
			bitLength += Settings.MAX_ENTITY_BITS;
		}
		bitLength++;
		bitLength++;
		if (username.Length == 3)
		{
			bitLength += 15;
		}
		return bitLength + cosmetics.GetBitLength();
	}

	public override void WriteToStreamPartial(ref BitStream stream)
	{
		base.WriteToStreamPartial(ref stream);
		stream.WriteBool(parentId >= 0);
		if (parentId >= 0)
		{
			stream.WriteInt(parentId, Settings.MAX_ENTITY_BITS);
		}
		stream.WriteBool(isRouted);
		if ((dirtyFlag & 0x80) > 0)
		{
			if (username.Length == 3)
			{
				stream.WriteBool(data: true);
				byte data = MathExtension.ConvertSimplifiedAlphabetToByte(username[0]);
				byte data2 = MathExtension.ConvertSimplifiedAlphabetToByte(username[1]);
				byte data3 = MathExtension.ConvertSimplifiedAlphabetToByte(username[2]);
				stream.WriteBits(data, 5);
				stream.WriteBits(data2, 5);
				stream.WriteBits(data3, 5);
			}
			else
			{
				stream.WriteBool(data: false);
			}
		}
		cosmetics.WriteToStreamPartial(ref stream, dirtyFlag);
	}

	public override void ReadFromStreamPartial(ref BitStream stream)
	{
		base.ReadFromStreamPartial(ref stream);
		if (stream.ReadBool())
		{
			parentId = stream.ReadInt(Settings.MAX_ENTITY_BITS);
		}
		else
		{
			parentId = -1;
		}
		bool num = isRouted;
		isRouted = stream.ReadBool();
		if (!num && isRouted)
		{
			setCameraCallback(0f, 0f);
		}
		if ((dirtyFlag & 0x80) > 0)
		{
			if (stream.ReadBool())
			{
				char c = MathExtension.ConvertByteToSimplifiedAlphabet(stream.ReadBits(5)[0]);
				char c2 = MathExtension.ConvertByteToSimplifiedAlphabet(stream.ReadBits(5)[0]);
				char c3 = MathExtension.ConvertByteToSimplifiedAlphabet(stream.ReadBits(5)[0]);
				char[] value = new char[3] { c, c2, c3 };
				username = new string(value);
				if (!isServer && !isLocal)
				{
					if (!nameTag.activeSelf)
					{
						nameTag.SetActive(value: true);
					}
					nameTag.GetComponentInChildren<TextMesh>().text = username;
				}
			}
			else
			{
				username = "";
				if (!isServer && !isLocal && nameTag.activeSelf)
				{
					nameTag.SetActive(value: false);
				}
			}
		}
		cosmetics.ReadFromStreamPartial(ref stream, dirtyFlag);
	}

	public override int GetBitLengthPartial()
	{
		int bitLengthPartial = base.GetBitLengthPartial();
		bitLengthPartial++;
		if (parentId >= 0)
		{
			bitLengthPartial += Settings.MAX_ENTITY_BITS;
		}
		bitLengthPartial++;
		bitLengthPartial++;
		if ((dirtyFlag & 0x80) > 0)
		{
			bitLengthPartial += 15;
		}
		return bitLengthPartial + cosmetics.GetBitLengthPartial(dirtyFlag);
	}

	public void ManualTick()
	{
		manual = true;
		Tick();
		manual = false;
	}

	public override void Tick()
	{
		if (isServer)
		{
			if (previousUsername != username)
			{
				dirtyFlag |= 128;
			}
			previousUsername = username;
			if (cosmetics.previousBranding != cosmetics.branding)
			{
				dirtyFlag |= 256;
			}
			cosmetics.previousBranding = cosmetics.branding;
			if (cosmetics.previousRace != cosmetics.race)
			{
				dirtyFlag |= 512;
			}
			cosmetics.previousRace = cosmetics.race;
			if (cosmetics.previousIsGold != cosmetics.isGold)
			{
				dirtyFlag |= 1024;
			}
			cosmetics.previousIsGold = cosmetics.isGold;
			if (body.velocity.sqrMagnitude > CONTINUOUS_SWITCH * CONTINUOUS_SWITCH)
			{
				body.collisionDetectionMode = CollisionDetectionMode.Continuous;
			}
			else
			{
				body.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
		}
		if (!manual)
		{
			return;
		}
		if (isLocal)
		{
			Vector2 vector = Vector2.zero;
			if (!isRouted && input != null)
			{
				vector = input.GetMovementVector();
				float yaw = input.yaw;
				vector = MathExtension.RotateVector2(vector, yaw);
			}
			if (noGroundTimer > 0f)
			{
				noGroundTimer -= Time.fixedDeltaTime;
			}
			float num = (capsule.height * 0.5f - capsule.radius) * base.transform.localScale.x;
			bool flag = false;
			groundBody = null;
			var gartrash6000 = frame;
			if (base.transform.parent != null)
			{
				frame = base.transform.parent.rotation;
			}
			if (parentId < 0)
			{
				gravity = PlanetFinder.instance.GetGravity(this);
			}
			else
			{
				gravity = 9.81f;
			}
			Quaternion quaternion = frame;
			Quaternion quaternion2 = Quaternion.Inverse(quaternion);
			Quaternion quaternion3 = quaternion;
			Quaternion quaternion4 = quaternion2;
			Vector3 vector2 = frame * Vector3.up;
			Vector3 vector3 = frame * Vector3.down;
			currentPlane = vector2;
			capsule.enabled = false;
			RaycastHit hitInfo;
			if (noGroundTimer <= 0f && Physics.SphereCast(base.transform.position, capsule.radius * base.transform.localScale.x - 0.01f, vector3, out hitInfo, num + 0.02f))
			{
				RaycastHit hitInfo2;
				Physics.Raycast(base.transform.position, (hitInfo.point - base.transform.position).normalized, out hitInfo2, float.PositiveInfinity);
				if (Vector3.Angle(vector2, hitInfo2.normal) <= maxWalkAngle)
				{
					currentPlane = hitInfo2.normal;
					quaternion3 = Quaternion.FromToRotation(vector2, currentPlane) * frame;
					quaternion4 = Quaternion.Inverse(quaternion3);
					if (!wasGrounded)
					{
						Vector3 vector4 = quaternion4 * previousVelocity;
						vector4.y = 0f;
						body.velocity = quaternion3 * vector4;
					}
					flag = true;
					walkAngle = Vector3.Angle(vector2, currentPlane);
					airTimer = 0f;
				}
			}
			if (wasGrounded && !flag && noGroundTimer <= 0f)
			{
				float num2 = (float)Math.Tan(walkAngle * ((float)Math.PI / 180f));
				Vector3 vector5 = quaternion4 * body.velocity;
				Vector2 vector6 = new Vector2(vector5.x, vector5.z);
				float num3 = vector6.magnitude * num2 * Time.fixedDeltaTime;
				if (Physics.SphereCast(base.transform.position, capsule.radius * base.transform.localScale.x - 0.01f, vector3, out hitInfo, num + 0.02f + num3))
				{
					RaycastHit hitInfo3;
					Physics.SphereCast(base.transform.position, 0.01f, (hitInfo.point + quaternion3 * new Vector3(vector6.x, 0f, vector6.y).normalized * 0.01f - base.transform.position).normalized, out hitInfo3, float.PositiveInfinity);
					if (Vector3.Angle(vector2, hitInfo3.normal) <= maxWalkAngle)
					{
						currentPlane = hitInfo3.normal;
						quaternion3 = Quaternion.FromToRotation(vector2, currentPlane) * frame;
						quaternion4 = Quaternion.Inverse(quaternion3);
						base.transform.position = hitInfo.point + hitInfo.normal * (capsule.radius * base.transform.localScale.x - 0.01f) + vector2 * (num + 0.01f);
						Vector3 vector7 = quaternion4 * body.velocity;
						vector7.y = 0f;
						body.velocity = quaternion3 * vector7;
						flag = true;
					}
				}
			}
			capsule.enabled = true;
			if (flag)
			{
				float num4 = 1f;
				Vector3 vector8 = quaternion4 * body.velocity;
				Vector2 lhs = new Vector2(vector8.x, vector8.z);
				if (Mathf.Abs(vector8.y) > 0f)
				{
					body.velocity = quaternion3 * new Vector3(vector8.x, 0f, vector8.z);
					vector8 = quaternion4 * body.velocity;
					lhs = new Vector2(vector8.x, vector8.z);
				}
				if (Vector2.Dot(lhs, vector) < 0f)
				{
					num4 += reverseAccelBonus;
				}
				body.velocity += quaternion3 * new Vector3(vector.x, 0f, vector.y) * walkAcceleration * num4 * Time.fixedDeltaTime;
				if (vector == Vector2.zero)
				{
					Vector3 vector9 = quaternion4 * body.velocity;
					float num5 = Mathf.Pow(walkFriction, Time.fixedDeltaTime);
					vector9.x *= num5;
					vector9.z *= num5;
					body.velocity = quaternion3 * vector9;
				}
				else
				{
					Vector3 vector10 = quaternion4 * body.velocity;
					Vector2 lhs2 = new Vector2(vector10.x, vector10.z);
					Vector2 vector11 = new Vector2(0f - vector.y, vector.x);
					float num6 = Mathf.Pow(sideFriction, Time.fixedDeltaTime);
					Vector2 vector12 = Vector2.Dot(lhs2, vector) * vector;
					Vector2 vector13 = vector11 * num6 * Vector2.Dot(lhs2, vector11);
					Vector2 vector14 = vector12 + vector13;
					body.velocity = quaternion3 * new Vector3(vector14.x, 0f, vector14.y);
				}
			}
			else
			{
				body.velocity += quaternion * (airAcceleration * Time.fixedDeltaTime * new Vector3(vector.x, 0f, vector.y));
				if (vector == Vector2.zero)
				{
					float num7 = Mathf.Pow(airFriction, Time.fixedDeltaTime);
					Vector3 vector15 = quaternion2 * body.velocity;
					vector15 = new Vector3(vector15.x * num7, vector15.y, vector15.z * num7);
					body.velocity = quaternion * vector15;
				}
				airTimer += Time.fixedDeltaTime;
				if (airTimer > 0.3f)
				{
					airTimer = 0.3f;
				}
			}
			if (flag)
			{
				if (input != null && input.jump.state == EButtonState.ON_PRESS)
				{
					Vector3 vector16 = quaternion2 * body.velocity;
					vector16 = new Vector3(vector16.x, jumpPower, vector16.z);
					body.velocity = quaternion * vector16;
					noGroundTimer = 0.2f;
					airTimer = 0.3f;
					groundBody = null;
					quaternion3 = quaternion;
					quaternion4 = quaternion2;
				}
			}
			else
			{
				if (airTimer < 0.3f && input != null && input.jump.state == EButtonState.ON_PRESS)
				{
					Vector3 vector17 = quaternion2 * body.velocity;
					vector17 = new Vector3(vector17.x, jumpPower, vector17.z);
					body.velocity = quaternion * vector17;
					noGroundTimer = 0.2f;
					airTimer = 0.3f;
					groundBody = null;
					quaternion3 = quaternion;
					quaternion4 = quaternion2;
				}
				body.velocity += gravity * Time.fixedDeltaTime * vector3;
			}
			Vector3 vector18 = quaternion4 * body.velocity;
			Vector2 vector19 = new Vector2(vector18.x, vector18.z);
			if (vector19.sqrMagnitude > maxSpeed * maxSpeed)
			{
				vector19 = vector19.normalized * maxSpeed;
				body.velocity = quaternion3 * new Vector3(vector19.x, vector18.y, vector19.y);
			}
			wasGrounded = flag;
			previousVelocity = body.velocity;
			previousPlane = currentPlane;
			if (isServer)
			{
				base.Tick();
			}
		}
		else
		{
			base.Tick();
		}
	}

	public void BaseTick()
	{
		previousPosition = Vector3.zero;
		previousRotation = Quaternion.identity;
		base.Tick();
	}

	public void LateTick()
	{
	}

	public new void Update()
	{
		if (!isLocal && interpolationFilter != null)
		{
			interpolationFilter.Update(base.transform, Time.deltaTime);
		}
	}

	public override void SetPriority(PlayerEntity player)
	{
		priority = 2f;
	}
}
