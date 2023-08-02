using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
	public Text txtScore;
	public Text txtBestScore;

	GameMode mode;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		GameController.instance.PushWindow (gameObject);
		Invoke ("ShowInterstial", 1F);
	}

	public void ShowInterstial()
	{
		//if (int.Parse (txtScore.ToString()) >= 500) 
		{
			UM_AdManager.ShowInterstitialAd ();
		}
	}
	
	/// <summary>
	/// Set the score and best score.
	/// </summary>
	/// <param name="GamePlayMode">Game play mode.</param>
	/// <param name="score">Score.</param>
	/// <param name="bestScore">Best score.</param>
	public void setScore (GameMode GamePlayMode, int score, int bestScore)
	{
		mode = GamePlayMode;
		txtScore.text = score.ToString ();
		txtBestScore.text = "Best : " + bestScore.ToString ();

		if (score > 100000) {
			UM_GameServiceManager.instance.UnlockAchievement ("com.blockpuzzle.100000points");
		} else if (score > 80000) {
			UM_GameServiceManager.instance.UnlockAchievement ("com.blockpuzzle.80000points");
		} else if (score > 50000) {
			UM_GameServiceManager.instance.UnlockAchievement ("com.blockpuzzle.50000points");
		} else if (score > 20000) {
			UM_GameServiceManager.instance.UnlockAchievement ("com.blockpuzzle.20000points");
		} else if (score > 10000) {
			UM_GameServiceManager.instance.UnlockAchievement ("com.blockpuzzle.10000points");
		}

		switch (GamePlayMode) {
		case GameMode.classic:
			UM_GameServiceManager.Instance.SubmitScore ("com.blockpuzzle.classicmode", bestScore);
			break;
		case GameMode.plus:
			UM_GameServiceManager.Instance.SubmitScore ("com.blockpuzzle.plusmode", bestScore);
			break;
		case GameMode.bomb:
			UM_GameServiceManager.Instance.SubmitScore ("com.blockpuzzle.bombmode", bestScore);
			break;
		}
	}

	/// <summary>
	/// This will navigate to home screen.
	/// </summary>
	public void OnHomeButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			GameController.instance.OnCloseButtonPressed ();
			Destroy (GameController.instance.WindowStack.Pop ());
			GameController.instance.SpawnUIScreen ("MainScreen", true);

			Resources.UnloadUnusedAssets ();
			System.GC.Collect (0, System.GCCollectionMode.Forced);
		}
	}

	/// <summary>
	/// Put code here to sharing game, score etc on social network.
	/// </summary>
	public void OnShareButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			Debug.Log ("Share stuff goes here..");
		}
	}

	/// <summary>
	/// Put your code here to open the leaderboard.
	/// </summary>
	public void OnLeaderboardButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			Debug.Log ("Leaderbord open stuff goes here..");

			UM_GameServiceManager.Instance.ShowLeaderBoardsUI ();

//			switch (mode) {
//			case GameMode.classic:
//				UM_GameServiceManager.Instance.ShowLeaderBoardsUI ("com.blockpuzzle.classicmode");
//				break;
//			case GameMode.plus:
//				UM_GameServiceManager.Instance.ShowLeaderBoardUI ("com.blockpuzzle.plusmode");
//				break;
//			case GameMode.bomb:
//				UM_GameServiceManager.Instance.ShowLeaderBoardUI ("com.blockpuzzle.bombmode");
//				break;
//			}
		}
	}

	/// <summary>
	/// This will navigate to home screen.
	/// </summary>
	public void OnCloseButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			GameController.instance.OnCloseButtonPressed ();

			Resources.UnloadUnusedAssets ();
			System.GC.Collect (0, System.GCCollectionMode.Forced);
		}
	}

	/// <summary>
	/// Will restart the game.
	/// </summary>
	public void OnRetryButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			BlockTrayManager.instance.ResetGame ();
			GameController.instance.RestartGamePlay ();

			Resources.UnloadUnusedAssets ();
			System.GC.Collect (0, System.GCCollectionMode.Forced);
		}
	}
}
