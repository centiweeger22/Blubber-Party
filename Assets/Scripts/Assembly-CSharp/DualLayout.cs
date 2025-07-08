using UnityEngine;

public class DualLayout : MonoBehaviour
{
	public RectTransform rectTransform;

	public RectTransform verticalTransform;

	public RectTransform horizontalTransform;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetVerticalLayout()
	{
		rectTransform.anchorMin = verticalTransform.anchorMin;
		rectTransform.anchorMax = verticalTransform.anchorMax;
		rectTransform.pivot = verticalTransform.pivot;
		rectTransform.position = verticalTransform.position;
	}

	public void SetHorizontalLayout()
	{
		rectTransform.anchorMin = horizontalTransform.anchorMin;
		rectTransform.anchorMax = horizontalTransform.anchorMax;
		rectTransform.pivot = horizontalTransform.pivot;
		rectTransform.position = horizontalTransform.position;
	}
}
