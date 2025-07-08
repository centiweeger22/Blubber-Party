using System;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour
{
	public EInput inputType;

	public MenuManager menuManager;

	public Transform cameraElbow;

	public Quaternion frame = Quaternion.identity;

	public Transform defaultFocus;

	public Transform focus;

	public float yaw;

	public float pitch;

	public float range = 5f;

	public float trueRange = 5f;

	public float sensitivity;

	public KeyCode connect = KeyCode.Mouse0;

	public KeyCode release = KeyCode.Escape;

	public TouchData touchData;

	private float collisionRadius = 0.2f;

	public LayerMask blockingMask;

	public bool isThirdPerson;

	public bool fuzz;

	public bool isRouted;

	private void Start()
	{
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}

	public void Poll()
	{
		if (menuManager != null)
		{
			if (Input.GetKeyDown(connect) && !menuManager.isActive)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
			if (Input.GetKeyDown(release) && !menuManager.isActive)
			{
				Cursor.lockState = CursorLockMode.None;
				menuManager.ActivateMenu();
			}
		}
		if (isRouted)
		{
			if (!isThirdPerson)
			{
				TogglePerspective();
			}
			range = 8f;
		}
		else
		{
			range = trueRange;
		}
		if (inputType == EInput.DESKTOP)
		{
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				yaw += Input.GetAxisRaw("Mouse X") * sensitivity;
				pitch += Input.GetAxisRaw("Mouse Y") * sensitivity;
				if (yaw < (float)Math.PI * -2f)
				{
					yaw += (float)Math.PI * 2f;
				}
				if (yaw > (float)Math.PI * 2f)
				{
					yaw -= (float)Math.PI * 2f;
				}
				pitch = Mathf.Clamp(pitch, -1.5697963f, 1.5697963f);
			}
		}
		else if (inputType == EInput.MOBILE)
		{
			MobileMenuManager mobileMenuManager = menuManager as MobileMenuManager;
			ManagedTouch managedTouch = null;
			float num = mobileMenuManager.jumpButton.GetComponent<RectTransform>().position.y / (float)Screen.height;
			int count = touchData.touchList.Count;
			for (int i = 0; i < count; i++)
			{
				ManagedTouch managedTouch2 = touchData.touchList[i];
				if (mobileMenuManager.currentOrientation == MobileMenuManager.EOrientation.VERTICAL)
				{
					if (managedTouch2.start.y >= (float)Screen.height * num)
					{
						managedTouch = managedTouch2;
						break;
					}
				}
				else if (mobileMenuManager.currentOrientation == MobileMenuManager.EOrientation.HORIZONTAL && managedTouch2.start.x >= (float)Screen.width * 0.5f)
				{
					managedTouch = managedTouch2;
					break;
				}
			}
			if (managedTouch != null)
			{
				float dpi = Screen.dpi;
				yaw += managedTouch.touch.deltaPosition.x * sensitivity / dpi;
				pitch += managedTouch.touch.deltaPosition.y * sensitivity / dpi;
			}
			if (yaw < (float)Math.PI * -2f)
			{
				yaw += (float)Math.PI * 2f;
			}
			if (yaw > (float)Math.PI * 2f)
			{
				yaw -= (float)Math.PI * 2f;
			}
			pitch = Mathf.Clamp(pitch, -1.5697963f, 1.5697963f);
		}
		if (fuzz)
		{
			yaw = UnityEngine.Random.Range(-(float)Math.PI, (float)Math.PI);
			pitch = UnityEngine.Random.Range(-1.5697963f, 1.5697963f);
		}
		if (focus == null)
		{
			focus = defaultFocus;
		}
		cameraElbow.rotation = frame;
		Vector3 vector = MathExtension.DirectionFromYawPitch(yaw, pitch);
		Vector3 vector2 = cameraElbow.TransformDirection(vector);
		Vector3 vector3 = focus.position + vector2 * range;
		RaycastHit hitInfo;
		if (Physics.SphereCast(focus.position, collisionRadius, (vector3 - focus.position).normalized, out hitInfo, range, blockingMask.value))
		{
			cameraElbow.position = hitInfo.point + hitInfo.normal * collisionRadius;
		}
		else
		{
			cameraElbow.position = focus.position + vector2 * range;
		}
		base.transform.localRotation = Quaternion.LookRotation(-vector);
	}

	public void TogglePerspective()
	{
		isThirdPerson = !isThirdPerson;
		if (isRouted)
		{
			isThirdPerson = true;
		}
		if (menuManager.client.proxy != null)
		{
			MeshRenderer[] componentsInChildren = menuManager.client.proxy.animator.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				if (isThirdPerson)
				{
					meshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
				}
				else
				{
					meshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
				}
			}
		}
		if (isThirdPerson)
		{
			if (menuManager.perspectiveButtonText != null)
			{
				menuManager.perspectiveButtonText.text = "First-Person";
			}
			trueRange = 2f;
		}
		else
		{
			if (menuManager.perspectiveButtonText != null)
			{
				menuManager.perspectiveButtonText.text = "Third-Person";
			}
			trueRange = 0.01f;
		}
	}

	public void CameraCorrection(Quaternion oldFrame, Quaternion newFrame)
	{
		Vector3 vector = oldFrame * Vector3.forward;
		Vector3 vector2 = newFrame * Vector3.forward;
		Vector2 vector3 = new Vector2(vector.x, vector.z);
		Vector2 vector4 = new Vector2(vector2.x, vector2.z);
		float unsignedAngle = Vector2.Angle(vector3.normalized, vector4.normalized);
		float sign = Mathf.Sign(vector3.normalized.x * vector4.normalized.y - vector3.normalized.y * vector4.normalized.x);
		float num =  (unsignedAngle * sign)* ((float)Math.PI / 180f);
		yaw += num;
	}

	public void SetCamera(float yaw, float pitch)
	{
		this.yaw = yaw;
		this.pitch = pitch;
	}
}
