using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenshotUtility : MonoBehaviour
{
	public static ScreenshotUtility screenShotUtility;

	[Header("Settings")]
	[Tooltip("Should the screenshot utility run only in the editor.")]
	public bool runOnlyInEditor = true;

	[Tooltip("What key is mapped to take the screenshot.")]
	public string m_ScreenshotKey = "c";

	[Tooltip("What is the scale factor for the screenshot. Standard is 1, 2x size is 2, etc..")]
	public int m_ScaleFactor = 1;

	[Tooltip("Include image size in filename.")]
	public bool includeImageSizeInFilename = true;

	[Header("Private Variables")]
	[Tooltip("Use the Reset Counter contextual menu item to reset this.")]
	[SerializeField]
	private int m_ImageCount;

	private const string ImageCntKey = "IMAGE_CNT";

	private void Awake()
	{
		if (screenShotUtility != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (runOnlyInEditor && !Application.isEditor)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		screenShotUtility = GetComponent<ScreenshotUtility>();
		Object.DontDestroyOnLoad(base.gameObject);
		m_ImageCount = PlayerPrefs.GetInt("IMAGE_CNT");
		if (!Directory.Exists("Screenshots"))
		{
			Directory.CreateDirectory("Screenshots");
		}
	}

	private void Update()
	{
		if (Keyboard.current.FindKeyOnCurrentKeyboardLayout(m_ScreenshotKey).wasPressedThisFrame)
		{
			TakeScreenshot();
		}
	}

	[ContextMenu("Reset Counter")]
	public void ResetCounter()
	{
		m_ImageCount = 0;
		PlayerPrefs.SetInt("IMAGE_CNT", m_ImageCount);
	}

	public void TakeScreenshot()
	{
		PlayerPrefs.SetInt("IMAGE_CNT", ++m_ImageCount);
		int num = Screen.width * m_ScaleFactor;
		int num2 = Screen.height * m_ScaleFactor;
		string text = "Screenshots/Screenshot_";
		if (includeImageSizeInFilename)
		{
			text = text + num + "x" + num2 + "_";
		}
		text = text + m_ImageCount + ".png";
		ScreenCapture.CaptureScreenshot(text, m_ScaleFactor);
		Debug.Log("Screenshot captured at " + text);
	}
}
