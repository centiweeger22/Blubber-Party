using System.Collections.Generic;

public class ClientInfo
{
	public int ticket = -1;

	public Preferences preferences;

	public PlayerEntity proxy;

	public List<InputSample> unprocessedInputs;

	public ClientInfo()
	{
		preferences = new Preferences();
		unprocessedInputs = new List<InputSample>();
	}

	public void SetNextInput()
	{
		int num = unprocessedInputs.Count;
		if (num == 0)
		{
			InputSample inputSample = new InputSample();
			inputSample.Initialise();
			inputSample.timestamp = -1;
			proxy.input = inputSample;
			return;
		}
		if (num > 1)
		{
			InputSample inputSample2 = unprocessedInputs[num - 1];
			int num2;
			for (num2 = 0; num2 < num - 1; num2++)
			{
				InputSample inputSample3 = unprocessedInputs[num2];
				if (inputSample3.jump.state == EButtonState.ON_PRESS)
				{
					inputSample2.jump.state = EButtonState.ON_PRESS;
				}
				if (inputSample3.fire.state == EButtonState.ON_PRESS)
				{
					inputSample2.fire.state = EButtonState.ON_PRESS;
				}
				unprocessedInputs.RemoveAt(num2);
				num2--;
				num--;
			}
		}
		InputSample input = unprocessedInputs[0];
		unprocessedInputs.RemoveAt(0);
		proxy.input = input;
	}
}
