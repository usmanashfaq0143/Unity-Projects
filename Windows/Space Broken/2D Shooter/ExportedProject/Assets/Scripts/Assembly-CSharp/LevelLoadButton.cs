using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoadButton : MonoBehaviour
{
	public void LoadLevelByName(string levelToLoadName)
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(levelToLoadName);
	}
}
