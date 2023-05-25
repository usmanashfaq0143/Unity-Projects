using UnityEngine;
using UnityEngine.UI;

public class HighScoreDisplay : UIelement
{
	[Tooltip("The text UI to use for display")]
	public Text displayText;

	public void DisplayHighScore()
	{
		if (displayText != null)
		{
			displayText.text = "High: " + GameManager.instance.highScore;
		}
	}

	public override void UpdateUI()
	{
		base.UpdateUI();
		DisplayHighScore();
	}
}
