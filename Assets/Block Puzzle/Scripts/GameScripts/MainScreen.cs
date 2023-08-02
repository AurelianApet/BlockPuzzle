using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using System.Xml.Linq;
using System.Linq;

public class MainScreen : MonoBehaviour
{
	//int ClickedMode = 0;
	/// <summary>
	/// Start this instance.
	/// </summary>

	void Awake()
	{
		UM_AdManager.Init ();
		GoogleAnalytics.StartTracking ();
	}

	void Start ()
	{
		GameController.instance.PushWindow (gameObject);
		UM_AdManager.ShowBanner (UM_AdManager.CreateAdBanner (TextAnchor.LowerCenter));
	}

	void OnEnable()
	{
		Invoke ("EnableSettingsMenu", 0.1F);
		SessionManager.OnSessionChangedEvent += OnSessionChangedEvent;
	}

	void OnSessionChangedEvent (int sessionCount)
	{
		if ((sessionCount >= 2)) {
			UM_AdManager.StartInterstitialAd ();

		}
	}

	void OnDisable()
	{
		SessionManager.OnSessionChangedEvent -= OnSessionChangedEvent;
	}
		
	/// <summary>
	/// Raises the play classic button pressed event.
	/// </summary>
	public void OnPlayClassicButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			GamePlay.GamePlayMode = GameMode.classic;
			if (PlayerPrefs.GetInt ("Classic_playedBefore", 0) == 0) 
			{
				GameController.instance.SpawnUIScreen ("Classic_HelpIntro", true);
				PlayerPrefs.SetInt("Classic_playedBefore", 1);
			}
			else
			{
				GameController.instance.SpawnUIScreen ("GamePlay", true);
			}

			DisableSettingsMenu();
		}
	}

	public void OnPlayTimerButtonPressed ()
	{
//		if (InputManager.instance.canInput ()) {
//			AudioManager.instance.PlayButtonClickSound ();
//			GamePlay.GamePlayMode = GameMode.timer;
//			if (PlayerPrefs.GetInt ("Timer_playedBefore", 0) == 0) 
//			{
//				GameController.instance.SpawnUIScreen ("timerModeIntroScreen", true);
//				PlayerPrefs.SetInt("Timer_playedBefore", 1);
//			}
//			else
//			{
//				GameController.instance.SpawnUIScreen ("GamePlay", true);
//			}
//
//			DisableSettingsMenu();
//		}
	}

	public void OnPlayHexaButtonPressed ()
	{
//		if (InputManager.instance.canInput ()) {
//			AudioManager.instance.PlayButtonClickSound ();
//			GamePlay.GamePlayMode = GameMode.hexa;
//			GameObject Gameplay = GameController.instance.SpawnUIScreen ("GamePlay_hex", true);
//			Gameplay.name = "GamePlay";
//			DisableSettingsMenu();
//		}
	}

	/// <summary>
	/// Raises the play plus button pressed event.
	/// </summary>
	public void OnPlayPlusButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {

			if (PlayerPrefs.GetInt ("Plus_playedbefore", 0) == 0) {
				GameController.instance.SpawnUIScreen ("PlusModeIntroScreen", true);
				PlayerPrefs.SetInt("Plus_playedbefore", 1);
			}
			else {
				AudioManager.instance.PlayButtonClickSound ();
				GameController.instance.SpawnUIScreen ("GamePlay", true);
				GamePlay.GamePlayMode = GameMode.plus;
			}
			DisableSettingsMenu();
		}
	}

	/// <summary>
	/// Raises the play bomb pressed event.
	/// </summary>
	public void OnPlayBombPressed ()
	{
		if (InputManager.instance.canInput ()) {
			if (PlayerPrefs.GetInt ("Bomb_playedbefore", 0) == 0) {
				GameController.instance.SpawnUIScreen ("BombModeIntroScreen", true);
				PlayerPrefs.SetInt ("Bomb_playedbefore", 1);
			} else {
				AudioManager.instance.PlayButtonClickSound ();
				GameController.instance.SpawnUIScreen ("GamePlay", true);
				GamePlay.GamePlayMode = GameMode.bomb;
			}
		}

		DisableSettingsMenu ();
	}

	void EnableSettingsMenu()
	{
		if (SettingsContent.instance.settings_main != null) {
			SettingsContent.instance.settings_main.gameObject.SetActive(true);
			SettingsContent.instance.transform.SetAsLastSibling();
		}
	}

	void DisableSettingsMenu() {
		if (SettingsContent.instance.settings_main != null) {
			SettingsContent.instance.settings_main.gameObject.SetActive(false);
		}
	}
}