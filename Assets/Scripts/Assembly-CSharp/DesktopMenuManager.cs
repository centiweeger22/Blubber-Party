using UnityEngine;

public class DesktopMenuManager : MenuManager
{
	public override void Initialise()
	{
		base.Initialise();
		if (Screen.fullScreen)
		{
			windowText.text = "Windowed";
		}
		else
		{
			windowText.text = "Full-Screen";
		}
	}

	public override void PerFrameUpdate()
	{
		base.PerFrameUpdate();
		if (!isActive && Input.GetKeyDown(KeyCode.T))
		{
			client.inputManager.cameraController.TogglePerspective();
		}
	}

	public override void DisconnectButtonPressed()
	{
		client.SendDisconnect();
		Application.Quit();
	}

	public override void UpdateTyping()
	{
		for (int i = 97; i <= 122; i++)
		{
			if (Input.GetKeyDown((KeyCode)i))
			{
				username += MathExtension.ConvertByteToSimplifiedAlphabet((byte)(i - 97));
				usernameText.text = username;
				if (username.Length == 3)
				{
					isTyping = false;
					client.preferences.username = username;
					client.SendPreferences(client.preferences);
				}
			}
		}
	}
}
