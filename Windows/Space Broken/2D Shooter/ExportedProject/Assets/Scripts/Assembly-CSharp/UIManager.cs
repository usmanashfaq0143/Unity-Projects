using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
	[Header("Page Management")]
	[Tooltip("The pages (Panels) managed by the UI Manager")]
	public List<UIPage> pages;

	[Tooltip("The index of the active page in the UI")]
	public int currentPage;

	[Tooltip("The page (by index) switched to when the UI Manager starts up")]
	public int defaultPage;

	[Header("Pause Settings")]
	[Tooltip("The index of the pause page in the pages list")]
	public int pausePageIndex = 1;

	[Tooltip("Whether or not to allow pausing")]
	public bool allowPause = true;

	private bool isPaused;

	private List<UIelement> UIelements;

	[HideInInspector]
	public EventSystem eventSystem;

	[SerializeField]
	private InputManager inputManager;

	private void OnEnable()
	{
		SetupGameManagerUIManager();
	}

	private void SetupGameManagerUIManager()
	{
		if (GameManager.instance != null && GameManager.instance.uiManager == null)
		{
			GameManager.instance.uiManager = this;
		}
	}

	private void SetUpUIElements()
	{
		UIelements = Object.FindObjectsOfType<UIelement>().ToList();
	}

	private void SetUpEventSystem()
	{
		eventSystem = Object.FindObjectOfType<EventSystem>();
		if (eventSystem == null)
		{
			Debug.LogWarning("There is no event system in the scene but you are trying to use the UIManager. /nAll UI in Unity requires an Event System to run. /nYou can add one by right clicking in hierarchy then selecting UI->EventSystem.");
		}
	}

	private void SetUpInputManager()
	{
		if (inputManager == null)
		{
			inputManager = InputManager.instance;
		}
		if (inputManager == null)
		{
			Debug.LogWarning("The UIManager can not find an Input Manager in the scene, without an Input Manager the UI can not pause");
		}
	}

	public void TogglePause()
	{
		if (allowPause)
		{
			if (isPaused)
			{
				SetActiveAllPages(activated: false);
				Time.timeScale = 1f;
				isPaused = false;
			}
			else
			{
				GoToPage(pausePageIndex);
				Time.timeScale = 0f;
				isPaused = true;
			}
		}
	}

	public void UpdateUI()
	{
		SetUpUIElements();
		foreach (UIelement uIelement in UIelements)
		{
			uIelement.UpdateUI();
		}
	}

	private void Start()
	{
		SetUpInputManager();
		SetUpEventSystem();
		SetUpUIElements();
		UpdateUI();
	}

	private void Update()
	{
		CheckPauseInput();
	}

	private void CheckPauseInput()
	{
		if (inputManager != null && inputManager.pausePressed)
		{
			TogglePause();
		}
	}

	public void GoToPage(int pageIndex)
	{
		if (pageIndex < pages.Count && pages[pageIndex] != null)
		{
			SetActiveAllPages(activated: false);
			pages[pageIndex].gameObject.SetActive(value: true);
			pages[pageIndex].SetSelectedUIToDefault();
		}
	}

	public void GoToPageByName(string pageName)
	{
		UIPage item2 = pages.Find((UIPage item) => item.name == pageName);
		int pageIndex = pages.IndexOf(item2);
		GoToPage(pageIndex);
	}

	public void SetActiveAllPages(bool activated)
	{
		if (pages == null)
		{
			return;
		}
		foreach (UIPage page in pages)
		{
			if (page != null)
			{
				page.gameObject.SetActive(activated);
			}
		}
	}
}
