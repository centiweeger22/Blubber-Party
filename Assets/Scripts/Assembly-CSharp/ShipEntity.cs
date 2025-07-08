using UnityEngine;

public class ShipEntity : TransformEntity
{
	public static Vector3 BLACKHOLE_POSITION = new Vector3(-100f, -9.5f, -250f);

	public static float BLACKHOLE_MAX_DIST = 12.5f;

	public static float BLACKHOLE_MIN_DIST = 150f;

	public static float BLACKHOLE_STRENGTH_MAX = 9f;

	public static float BLACKHOLE_STRENGTH_MIN = 0f;

	public Vector3 startingPosition;

	public Quaternion startingRotation;

	public GameObject tail;

	public Follow tailScript;

	public Rigidbody body;

	public BoxCollider inside;

	public BoxCollider drive;

	public PlayerEntity controller;

	public float MAX_SPEED = 5f;

	public float ACCELERATION = 3f;

	public float FRICTION = 0.5f;

	public float SIDE_FRICTION = 0.5f;

	public float EPSILON = 0.1f;

	public float KP = 5f;

	public float KD = 1f;

	public float KPA = 0.02f;

	public float KDA = 0.02f;

	public float battery = 1f;

	public float depletionRate = 0.002f;

	public bool isStarted;

	public float CUSHION = 10f;

	public float DRIFT = 0.2f;

	public override void Initialise()
	{
		base.Initialise();
		startingPosition = base.transform.position;
		startingRotation = base.transform.rotation;
		tail = new GameObject("Tail(ShipEntity)");
		tailScript = tail.AddComponent<Follow>();
		tailScript.target = base.transform;
	}

	public override void WriteToStream(ref BitStream stream)
	{
		base.WriteToStream(ref stream);
		stream.WriteFloat(battery, 32);
	}

	public override void ReadFromStream(ref BitStream stream)
	{
		base.ReadFromStream(ref stream);
		battery = stream.ReadFloat(32);
	}

	public override int GetBitLength()
	{
		return base.GetBitLength() + 32;
	}

	public override void ReadFromStreamPartial(ref BitStream stream)
	{
		base.ReadFromStreamPartial(ref stream);
		battery = stream.ReadFloat(32);
	}

	public override void WriteToStreamPartial(ref BitStream stream)
	{
		base.WriteToStreamPartial(ref stream);
		stream.WriteFloat(battery, 32);
	}

	public override int GetBitLengthPartial()
	{
		return base.GetBitLengthPartial() + 32;
	}

	public override void Tick()
	{
		Vector2 vector = Vector2.zero;
		bool flag = false;
		Vector3 vector2 = BLACKHOLE_POSITION - base.transform.position;
		float sqrMagnitude = vector2.sqrMagnitude;
		if (sqrMagnitude > BLACKHOLE_MAX_DIST * BLACKHOLE_MAX_DIST && sqrMagnitude < BLACKHOLE_MIN_DIST * BLACKHOLE_MIN_DIST)
		{
			float num = Mathf.Sqrt(sqrMagnitude);
			float num2 = Mathf.InverseLerp(BLACKHOLE_MAX_DIST, BLACKHOLE_MIN_DIST, num);
			num2 = 1f - num2;
			num2 *= num2;
			num2 = 1f - num2;
			float num3 = Mathf.Lerp(BLACKHOLE_STRENGTH_MAX, BLACKHOLE_STRENGTH_MIN, num2) * ACCELERATION;
			Vector3 lhs = Vector3.forward;
			if (controller != null)
			{
				lhs = base.transform.rotation * Vector3.forward;
			}
			float num4 = Vector3.Dot(lhs, vector2);
			float num5 = 1f;
			if (num4 < 0f)
			{
				num5 = 0.1f;
			}
			body.velocity += vector2 / num * num3 * num5 * Time.fixedDeltaTime;
			flag = true;
		}
		if (controller != null)
		{
			Vector3 vector3 = Vector3.zero;
			if (controller.input != null)
			{
				vector = controller.input.GetMovementVector();
				vector3 = controller.frame * -controller.input.GetLookVector();
			}
			Vector3 vector4 = base.transform.rotation * Vector3.forward;
			var gartrash100 = base.transform.rotation * Vector3.up;
			float num6 = 0.9f;
			if (Vector3.Dot(vector4, vector3) < num6)
			{
				Vector3 vector5 = Vector3.Cross(vector4, vector3);
				float magnitude = (vector4 - vector3).magnitude;
				Vector3 vector6 = vector5.normalized * magnitude * KP;
				Vector3 vector7 = -body.angularVelocity * KD;
				Vector3 vector8 = vector6 + vector7;
				body.angularVelocity += vector8 * Time.fixedDeltaTime;
				float num7 = base.transform.eulerAngles.z;
				if (num7 > 180f)
				{
					num7 -= 360f;
				}
				float num8 = Mathf.Sign(num7);
				float num9 = Mathf.Abs(num7);
				float num10 = num8 * num9 * KPA;
				float num11 = (0f - Vector3.Dot(body.angularVelocity, vector4)) * KDA;
				float num12 = num10 + num11;
				body.angularVelocity -= vector4 * num12 * Time.fixedDeltaTime;
			}
			else
			{
				Vector3 vector9 = -body.angularVelocity * KD;
				body.angularVelocity += vector9 * Time.fixedDeltaTime;
				float num13 = base.transform.eulerAngles.z;
				if (num13 > 180f)
				{
					num13 -= 360f;
				}
				float num14 = Mathf.Sign(num13);
				float num15 = Mathf.Abs(num13);
				float num16 = num14 * num15 * KPA;
				float num17 = (0f - Vector3.Dot(body.angularVelocity, vector4)) * KDA;
				float num18 = num16 + num17;
				body.angularVelocity -= vector4 * num18 * Time.fixedDeltaTime;
			}
			Vector3 vector10 = new Vector3(0f - vector.x, 0f, vector.y);
			Vector3 vector11 = base.transform.rotation * vector10 * ACCELERATION;
			float sqrMagnitude2 = body.velocity.sqrMagnitude;
			if (sqrMagnitude2 > MAX_SPEED * MAX_SPEED)
			{
				float num19 = Mathf.Sqrt(sqrMagnitude2);
				body.velocity += vector11 * Time.fixedDeltaTime;
				if (body.velocity.sqrMagnitude > num19 * num19)
				{
					float num20 = num19 / body.velocity.magnitude;
					body.velocity *= num20;
				}
			}
			else
			{
				body.velocity += vector11 * Time.fixedDeltaTime;
			}
			if (vector == Vector2.zero)
			{
				if (!flag)
				{
					float num21 = Mathf.Pow(FRICTION, Time.fixedDeltaTime);
					body.velocity *= num21;
				}
			}
			else
			{
				float num22 = Mathf.Pow(SIDE_FRICTION, Time.fixedDeltaTime);
				Vector3 normalized = vector11.normalized;
				Vector3 vector12 = Vector3.Cross(normalized, Vector3.up);
				Vector3 vector13 = Vector3.Cross(vector12, normalized);
				float num23 = Vector3.Dot(body.velocity, normalized);
				float num24 = Vector3.Dot(body.velocity, vector12);
				float num25 = Vector3.Dot(body.velocity, vector13);
				num24 *= num22;
				num25 *= num22;
				body.velocity = normalized * num23 + vector12 * num24 + vector13 * num25;
			}
		}
		else
		{
			Vector3 vector14 = base.transform.rotation * Vector3.forward;
			if (!flag)
			{
				float num26 = Mathf.Pow(FRICTION, Time.fixedDeltaTime);
				body.velocity *= num26;
				Vector3 vector15 = -body.angularVelocity * KD;
				body.angularVelocity += vector15 * Time.fixedDeltaTime;
			}
			float num27 = base.transform.eulerAngles.z;
			if (num27 > 180f)
			{
				num27 -= 360f;
			}
			float num28 = Mathf.Sign(num27);
			float num29 = Mathf.Abs(num27);
			float num30 = num28 * num29 * KPA;
			float num31 = (0f - Vector3.Dot(body.angularVelocity, vector14)) * KDA;
			float num32 = num30 + num31;
			body.angularVelocity -= vector14 * num32 * Time.fixedDeltaTime;
			if (type == EObject.SMALL_SHIP)
			{
				if (base.transform.position.y < GameServer.DEATH_Y + CUSHION)
				{
					base.transform.position += Vector3.up * DRIFT * Time.fixedDeltaTime;
				}
				else if (base.transform.position.y > 0f - GameServer.DEATH_Y - CUSHION)
				{
					base.transform.position -= Vector3.up * DRIFT * Time.fixedDeltaTime;
				}
			}
		}
		if ((startingPosition - base.transform.position).sqrMagnitude > EPSILON * EPSILON)
		{
			isStarted = true;
		}
		if (isStarted)
		{
			battery -= depletionRate * Time.fixedDeltaTime;
			if (battery <= 0f)
			{
				battery = 1f;
				isStarted = false;
				if (controller != null)
				{
					controller.parentId = -1;
					controller.frame = Quaternion.identity;
					controller.isRouted = false;
					controller.transform.SetParent(null);
					PlayerEntity[] componentsInChildren = GetComponentsInChildren<PlayerEntity>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].parentId = -1;
						componentsInChildren[i].frame = Quaternion.identity;
						componentsInChildren[i].transform.SetParent(null);
					}
					controller = null;
				}
				base.transform.position = startingPosition;
				base.transform.rotation = startingRotation;
				body.velocity = Vector3.zero;
				body.angularVelocity = Vector3.zero;
			}
		}
		position.vector = base.transform.position;
		rotation.quaternion = base.transform.rotation;
		if (position.vector.x != previousPosition.x)
		{
			dirtyFlag |= 1;
		}
		if (position.vector.y != previousPosition.y)
		{
			dirtyFlag |= 2;
		}
		if (position.vector.z != previousPosition.z)
		{
			dirtyFlag |= 4;
		}
		if (rotation.quaternion.x != previousRotation.x)
		{
			dirtyFlag |= 8;
			dirtyFlag |= 64;
		}
		if (rotation.quaternion.y != previousRotation.y)
		{
			dirtyFlag |= 16;
			dirtyFlag |= 64;
		}
		if (rotation.quaternion.z != previousRotation.z)
		{
			dirtyFlag |= 32;
			dirtyFlag |= 64;
		}
		if (rotation.quaternion.w != previousRotation.w)
		{
			dirtyFlag |= 64;
		}
		previousPosition = position.vector;
		previousRotation = rotation.quaternion;
	}

	public void OnDestroy()
	{
		if (!(tail == null))
		{
			Object.Destroy(tail);
		}
	}
}
