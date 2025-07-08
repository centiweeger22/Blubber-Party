using UnityEngine;
using UnityEngine.UI;

public class MobileMenuManager : MenuManager
{
	public enum EOrientation
	{
		VERTICAL = 0,
		HORIZONTAL = 1
	}

	public CanvasScaler scaler;

	public EOrientation currentOrientation;

	public DualLayout[] dualLayouts;

	public UnityEngine.UI.Button jumpButton;

	public UnityEngine.UI.Button fireButton;

	protected bool joystick;

	protected float joystickFadeLerp;

	public float joystickFadeTime;

	public RawImage[] joystickImages;

	protected float[] joystickOriginalAlphas;

	public RawImage joystickImage;

	public RawImage knobImage;

	public float originalJoystickSize = 400f;

	public float virtualJoystickSize = 1.5f;

	public float deadzone = 0.2f;

	public override void Initialise()
	{
		base.Initialise();
		joystickOriginalAlphas = new float[joystickImages.Length];
		for (int i = 0; i < joystickImages.Length; i++)
		{
			joystickOriginalAlphas[i] = joystickImages[i].color.a;
		}
		SetJoystickAlpha(0f);
		EOrientation orientation = DetectOrientation();
		SetOrientation(orientation);
	}

	public override void PerFrameUpdate()
	{
		UpdateOrientation();
		ScaleVirtualJoystick();
		UpdateJoystick();
		base.PerFrameUpdate();
	}

	public EOrientation DetectOrientation()
	{
		if (Screen.height > Screen.width)
		{
			return EOrientation.VERTICAL;
		}
		return EOrientation.HORIZONTAL;
	}

	public void UpdateOrientation()
	{
		EOrientation eOrientation = DetectOrientation();
		if (currentOrientation != eOrientation)
		{
			currentOrientation = eOrientation;
			SetOrientation(eOrientation);
		}
	}

	public void SetOrientation(EOrientation orientation)
	{
		switch (orientation)
		{
		case EOrientation.VERTICAL:
		{
			DualLayout[] array = dualLayouts;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetVerticalLayout();
			}
			break;
		}
		case EOrientation.HORIZONTAL:
		{
			DualLayout[] array = dualLayouts;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetHorizontalLayout();
			}
			break;
		}
		}
	}

	public override void DisconnectButtonPressed()
	{
		client.SendDisconnect();
		if (Settings.platformType == Settings.EPlatformType.ANDROID)
		{
			new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity").Call<bool>("moveTaskToBack", new object[1] { true });
		}
		else if (Settings.platformType == Settings.EPlatformType.IOS)
		{
			Application.Quit();
		}
	}

	public void PollGameButtons()
	{
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if (touch.phase == TouchPhase.Began)
			{
				if (MobileInputManager.IsTouchOverButton(touch, jumpButton))
				{
					JumpButtonPressed();
				}
				else if (MobileInputManager.IsTouchOverButton(touch, fireButton))
				{
					FireButtonPressed();
				}
			}
		}
	}

	public void JumpButtonPressed()
	{
		buttonData.jump = true;
	}

	public void FireButtonPressed()
	{
		buttonData.fire = true;
	}

	public void ScaleVirtualJoystick()
	{
		float dpi = Screen.dpi;
		float a = (float)Mathf.Min(Screen.width, Screen.height) / scaler.referenceResolution.x;
		float b = (float)Mathf.Max(Screen.width, Screen.height) / scaler.referenceResolution.y;
		float num = Mathf.Lerp(a, b, scaler.matchWidthOrHeight);
		float num2 = originalJoystickSize * num / dpi;
		float num3 = virtualJoystickSize / num2;
		joystickImage.rectTransform.localScale = Vector3.one * num3;
	}

	public void PollVirtualJoystick()
	{
		ManagedTouch managedTouch = null;
		float num = jumpButton.GetComponent<RectTransform>().position.y / (float)Screen.height;
		int count = touchData.touchList.Count;
		for (int i = 0; i < count; i++)
		{
			ManagedTouch managedTouch2 = touchData.touchList[i];
			if (currentOrientation == EOrientation.VERTICAL)
			{
				if (managedTouch2.start.y < (float)Screen.height * num)
				{
					managedTouch = managedTouch2;
					break;
				}
			}
			else if (currentOrientation == EOrientation.HORIZONTAL && managedTouch2.start.x < (float)Screen.width * 0.5f)
			{
				managedTouch = managedTouch2;
				break;
			}
		}
		if (managedTouch == null)
		{
			buttonData.joystick = Vector2.zero;
			joystick = false;
			return;
		}
		float dpi = Screen.dpi;
		float a = (float)Mathf.Min(Screen.width, Screen.height) / scaler.referenceResolution.x;
		float b = (float)Mathf.Max(Screen.width, Screen.height) / scaler.referenceResolution.y;
		float num2 = Mathf.Lerp(a, b, scaler.matchWidthOrHeight);
		float num3 = originalJoystickSize * num2 / dpi;
		float num4 = virtualJoystickSize / num3;
		Vector2 vector = managedTouch.touch.position - managedTouch.start;
		vector.x /= dpi;
		vector.y /= dpi;
		vector /= virtualJoystickSize * num4 * 0.5f;
		if (vector.sqrMagnitude > 1f)
		{
			vector.Normalize();
		}
		buttonData.joystick = vector;
		joystickImage.rectTransform.position = new Vector3(managedTouch.start.x, managedTouch.start.y, joystickImage.rectTransform.position.z);
		float num5 = originalJoystickSize * 0.5f;
		knobImage.rectTransform.localPosition = new Vector3(vector.x * num5, vector.y * num5, knobImage.rectTransform.localPosition.z);
		joystick = true;
	}

	public void UpdateJoystick()
	{
		if (joystick)
		{
			if (joystickFadeLerp < 1f)
			{
				float num = 1f / joystickFadeTime;
				joystickFadeLerp += num * Time.deltaTime;
				if (joystickFadeLerp > 1f)
				{
					joystickFadeLerp = 1f;
				}
				SetJoystickAlpha(joystickFadeLerp);
			}
		}
		else if (joystickFadeLerp > 0f)
		{
			float num2 = 1f / joystickFadeTime;
			joystickFadeLerp -= num2 * Time.deltaTime;
			if (joystickFadeLerp < 0f)
			{
				joystickFadeLerp = 0f;
			}
			SetJoystickAlpha(joystickFadeLerp);
		}
	}

	public void SetJoystickAlpha(float alpha)
	{
		for (int i = 0; i < joystickImages.Length; i++)
		{
			RawImage rawImage = joystickImages[i];
			rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, joystickOriginalAlphas[i] * alpha);
		}
	}

	public override void UpdateTyping()
	{
		// if (mobileKeyboard == null)
		// {
		// 	username = "";
		// 	usernameText.text = username;
		// 	isTyping = false;
		// 	return;
		// }
		// if (mobileKeyboard.status == TouchScreenKeyboard.Status.Canceled || mobileKeyboard.status == TouchScreenKeyboard.Status.Done)
		// {
		// 	username = "";
		// 	usernameText.text = username;
		// 	isTyping = false;
		// 	return;
		// }
		string text = mobileKeyboard.text;
		username = "";
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			username += MathExtension.SanitiseSimplifiedAlphabetChar(text[i]);
		}
		usernameText.text = username;
		if (username.Length == 3)
		{
			isTyping = false;
			client.preferences.username = username;
			client.SendPreferences(client.preferences);
		}
	}
}
