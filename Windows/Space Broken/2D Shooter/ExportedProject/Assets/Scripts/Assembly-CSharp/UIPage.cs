using UnityEngine;

public class UIPage : MonoBehaviour
{
	[Tooltip("The default UI to have selected when opening this page")]
	public GameObject defaultSelected;

	public void SetSelectedUIToDefault()
	{
		if (defaultSelected != null)
		{
			GameManager.instance.uiManager.eventSystem.SetSelectedGameObject(null);
			GameManager.instance.uiManager.eventSystem.SetSelectedGameObject(defaultSelected);
		}
	}
}
