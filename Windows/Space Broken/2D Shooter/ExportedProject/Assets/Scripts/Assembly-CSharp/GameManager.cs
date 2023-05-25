using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	[Tooltip("The UIManager component which manages the current scene's UI")]
	public UIManager uiManager;

	[Tooltip("The player gameobject")]
	public GameObject player;

	[Header("Scores")]
	[Tooltip("The player's score")]
	[SerializeField]
	private int gameManagerScore;

	[Tooltip("The highest score acheived on this device")]
	public int highScore;

	[Header("Game Progress / Victory Settings")]
	[Tooltip("Whether the game is winnable or not \nDefault: true")]
	public bool gameIsWinnable = true;

	[Tooltip("The number of enemies that must be defeated to win the game")]
	public int enemiesToDefeat = 10;

	private int enemiesDefeated;

	[Tooltip("Whether or not to print debug statements about whether the game can be won or not according to the game manager's search at start up")]
	public bool printDebugOfWinnableStatus = true;

	[Tooltip("Page index in the UIManager to go to on winning the game")]
	public int gameVictoryPageIndex;

	[Tooltip("The effect to create upon winning the game")]
	public GameObject victoryEffect;

	private int numberOfEnemiesFoundAtStart;

	[Header("Game Over Settings:")]
	[Tooltip("The index in the UI manager of the game over page")]
	public int gameOverPageIndex;

	[Tooltip("The game over effect to create when the game is lost")]
	public GameObject gameOverEffect;

	[HideInInspector]
	public bool gameIsOver;

	public static int score
	{
		get
		{
			return instance.gameManagerScore;
		}
		set
		{
			instance.gameManagerScore = value;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Object.DestroyImmediate(this);
		}
	}

	private void Start()
	{
		HandleStartUp();
	}

	private void HandleStartUp()
	{
		if (PlayerPrefs.HasKey("highscore"))
		{
			highScore = PlayerPrefs.GetInt("highscore");
		}
		if (PlayerPrefs.HasKey("score"))
		{
			score = PlayerPrefs.GetInt("score");
		}
		UpdateUIElements();
		if (printDebugOfWinnableStatus)
		{
			FigureOutHowManyEnemiesExist();
		}
	}

	private void FigureOutHowManyEnemiesExist()
	{
		List<EnemySpawner> list = Object.FindObjectsOfType<EnemySpawner>().ToList();
		List<Enemy> list2 = Object.FindObjectsOfType<Enemy>().ToList();
		int num = 0;
		int num2 = 0;
		int count = list2.Count;
		foreach (EnemySpawner item in list)
		{
			if (item.spawnInfinite)
			{
				num++;
			}
			else
			{
				num2 += item.maxSpawn;
			}
		}
		numberOfEnemiesFoundAtStart = num2 + count;
		if (gameIsWinnable)
		{
			if (num > 0)
			{
				Debug.Log("There are " + num + " infinite spawners  so the level will always be winnable, \nhowever you sshould still playtest for timely completion");
			}
			else if (enemiesToDefeat > numberOfEnemiesFoundAtStart)
			{
				Debug.LogWarning("There are " + enemiesToDefeat + " enemies to defeat but only " + numberOfEnemiesFoundAtStart + " enemies found at start \nThe level can not be completed!");
			}
			else
			{
				Debug.Log("There are " + enemiesToDefeat + " enemies to defeat and " + numberOfEnemiesFoundAtStart + " enemies found at start \nThe level can completed");
			}
		}
	}

	public void IncrementEnemiesDefeated()
	{
		enemiesDefeated++;
		if (enemiesDefeated >= enemiesToDefeat && gameIsWinnable)
		{
			LevelCleared();
		}
	}

	private void OnApplicationQuit()
	{
		SaveHighScore();
		ResetScore();
	}

	public static void AddScore(int scoreAmount)
	{
		score += scoreAmount;
		if (score > instance.highScore)
		{
			SaveHighScore();
		}
		UpdateUIElements();
	}

	public static void ResetScore()
	{
		PlayerPrefs.SetInt("score", 0);
		score = 0;
	}

	public static void SaveHighScore()
	{
		if (score > instance.highScore)
		{
			PlayerPrefs.SetInt("highscore", score);
			instance.highScore = score;
		}
		UpdateUIElements();
	}

	public static void ResetHighScore()
	{
		PlayerPrefs.SetInt("highscore", 0);
		if (instance != null)
		{
			instance.highScore = 0;
		}
		UpdateUIElements();
	}

	public static void UpdateUIElements()
	{
		if (instance != null && instance.uiManager != null)
		{
			instance.uiManager.UpdateUI();
		}
	}

	public void LevelCleared()
	{
		PlayerPrefs.SetInt("score", score);
		if (uiManager != null)
		{
			player.SetActive(value: false);
			uiManager.allowPause = false;
			uiManager.GoToPage(gameVictoryPageIndex);
			if (victoryEffect != null)
			{
				Object.Instantiate(victoryEffect, base.transform.position, base.transform.rotation, null);
			}
		}
	}

	public void GameOver()
	{
		gameIsOver = true;
		if (gameOverEffect != null)
		{
			Object.Instantiate(gameOverEffect, base.transform.position, base.transform.rotation, null);
		}
		if (uiManager != null)
		{
			uiManager.allowPause = false;
			uiManager.GoToPage(gameOverPageIndex);
		}
	}
}
