using UnityEngine;

public class Button
{
	public KeyCode keyCode;

	public EButtonState state;

	public bool changeDetected;

	public bool requireRelease;

	public Button(KeyCode _keyCode)
	{
		keyCode = _keyCode;
		state = EButtonState.RELEASED;
	}

	public void Poll(bool menuOverride, bool fuzz = false)
	{
		if (changeDetected)
		{
			return;
		}
		bool flag = Input.GetKey(keyCode);
		if (fuzz)
		{
			flag = Random.Range(0, 2) == 1;
		}
		if (flag && !menuOverride && !requireRelease)
		{
			if (state == EButtonState.ON_RELEASE)
			{
				changeDetected = true;
				state = EButtonState.RELEASED;
			}
			else if (state == EButtonState.RELEASED)
			{
				changeDetected = true;
				state = EButtonState.ON_PRESS;
			}
			else if (state == EButtonState.ON_PRESS)
			{
				changeDetected = true;
				state = EButtonState.PRESSED;
			}
			return;
		}
		if (!flag)
		{
			requireRelease = false;
		}
		if (state == EButtonState.ON_PRESS)
		{
			changeDetected = true;
			state = EButtonState.PRESSED;
		}
		else if (state == EButtonState.PRESSED)
		{
			changeDetected = true;
			state = EButtonState.ON_RELEASE;
		}
		else if (state == EButtonState.ON_RELEASE)
		{
			changeDetected = true;
			state = EButtonState.RELEASED;
		}
	}

	public void PollAutomatic(bool automatedState, bool menuOverride, bool fuzz = false)
	{
		if (changeDetected)
		{
			return;
		}
		if (fuzz)
		{
			automatedState = Random.Range(0, 2) == 1;
		}
		if (automatedState && !menuOverride && !requireRelease)
		{
			if (state == EButtonState.ON_RELEASE)
			{
				changeDetected = true;
				state = EButtonState.RELEASED;
			}
			else if (state == EButtonState.RELEASED)
			{
				changeDetected = true;
				state = EButtonState.ON_PRESS;
			}
			else if (state == EButtonState.ON_PRESS)
			{
				changeDetected = true;
				state = EButtonState.PRESSED;
			}
			return;
		}
		if (!automatedState)
		{
			requireRelease = false;
		}
		if (state == EButtonState.ON_PRESS)
		{
			changeDetected = true;
			state = EButtonState.PRESSED;
		}
		else if (state == EButtonState.PRESSED)
		{
			changeDetected = true;
			state = EButtonState.ON_RELEASE;
		}
		else if (state == EButtonState.ON_RELEASE)
		{
			changeDetected = true;
			state = EButtonState.RELEASED;
		}
	}

	public void Reset()
	{
		changeDetected = false;
	}

	public Button Clone()
	{
		return new Button(keyCode)
		{
			state = state,
			changeDetected = changeDetected
		};
	}
}
