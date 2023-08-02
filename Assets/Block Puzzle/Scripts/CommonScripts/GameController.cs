using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Xml.Linq;
using System.Linq;

/// <summary>
/// This script is center of enire project and manages all the navigation flow.
/// </summary>
public class GameController : MonoBehaviour
{
	public static GameController instance;
	public Canvas UICanvas;

	/// <summary>
	/// The game document.
	/// GameDoc :- xml for BlockDetails if Left Game InBetween Gaameplay...
	/// </summary>
	public XDocument GameDoc;
	/// <summary>
	/// This stack manages all the screen. any screen on the screen is pused and removing screen will be popped.
	/// You cab always ask for the help if you're having trouble in changing flow.
	/// </summary>
	public Stack<GameObject> WindowStack = new Stack<GameObject> ();

	/// <summary>
	/// isHelpRunning Is To mention that help is running on screen or not
	/// isHelpRunning = 0 : Means No Help isRunning
	/// isHelpRunning = 1 : Means Help isRunning for Mode Classic
	/// isHelpRunning = 2 : Means Help isRunning for Mode Bomb
	/// isHelpRunning = 3 : Means Help isRunning for Mode Plus
	/// </summary>
	public int isHelpRunning = 0;

	/// <summary>
	/// Wheather to apply data from which game was left or not
	/// </summary>
	public bool PlayFromLastStatus = false;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake ()
	{
		if (instance == null) {
			instance = this;
			return;
		}
		Destroy (gameObject);
	}

	public void OnEnable()
	{
		GameDoc = new XDocument ();
		GameDoc.Declaration = new XDeclaration ("1.0","UTF-16","no");
		XElement resources = new XElement ("resources");
		XElement totalScore = new XElement ("totalScore", new XAttribute ("score", ""));
		XElement timerValue = new XElement ("timerValue", new XAttribute ("time", ""));
		XElement currentMode = new XElement ("currentMode", new XAttribute ("modeId", ""));
		XElement suggestedObject1 = new XElement ("suggestedObject1", new XAttribute ("objectName", ""));
		XElement suggestedObject2 = new XElement ("suggestedObject2", new XAttribute ("objectName", ""));
		XElement suggestedObject3 = new XElement ("suggestedObject3", new XAttribute ("objectName", ""));
		resources.Add (totalScore);
		resources.Add (timerValue);
		resources.Add (currentMode);
		resources.Add (suggestedObject1);
		resources.Add (suggestedObject2);
		resources.Add (suggestedObject3);
		GameDoc.Add (resources);
	}

	public void ResetGameDoc()
	{
		GameDoc = new XDocument ();
		GameDoc.Declaration = new XDeclaration ("1.0","UTF-16","no");
		XElement resources = new XElement ("resources");
		XElement totalScore = new XElement ("totalScore", new XAttribute ("score", ""));
		XElement timerValue = new XElement ("timerValue", new XAttribute ("time", ""));
		XElement currentMode = new XElement ("currentMode", new XAttribute ("modeId", ""));
		XElement suggestedObject1 = new XElement ("suggestedObject1", new XAttribute ("objectName", ""));
		XElement suggestedObject2 = new XElement ("suggestedObject2", new XAttribute ("objectName", ""));
		XElement suggestedObject3 = new XElement ("suggestedObject3", new XAttribute ("objectName", ""));
		resources.Add (totalScore);
		resources.Add (timerValue);
		resources.Add (currentMode);
		resources.Add (suggestedObject1);
		resources.Add (suggestedObject2);
		resources.Add (suggestedObject3);
		GameDoc.Add (resources);

		PlayerPrefs.DeleteKey ("GameData");
	}

	void Start()
	{
		Invoke ("CheckForLastStatus",0.2f);
		UM_GameServiceManager.instance.Connect ();
		Application.targetFrameRate = 60;
	}

	void CheckForLastStatus()
	{
		if (PlayerPrefs.GetString ("GameData", string.Empty) != string.Empty) 
		{
			GameDoc = XDocument.Parse (PlayerPrefs.GetString ("GameData", string.Empty));
			XElement root = GameController.instance.GameDoc.Root;
			if (root.Elements ("block").Count () > 0) {
				GameController.instance.PlayFromLastStatus = true;
				GameMode mode = (GameMode)(int.Parse (GameController.instance.GameDoc.Root.Element ("currentMode").Attribute ("modeId").Value));
				if (mode == GameMode.classic) {
					GamePlay.GamePlayMode = GameMode.classic;
				} else if (mode == GameMode.bomb) {
					GamePlay.GamePlayMode = GameMode.bomb;
				} else if (mode == GameMode.plus) {
					GamePlay.GamePlayMode = GameMode.plus;
				}

//				else if (mode == GameMode.hexa) {
//					GamePlay.GamePlayMode = GameMode.hexa;
//				} else {
//					GamePlay.GamePlayMode = GameMode.timer;
//				}				

				//if (GamePlay.GamePlayMode == GameMode.hexa) {
				//	GameObject Gameplay = GameController.instance.SpawnUIScreen ("GamePlay_hex", true);
				//	Gameplay.name = "GamePlay";
				//} else 
				
				{
					GameObject Gameplay = GameController.instance.SpawnUIScreen ("GamePlay", true);
				}
			}

			SettingsContent.instance.settings_main.gameObject.SetActive (false);

			Debug.Log (GameDoc.ToString ());
		} 
		else {
			PlayerPrefs.DeleteKey ("GameData");
		}
	}

	/// <summary>
	/// Creates the bomb element in Xml.
	/// </summary>
	/// <returns>The bomb element.</returns>
	/// <param name="rowId">Row identifier.</param>
	/// <param name="columnId">Column identifier.</param>
	/// <param name="bomb">Bomb Counter.</param>
	public XElement createBombElement(int rowId,int columnId,int bomb)
	{
		XElement bombelement = (from e in GameController.instance.GameDoc.Root.Elements ("bomb")
			where (e.Attribute ("row").Value == rowId.ToString () && e.Attribute ("col").Value == columnId.ToString ())
			select e).FirstOrDefault ();

		if (bombelement == null) 
		{
			bombelement = new XElement ("bomb");
			XAttribute row1 = new XAttribute ("row",rowId.ToString());
			XAttribute col1 = new XAttribute ("col",columnId.ToString());
			XAttribute number1 = new XAttribute ("number",bomb.ToString());
			bombelement.Add (row1);
			bombelement.Add (col1);
			bombelement.Add (number1);
			GameDoc.Root.Add (bombelement);	
		} else {
			bombelement.Attribute ("number").SetValue (bomb.ToString ());
		}

		return bombelement;
	}

	/// <summary>
	/// Creates the block element.
	/// </summary>
	/// <returns>The block element.</returns>
	/// <param name="rowId">Row identifier.</param>
	/// <param name="columnId">Column identifier.</param>
	/// <param name="blockColor">Block color.</param>
	public XElement createBlockElement(int rowId,int columnId,Color blockColor)
	{
		XElement element = (from e in GameController.instance.GameDoc.Root.Elements ("block")
			where (e.Attribute ("row").Value == rowId.ToString () && e.Attribute ("col").Value == columnId.ToString ())
			select e).FirstOrDefault ();
		
		if (element == null) {
			element = new XElement ("block");
			XAttribute row = new XAttribute ("row", rowId.ToString ());
			XAttribute col = new XAttribute ("col", columnId.ToString ());
			element.Add (row);
			element.Add (col);

			XElement colorelement = new XElement ("color");
			XAttribute r = new XAttribute ("r", blockColor.r.ToString ());
			XAttribute g = new XAttribute ("g", blockColor.g.ToString ());
			XAttribute b = new XAttribute ("b", blockColor.b.ToString ());
			colorelement.Add (r);
			colorelement.Add (g);
			colorelement.Add (b);

			element.Add (colorelement);

			GameDoc.Root.Add (element);
		} else {
			XElement colorelement = element.Element("color");
			colorelement.Attribute("r").SetValue(blockColor.r.ToString ());
			colorelement.Attribute("g").SetValue(blockColor.g.ToString ());
			colorelement.Attribute("b").SetValue(blockColor.b.ToString ());
		}
		return element;
	}

	/// <summary>
	/// Removes the bomb node From Xml.
	/// </summary>
	/// <param name="rowId">Row identifier.</param>
	/// <param name="columnId">Column identifier.</param>
	public void removeBombNode(int rowId,int columnId)
	{
		GameController.instance.GameDoc.Root.Elements ("bomb").Where (e=>  e.Attribute ("row").Value == rowId.ToString () && e.Attribute ("col").Value == columnId.ToString ()).Remove();
	}
	 
//	if game goes in background the gamedata will be saved if game is suspended from background
	void OnApplicationFocus(bool Focus)
	{
		if (!Focus && WindowStack.Count > 0 ) {
			GameObject PeekWiondow = WindowStack.Peek ();
			if (GamePlay.instance != null && (PeekWiondow != null && (PeekWiondow.name == "GamePlay" || PeekWiondow.name == "GamePlay_hex" || PeekWiondow.name == "Settings-Screen-GamePlay" || PeekWiondow.name == "Quit-Confirm-Play"))) {
				GameDoc.Root.Element ("totalScore").Attribute ("score").SetValue (GamePlay.instance.Score.ToString ());
				GameDoc.Root.Element ("currentMode").Attribute ("modeId").SetValue (((int)GamePlay.GamePlayMode).ToString ());
				float percentageValue = (GamePlay.instance.Timer.sizeDelta.x * 100) / 555f;
				int timer = (int)(percentageValue * 120) / 100;
				GameDoc.Root.Element ("timerValue").Attribute ("time").SetValue (timer.ToString ());
				
				PlayerPrefs.SetString ("GameData", GameDoc.ToString ());
			} else {
				PlayerPrefs.SetString ("GameData",string.Empty);
			}
		}
	}


	/// <summary>
	/// Spawns the prefab from resources.
	/// </summary>
	/// <returns>The prefab from resources.</returns>
	/// <param name="path">Path.</param>
	public GameObject SpawnPrefabFromResources (string path)
	{
		GameObject thisObject = (GameObject)Instantiate (Resources.Load (path));
		thisObject.name = thisObject.name.Replace ("(Clone)", "");
		return thisObject;
	}

	/// <summary>
	/// Spawns the user interface screen.
	/// </summary>
	/// <returns>The user interface screen.</returns>
	/// <param name="name">Name.</param>
	/// <param name="doAddToStack">If set to <c>true</c> do add to stack.</param>
	public GameObject SpawnUIScreen (string name, bool doAddToStack = true)
	{
		if (name == "GamePlay" || name == "GamePlay_help" || name == "GamePlay_hex") {
			if(WindowStack.Count > 0) {
				Destroy (WindowStack.Pop ());
			}
		}
		GameObject thisScreen = (GameObject)Instantiate (Resources.Load ("UIScreens/" + name.ToString ()));
		thisScreen.name = name;
		thisScreen.transform.SetParent (UICanvas.transform);
		thisScreen.transform.localPosition = Vector3.zero;
		thisScreen.transform.localScale = Vector3.one;
		thisScreen.GetComponent<RectTransform> ().sizeDelta = Vector3.zero;
		thisScreen.Init ();
		thisScreen.OnWindowLoad ();
		thisScreen.SetActive (true);

		if (doAddToStack) {
			WindowStack.Push (thisScreen);
		}
		return thisScreen;
	}

	/// <summary>
	/// Spawns the prefab.
	/// </summary>
	/// <returns>The prefab.</returns>
	/// <param name="name">Name.</param>
	/// <param name="doAddToStack">If set to <c>true</c> do add to stack.</param>
	public GameObject SpawnPrefab (string name, bool doAddToStack = false)
	{
		GameObject thisScreen = (GameObject)Instantiate (Resources.Load ("Prefabs/GamePlay/" + name.ToString ()));
		if (doAddToStack) {
			WindowStack.Push (thisScreen);
		}
		return thisScreen;
	}

	/// <summary>
	/// Pushes the window to stack when it is spawed.
	/// </summary>
	/// <param name="window">Window.</param>
	public void PushWindow (GameObject window)
	{
		if (!WindowStack.Contains (window)) {
			WindowStack.Push (window);
		}
	}

	/// <summary>
	/// Pops the window when it is removed.
	/// </summary>
	/// <returns>The window.</returns>
	public GameObject PopWindow ()
	{
		if (WindowStack.Count > 0) {
			Debug.LogError (WindowStack.Peek ().name + "  pop");
			return WindowStack.Pop ();
		}
		return null;
	}

	/// <summary>
	/// Peeks the last entered windows from the stack.
	/// </summary>
	/// <returns>The window.</returns>
	public GameObject PeekWindow ()
	{
		if (WindowStack.Count > 0) {
			return WindowStack.Peek ();
		}
		return null;
	}

	/// <summary>
	/// Raises the back button pressed event.
	/// </summary>
	public void OnBackButtonPressed ()
	{
		if (WindowStack != null && WindowStack.Count > 0) {
			GameObject currentWindow = WindowStack.Peek ();
	
			///if back button pressed from main screen, it will ask for quit-confirm.
			if (currentWindow.name == "MainScreen") {
				GameController.instance.SpawnUIScreen ("Quit-Confirm-Game", true);
				return;
			} 

			/// if back button pressed during gameplay, this will ask for confirmation to quit the play.
			else if (currentWindow.name == "GamePlay") {
				GameController.instance.SpawnUIScreen ("Quit-Confirm-Play", true);
				return;
			}

			///if Game Over screen is opened and back/close/home button is pressed, it will navigate to main screen.
			else if (currentWindow.name == "GameOver") {
				if (currentWindow.OnWindowRemove () == false) {
					Destroy (currentWindow);
				}
				WindowStack.Pop ();
				Destroy (GameController.instance.WindowStack.Pop ());
				SpawnUIScreen("MainScreen",true);
				return;
			}

			/// if setting screen is opened, pressing back button or close button will force screen to close.
			else if (currentWindow.name == "Settings-Screen-Main" || currentWindow.name == "Settings-Screen-GamePlay") {
				currentWindow.GetComponent<Settings> ().CloseMenu ();
			} 

			/// if any other screen mentioned above is opened and back button is pressed, this will lead to close that screen only.
			else {
				if (currentWindow.OnWindowRemove () == false) {
					Destroy (currentWindow);
				}
			}
			WindowStack.Pop ();
		} 

		InputManager.instance.DisableTouchForDelay ();
	}

	/// <summary>
	/// Restarts the game play.
	/// This is an adjustment made where only game
	/// </summary>
	public void RestartGamePlay()
	{
		GameObject currentWindow = WindowStack.Peek ();
		if (currentWindow != null) {
			if (currentWindow.name == "GameOver") {
				if (currentWindow.OnWindowRemove () == false) {
					Destroy (currentWindow);
				}
			}
			WindowStack.Pop ();
		}
	}

	/// <summary>
	/// Raises the close button pressed event.
	/// </summary>
	public void OnCloseButtonPressed ()
	{
		OnBackButtonPressed ();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
		///Detects the back button press event.
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (InputManager.instance.canInput ()) {
				OnBackButtonPressed ();
			}
		}
	}
}