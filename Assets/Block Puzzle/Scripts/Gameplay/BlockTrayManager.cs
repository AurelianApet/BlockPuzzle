using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml.Linq;

/// <summary>
/// Block tray manager.
/// This component have all the blocks that will be used during gameplay, it will spawn random blocks based on the probability of the blocks.
/// </summary>
public class BlockTrayManager : MonoBehaviour
{
	public static BlockTrayManager instance;
	public Transform blockContainer;
	public List<Transform> blockList;
	List<int> ProbabilityPool = new List<int> ();
	float blockTransitionTime = 0.5F;
	//bool verticalHelp_classicMode = false;


	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake ()
	{
		if (instance == null) {
			instance = this;
		}
	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		FillProbabilityPool ();
		startGame ();
	}

	/// <summary>
	/// Starts the game.
	/// </summary>
	public void startGame ()
	{
		if (GameController.instance.isHelpRunning == 0)
		{
			OnPlacingBlock ();
		}
	}

	/// <summary>
	/// Resets the game For Replay level.
	/// </summary>
	public void ResetGame ()
	{
		ProbabilityPool.Clear ();
		FillProbabilityPool ();

		GameController.instance.GameDoc = new XDocument ();
		GameController.instance.GameDoc.Declaration = new XDeclaration ("1.0","UTF-16","no");
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
		GameController.instance.GameDoc.Add (resources);

		BlockManager.instance.ReInitializeBlocks ();
		foreach (Transform block in blockContainer) {
			if (block.childCount > 0) {
				Destroy (block.GetChild (0).gameObject);
			}
		}	

		GamePlay.instance.txtScore.text = "0";
		GamePlay.instance.Score = 0;
		GamePlay.instance.TotalMoves = 0;
		GamePlay.instance.TimerValue = GamePlay.instance.TotalTimerValue;
		foreach(BombModedetails b in GamePlay.instance.bombPlacingDetails)
		{
			BlockData objblock = BlockManager.instance.BlockList.Find (o => o.rowId == b.block.rowId && o.columnId == b.block.columnId);
			if (objblock != null) {
				if (objblock.block.transform.childCount > 0) {
					objblock.block.sprite = GamePlay.instance.blockImage;
					objblock.block.color = GamePlay.instance.blockColor;
					Destroy (objblock.block.transform.GetChild (0).gameObject);
				}
			}
		}
		GamePlay.instance.bombPlacingDetails.Clear ();

		Invoke ("OnPlacingBlock", 0.5f);

//		if (GamePlay.GamePlayMode == GameMode.timer) {
//			Debug.LogError ("Inside game reset");
//			GamePlay.instance.Timer.sizeDelta = new Vector2 (555f, GamePlay.instance.Timer.sizeDelta.y);
//			GamePlay.instance.SetTimer ();
//		}
	}

	/// <summary>
	/// Fills the probability pool. Calculates the probability of the all blocks.
	/// </summary>
	void FillProbabilityPool ()
	{
//		if (GamePlay.GamePlayMode == GameMode.hexa) {
//			for (int i = 0; i < blockList.Count; i++) {
//				for (int index = 0; index < blockList [i].GetComponent<Block> ().blockProbability; index++) {
//					ProbabilityPool.Add (i);
//				}
//			}
//		} else 
		
		{
			for (int i = 0; i < (GameController.instance.isHelpRunning == 0 ? (GamePlay.GamePlayMode == GameMode.plus ? blockList.Count : 19) : blockList.Count); i++) {
				for (int index = 0; index < blockList [i].GetComponent<Block> ().blockProbability; index++) {
					ProbabilityPool.Add (i);
				}
			}
		}

		ShuffleGenericList (ProbabilityPool);
	}

	/// <summary>
	/// Swaps the object.
	/// </summary>
	/// <param name="parentBlock">Parent block.</param>
	/// <param name="blockToTansit">Block to tansit.</param>
	public void swapObject (Transform parentBlock, Transform blockToTansit)
	{
		blockToTansit.SetParent (parentBlock);
		EGTween.MoveTo (blockToTansit.gameObject, EGTween.Hash ("isLocal", true, "x", 0, "time", blockTransitionTime));
	}

	/// <summary>
	/// Raises the placing block event.
	/// </summary>
	public void OnPlacingBlock ()
	{
		int blockRemained = 0;
		foreach (Transform t in blockContainer) {
			if (t.childCount > 0) {
				blockRemained++;
			}
		}	
		if (blockRemained == 2) {
			for (int i = 1; i <= 3; i++) {
				if (blockContainer.GetChild (i - 1).childCount <= 0) {
					if (i == 1) {
						if (blockContainer.GetChild (i).childCount > 0) {
							swapObject (blockContainer.GetChild (i - 1), blockContainer.GetChild (i).GetChild (0));
							swapObject (blockContainer.GetChild (i), blockContainer.GetChild (i + 1).GetChild (0));
						}
					} else if (i == 2) {
						if (blockContainer.GetChild (i).childCount >= 0) {
							swapObject (blockContainer.GetChild (i - 1), blockContainer.GetChild (i).GetChild (0));
						}
					} else {
						GameObject obj = (GameObject)Instantiate (blockList [ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)]].gameObject);
						obj.transform.SetParent (blockContainer.GetChild (i - 1).transform);
						obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
						obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
						obj.gameObject.SetActive (true);
						EGTween.MoveFrom (obj, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
					}
				}
			}
		} else if (blockRemained == 0) {
			for (int i = 0; i < 3; i++) {
				GameObject obj = null;
				if (GameController.instance.PlayFromLastStatus) {
					string ObjectName = GameController.instance.GameDoc.Root.Element("suggestedObject"+(i+1)).Attribute("objectName").Value;
					ObjectName = ObjectName.Replace ("(Clone)", "");
					int index = blockList.FindIndex (o => o.name == ObjectName);
					if (index == -1) {
						index = ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)];
					}
					obj = (GameObject)Instantiate (blockList [index].gameObject);
				}
				else
				{	
					obj = (GameObject)Instantiate (blockList [ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)]].gameObject);
				}

				obj.transform.SetParent (blockContainer.GetChild (i).transform);
				obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
				obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
				obj.gameObject.SetActive (true);
			}
			EGTween.MoveFrom (blockContainer.gameObject, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
		}

		if (GamePlay.instance != null && GameController.instance.GameDoc != null) {
			GameController.instance.GameDoc.Root.Element ("suggestedObject1").Attribute ("objectName").SetValue (blockContainer.GetChild (0).GetChild(0).transform.name);
			GameController.instance.GameDoc.Root.Element ("suggestedObject2").Attribute ("objectName").SetValue (blockContainer.GetChild (1).GetChild(0).transform.name);
			GameController.instance.GameDoc.Root.Element ("suggestedObject3").Attribute ("objectName").SetValue (blockContainer.GetChild (2).GetChild(0).transform.name);
		}

		Debug.Log (GameController.instance.GameDoc.ToString ());
	}

	/// <summary>
	/// Shuffles the generic list.
	/// </summary>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public void ShuffleGenericList<T> (List<T> list)
	{  
		System.Random rng = new System.Random (); 
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next (n + 1);  
			T value = list [k];  
			list [k] = list [n];  
			list [n] = value;  
		}  
	}
}