using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	public EInput inputType;

	public bool isActive;

	public Client client;

	public GameObject gamePanel;

	public GameObject basePanel;

	public GameObject settingsPanel;

	public ButtonData buttonData;

	public TouchData touchData;

	public List<GameObject> stack;

	public GameObject loading;

	public RawImage crosshair;

	public float minSensitivity = 0.01f;

	public float maxSensitivity = 0.2f;

	public Slider sensitivitySlider;

	public Slider entityLimitSlider;

	public Text perspectiveButtonText;

	public Text windowText;

	public int previousWidth = 400;

	public int previousHeight = 300;

	protected bool warning;

	public float warningThreshold = 3f;

	protected float warningFadeLerp;

	public float warningFadeTime;

	public Image[] warningImages;

	public RawImage[] warningRawImages;

	public Text warningText;

	protected float[] warningOriginalAlphas;

	protected float[] warningOriginalRawAlphas;

	protected bool information;

	public GameObject informationPanel;

	public Text informationText1;

	public Text informationText2;

	public bool isTyping;

	public string username = "";

	public Text usernameText;

	public TouchScreenKeyboard mobileKeyboard;

	public AudioSource soundSource;

	public Slider volumeSlider;

	private void Start()
	{
		Initialise();
	}

	public virtual void Initialise()
	{
		entityLimitSlider.value = 0.05f;
		warningOriginalAlphas = new float[warningImages.Length];
		for (int i = 0; i < warningImages.Length; i++)
		{
			warningOriginalAlphas[i] = warningImages[i].color.a;
		}
		warningOriginalRawAlphas = new float[warningRawImages.Length];
		for (int j = 0; j < warningRawImages.Length; j++)
		{
			warningOriginalRawAlphas[j] = warningRawImages[j].color.a;
		}
		SetWarningAlpha(0f);
		Add(gamePanel);
	}

	private void Update()
	{
		PerFrameUpdate();
	}

	public virtual void PerFrameUpdate()
	{
		UpdateLoading();
		UpdateCrosshair();
		UpdateWarning();
		if (information)
		{
			UpdateInformation();
		}
		if (isTyping)
		{
			UpdateTyping();
		}
	}

	public void UpdateLoading()
	{
		if (!(loading == null))
		{
			loading.SetActive(client.proxyId < 0);
			if (loading.activeSelf)
			{
				Cursor.lockState = CursorLockMode.None;
			}
		}
	}

	public void ActivateMenu()
	{
		sensitivitySlider.value = Mathf.Clamp01((client.inputManager.cameraController.sensitivity - minSensitivity) / (maxSensitivity - minSensitivity));
		Add(basePanel);
		isActive = true;
		client.inputManager.menuOverride = true;
	}

	public void Pop()
	{
		List<GameObject> list = stack;
		list[list.Count - 1].SetActive(value: false);
		stack.RemoveAt(stack.Count - 1);
		if (stack.Count > 0)
		{
			List<GameObject> list2 = stack;
			list2[list2.Count - 1].SetActive(value: true);
		}
	}

	public void Add(GameObject menu)
	{
		if (stack.Count > 0)
		{
			List<GameObject> list = stack;
			list[list.Count - 1].SetActive(value: false);
		}
		stack.Add(menu);
		menu.SetActive(value: true);
	}

	public virtual void DisconnectButtonPressed()
	{
	}

	public void ResumeButtonPressed()
	{
		if (isTyping)
		{
			username = "";
			usernameText.text = username;
			isTyping = false;
		}
		Pop();
		isActive = false;
		client.inputManager.menuOverride = false;
	}

	public void MenuButtonPressed()
	{
		if (!isActive)
		{
			ActivateMenu();
		}
	}

	public void SettingsButtonPressed()
	{
		Add(settingsPanel);
	}

	public void BackButtonPressed()
	{
		if (isTyping)
		{
			username = "";
			usernameText.text = username;
			isTyping = false;
		}
		Pop();
	}

	public void WindowButtonPressed()
	{
		if (!Screen.fullScreen)
		{
			previousWidth = Screen.width;
			previousHeight = Screen.height;
			Screen.SetResolution(1920, 1080, fullscreen: true);
			windowText.text = "Windowed";
		}
		else
		{
			Screen.SetResolution(previousWidth, previousHeight, fullscreen: false);
			windowText.text = "Full-Screen";
		}
	}

	public void SensitivitySliderChanged()
	{
		if (stack.Count > 0)
		{
			client.inputManager.cameraController.sensitivity = Mathf.Lerp(minSensitivity, maxSensitivity, sensitivitySlider.value);
		}
	}

	public void EntityLimitSliderChanged()
	{
		if (stack.Count > 0)
		{
			client.preferences.entityCount.value = entityLimitSlider.normalizedValue;
			client.SendPreferences(client.preferences);
		}
	}

	public void PerspectiveButtonPressed()
	{
		client.inputManager.cameraController.TogglePerspective();
	}

	public void InformationButtonPressed()
	{
		information = !information;
		if (information)
		{
			informationPanel.SetActive(value: true);
		}
		else
		{
			informationPanel.SetActive(value: false);
		}
	}

	public void ChangeNamePressed()
	{
		username = "";
		usernameText.text = username;
		isTyping = true;
		if (inputType == EInput.MOBILE)
		{
			mobileKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.ASCIICapable, autocorrection: false, multiline: false, secure: false);
		}
	}

	public void VolumeSliderChanged()
	{
		soundSource.volume = volumeSlider.normalizedValue;
	}

	public void UpdateCrosshair()
	{
	}

	public void UpdateWarning()
	{
		if (client.idleTime > warningThreshold)
		{
			float num = 10f - client.idleTime;
			if (num < 0f)
			{
				num = 0f;
			}
			num = (float)Math.Round(num, 1);
			if (inputType == EInput.DESKTOP)
			{
				warningText.text = "WARNING: connection problem\r\nauto-disconnect in: " + num + " seconds";
			}
			else if (inputType == EInput.MOBILE)
			{
				warningText.text = num.ToString();
			}
			warning = true;
		}
		else
		{
			warning = false;
		}
		if (warning)
		{
			if (warningFadeLerp < 1f)
			{
				float num2 = 1f / warningFadeTime;
				warningFadeLerp += num2 * Time.deltaTime;
				if (warningFadeLerp > 1f)
				{
					warningFadeLerp = 1f;
				}
				SetWarningAlpha(warningFadeLerp);
			}
		}
		else if (warningFadeLerp > 0f)
		{
			float num3 = 1f / warningFadeTime;
			warningFadeLerp -= num3 * Time.deltaTime;
			if (warningFadeLerp < 0f)
			{
				warningFadeLerp = 0f;
			}
			SetWarningAlpha(warningFadeLerp);
		}
	}

	public void SetWarningAlpha(float alpha)
	{
		for (int i = 0; i < warningImages.Length; i++)
		{
			Image image = warningImages[i];
			image.color = new Color(image.color.r, image.color.g, image.color.b, warningOriginalAlphas[i] * alpha);
		}
		for (int j = 0; j < warningRawImages.Length; j++)
		{
			RawImage rawImage = warningRawImages[j];
			rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, warningOriginalRawAlphas[j] * alpha);
		}
		warningText.color = new Color(warningText.color.r, warningText.color.g, warningText.color.b, alpha);
	}

	public void UpdateInformation()
	{
		string text = "OFFLINE";
		if (client.proxyId >= 0)
		{
			text = "ONLINE";
		}
		int num = (int)client.statistics.fpsAverage.GetAverage();
		int num2 = (int)client.statistics.pingAverage.GetAverage();
		int count = client.entityManager.entities.Count;
		int count2 = client.players.Count;
		informationText1.text = text + "\nFPS " + num + "\nPING " + num2 + "\nPLAYERS " + count2 + "\nENTITIES " + count;
		float num3 = client.statistics.sentBytes * 8;
		string text2 = "bps";
		if (num3 >= 1000f)
		{
			num3 /= 1000f;
			text2 = "kbps";
			if (num3 >= 1000f)
			{
				num3 /= 1000f;
				text2 = "mbps";
			}
		}
		num3 = (float)Math.Round(num3, 1);
		float num4 = client.statistics.recievedBytes * 8;
		string text3 = "bps";
		if (num4 >= 1000f)
		{
			num4 /= 1000f;
			text3 = "kbps";
			if (num4 >= 1000f)
			{
				num4 /= 1000f;
				text3 = "mbps";
			}
		}
		num4 = (float)Math.Round(num4, 1);
		informationText2.text = "UP " + num3 + text2 + "\nDOWN " + num4 + text3;
	}

	public virtual void UpdateTyping()
	{
	}
}
