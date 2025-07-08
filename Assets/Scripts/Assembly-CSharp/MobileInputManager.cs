using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileInputManager : InputManager
{
	public MobileInputSample sample;

	public ButtonData buttonData;

	public TouchData touchData;

	public Vector2 touchAnchor = Vector2.zero;

	public bool isTouching;

	public override void Initialise()
	{
		sample = new MobileInputSample();
		sample.Initialise();
		buttonData = new ButtonData();
		touchData = new TouchData();
		cameraController.inputType = EInput.MOBILE;
		cameraController.menuManager.inputType = EInput.MOBILE;
		cameraController.touchData = touchData;
		cameraController.menuManager.buttonData = buttonData;
		cameraController.menuManager.touchData = touchData;
	}

	public override InputSample GetInputSample()
	{
		return sample;
	}

	public override void PerFrameUpdate()
	{
		MobileMenuManager obj = cameraController.menuManager as MobileMenuManager;
		PollTouches();
		obj.PollVirtualJoystick();
		sample.joystickX.value = 0f - buttonData.joystick.x;
		sample.joystickY.value = buttonData.joystick.y;
		sample.joystickX.Quantize();
		sample.joystickY.Quantize();
		obj.PollGameButtons();
		if (buttonData.jump)
		{
			sample.jump.state = EButtonState.ON_PRESS;
			sample.jump.changeDetected = true;
			buttonData.jump = false;
		}
		if (buttonData.fire)
		{
			sample.fire.state = EButtonState.ON_PRESS;
			sample.fire.changeDetected = true;
			buttonData.fire = false;
		}
		sample.jump.Poll(true, fuzz);
		sample.fire.Poll(true, fuzz);
		cameraController.Poll();
		sample.yaw = cameraController.yaw;
		sample.pitch = 0f;
		if (!cameraController.isThirdPerson || cameraController.isRouted)
		{
			sample.pitch = cameraController.pitch;
		}
	}

	public override void Tick()
	{
		sample.jump.Reset();
		sample.fire.Reset();
		sample.timestamp++;
		if (sample.timestamp >= 65536)
		{
			sample.timestamp -= 65536;
		}
	}

	public void PollTouches()
	{
		int num = touchData.touchList.Count;
		for (int i = 0; i < num; i++)
		{
			ManagedTouch managedTouch = touchData.touchList[i];
			bool flag = true;
			for (int j = 0; j < Input.touchCount; j++)
			{
				Touch touch = Input.GetTouch(j);
				if (touch.fingerId == managedTouch.fingerId)
				{
					managedTouch.touch = touch;
					flag = false;
				}
				if (managedTouch.touch.phase == TouchPhase.Canceled || managedTouch.touch.phase == TouchPhase.Ended)
				{
					touchData.touchList.RemoveAt(i);
					i--;
					num--;
					break;
				}
			}
			if (flag)
			{
				touchData.touchList.RemoveAt(i);
				i--;
				num--;
			}
		}
		for (int k = 0; k < Input.touchCount; k++)
		{
			Touch touch2 = Input.GetTouch(k);
			if (touch2.phase != TouchPhase.Began || IsTouchOverUI(touch2))
			{
				continue;
			}
			bool flag2 = true;
			for (int l = 0; l < num; l++)
			{
				if (touchData.touchList[l].fingerId == touch2.fingerId)
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				ManagedTouch managedTouch2 = new ManagedTouch();
				managedTouch2.index = k;
				managedTouch2.fingerId = touch2.fingerId;
				managedTouch2.touch = touch2;
				managedTouch2.start = touch2.position;
				touchData.touchList.Add(managedTouch2);
			}
		}
	}

	public static bool IsTouchOverUI(Touch touch)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = new Vector2(touch.position.x, touch.position.y);
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		return list.Count > 0;
	}

	public static bool IsTouchOverButton(Touch touch, UnityEngine.UI.Button button)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = new Vector2(touch.position.x, touch.position.y);
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (list[i].gameObject == button.gameObject)
			{
				return true;
			}
		}
		return false;
	}
}
