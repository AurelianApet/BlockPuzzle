using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// Game mode.
/// </summary>
public enum GameMode
{
	classic = 0,
	plus = 1,
	bomb = 2,
	//hexa = 3,
	//timer = 4
}

/// <summary>
/// Game play.
/// </summary>
public class GamePlay : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IDragHandler,IBeginDragHandler
{
	public static GamePlay instance;

	public Text txtScore;
	public Text txtBestScore;
	public Transform sampleBlock;

	public GameObject settingsPanel;

	bool blockSelected = false;
	RectTransform SelectedObject;
	Transform LastCheckedBlock;
	public Color blockColor = new Color (0.77F, 0.82F, 0.87F, 1.00F);
	List<Image> hintColorBlocks = new List<Image> ();
	public static GameMode GamePlayMode;

	public int Score;
	int BestScore;

	public AudioClip SFX_BlockPlace;
	public AudioClip SFX_GameOver;

	public AudioClip SFX_SingleLine;
	public AudioClip SFX_DoubleLine;
	public AudioClip SFX_MultiLine;

	//Details For blocked in whuich Bombs are palced
	public List<BombModedetails> bombPlacingDetails = new List<BombModedetails>();
	public int TotalMoves = 0;

	public Sprite dynamiteImage,blockImage;

	/// <summary>
	/// Column List For hexa Mode
	/// </summary>
	public List<RowColumnData> column1List;
	public List<RowColumnData> column2List;

	/// <summary>
	/// Timer Mode Variables
	/// </summary>
	public RectTransform Timer;
	Image BarImage;

	/// <summary>
	/// Progress of bar
	/// </summary>
	public int TimerValue = 120;

	/// <summary>
	/// Mad Timer Value to empty the bar
	/// </summary>
	public int TotalTimerValue = 120;

	public bool isGamePaused = false;
	public List<Color> ThemeColors;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	/// 

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
		BestScore = PlayerPrefs.GetInt ("bestscore_" + GamePlayMode.ToString (), 0);
		txtBestScore.text = "Best : " + BestScore.ToString ();
		EnableSettingsMenu ();
	
//		if (GamePlayMode == GameMode.timer) {
//			BarImage = Timer.GetComponent<Image> ();
//			SetTimer ();
//			Timer.parent.gameObject.SetActive (true);
//		}

		blockColor = (ThemeManager.instance.isDarkTheme) ? ThemeColors [0] : ThemeColors [1];
		Input.multiTouchEnabled = false;
	}

	public void UpdateScoreFromLastSession()
	{
		Score = PlayerPrefs.GetInt ("lastscore_" + GamePlayMode.ToString (), 0);
		txtScore.text = Score.ToString ();
	}

	/// <summary>
	/// Sets the timer For Bar Progress
	/// this method will set and reset the timer on clearing row and column
	/// </summary>
	/// <param name="timerToIncrease">Timer to increase.</param>
	public void SetTimer (float timerToIncrease = 0)
	{
//		if (GamePlayMode == GameMode.timer) {
//			EGTween.Stop (gameObject);
//			float PercentageValue = ((float.Parse (Timer.sizeDelta.x.ToString ()) * 100.0f) / 555.0f);
//			float timerToCompletebar = (PercentageValue * TotalTimerValue) / 100.0f;
//			timerToCompletebar += timerToIncrease;
//			timerToCompletebar = timerToCompletebar > TotalTimerValue ? TotalTimerValue : timerToCompletebar;
//			float newTimerpercentage = timerToCompletebar * 100 / TotalTimerValue;
//			Timer.sizeDelta = new Vector2 ((newTimerpercentage * 555) / 100, Timer.sizeDelta.y);
//			EGTween.ValueTo (gameObject, EGTween.Hash ("from", (float)Timer.sizeDelta.x, "to", 0.0f, "time", timerToCompletebar, "onUpdate", "BarProgress", "onComplete", "OnBarCompelete", "onCompleteTarget", gameObject));
//		}
	}

	/// <summary>
	/// set the current progress to bar.
	/// </summary>
	/// <param name="progress">Progress.</param>
	void BarProgress(float progress)
	{
		if (progress <= 150) {
			BarImage.color = new Color (218.0f/255,46.0f/255,46.0f/255);
		} else {
			BarImage.color = new Color (60.0f/255,156.0f/255,15.0f/255);
		}
		Timer.sizeDelta = new Vector2(progress, Timer.sizeDelta.y);
	}

	/// <summary>
	/// Raises the bar compelete event.
	/// calling game over...
	/// </summary>
	void OnBarCompelete()
	{
		GameController.instance.SpawnUIScreen ("GameOver", true).GetComponent<GameOver> ().setScore (GamePlayMode, Score, BestScore);
		AudioManager.instance.PlayOneShotClip (SFX_GameOver);
	}

	/// <summary>
	/// Raises the undo button pressed event.
	/// </summary>
	public void OnUndoButtonPressed	()
	{
		
	}

	#region IPointerDownHandler implementation

	/// <summary>
	/// Raises the pointer down event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public void OnPointerDown (PointerEventData eventData)
	{
		if (eventData.pointerCurrentRaycast.gameObject != null) {
			GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
			if (clickedObject.name.Contains ("objcontainer")) {
				if (clickedObject.transform.childCount > 0) {
					blockSelected = true;
					SelectedObject = clickedObject.transform.GetChild (0).GetComponent<RectTransform> ();
					SelectedObject.FindChild ("blocksContainer").localScale = Vector3.one;
					SelectedObject.localScale = new Vector3 (0.9f, 0.9f, 0.9f);

					Vector3 pos = Camera.main.ScreenToWorldPoint (eventData.position);
					pos.z = SelectedObject.position.z;
					SelectedObject.position = pos;
				}
			}
		}
	}
	#endregion

	int layerMask = 1 << LayerMask.NameToLayer ("yer");

	#region IBeginDragHandler implementation

	/// <summary>
	/// Raises the begin drag event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public void OnBeginDrag (PointerEventData eventData)
	{
		if (blockSelected && SelectedObject != null) {
			Vector3 pos = Camera.main.ScreenToWorldPoint (eventData.position);
			pos.z = SelectedObject.position.z;
			SelectedObject.position = pos;

			RaycastHit2D hit = Physics2D.Raycast (SelectedObject.GetComponent<Block> ().ObjectDetails.ColliderObject.position, Vector2.zero, layerMask);
			if (hit.collider != null) {
				if (hit.collider.name.Contains ("Block_")) {
					if (LastCheckedBlock != hit.collider.transform) {
						LastCheckedBlock = hit.collider.transform;
						CheckForHintBlocks (hit.collider.transform);
					}
				} else {
					ResetBlockColor ();
				}
			} else {
				ResetBlockColor ();
			}
		}
	}

	#endregion

	#region IDragHandler implementation

	/// <summary>
	/// Raises the drag event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public void OnDrag (PointerEventData eventData)
	{
		if (blockSelected && SelectedObject != null) {

			if (GameController.instance.isHelpRunning == 1) {
				if (transform.GetComponent<ClassicHelp_Gameplay> ()) {
					transform.GetComponent<ClassicHelp_Gameplay> ().StophandAnimation ();
				}
			}

			Vector3 pos = Camera.main.ScreenToWorldPoint (eventData.position);
			pos.z = SelectedObject.position.z;
			SelectedObject.position = pos;

			RaycastHit2D hit = Physics2D.Raycast (SelectedObject.GetComponent<Block> ().ObjectDetails.ColliderObject.position, Vector2.zero, layerMask);
			if (hit.collider != null) {
				if (hit.collider.name.Contains ("Block_")) {
					if (LastCheckedBlock != hit.collider.transform) {
						LastCheckedBlock = hit.collider.transform;
						CheckForHintBlocks (hit.collider.transform);
					}
				} else {
					ResetBlockColor ();
				}
			} else {
				ResetBlockColor ();
			}
		}
	}

	#endregion

	#region IPointerUpHandler implementation

	/// <summary>
	/// Raises the pointer up event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public void OnPointerUp (PointerEventData eventData)
	{
		ResetBlockColor ();
		if (blockSelected) {
			if (SelectedObject != null) {
				int layerMask = 1 << LayerMask.NameToLayer ("objBlockLayer");
				RaycastHit2D hit = Physics2D.Raycast (SelectedObject.GetComponent<Block> ().ObjectDetails.ColliderObject.position, Vector2.zero, layerMask);
				if (hit.collider != null) {
					if (hit.collider.name.Contains ("Block_")) {
						bool CanplaceObject = CheckForEmptyBlocks (hit.collider.transform);
						if (CanplaceObject) {
							StartCoroutine ("PlaceObject", hit.collider.transform);		
						} else {
							SelectedObject.localScale = new Vector3 (0.6f, 0.6f, 1);
							SelectedObject.GetComponent<Block> ().ResetScaling ();
							SelectedObject.anchoredPosition = Vector2.zero;
						}
					} else {
						SelectedObject.localScale = new Vector3 (0.6f, 0.6f, 1);
						SelectedObject.GetComponent<Block> ().ResetScaling ();
						SelectedObject.anchoredPosition = Vector2.zero;
					}
				} else {
					SelectedObject.localScale = new Vector3 (0.6f, 0.6f, 1);
					SelectedObject.GetComponent<Block> ().ResetScaling ();
					SelectedObject.anchoredPosition = Vector2.zero;
				}
			}
		}
	}

	#endregion

	/// <summary>
	/// Places the object on given block.
	/// </summary>
	/// <param name="BlockToCheck">Block to check.</param>
	IEnumerator PlaceObject (Transform BlockToCheck)
	{
		if (BlockToCheck != null) {
			string[] id = BlockToCheck.name.Split ('_');
			if (id.Length >= 3) {
				int rowId = id [1].TryParseInt (-1);
				int columnId = id [2].TryParseInt (-1);
				if (rowId != -1 && columnId != -1) {
					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == rowId && o.columnId == columnId);
					if (data != null) {
						if (!data.isFilled) {
							Color ColorToSet = SelectedObject.GetComponent<Block> ().ObjectDetails.blockColor;
							List<BlockShapeDetails> ObjectBlocks = SelectedObject.GetComponent<Block> ().ObjectDetails.objectBlocksids;
							SelectedObject.transform.position = data.block.transform.position;
							foreach (BlockShapeDetails s  in ObjectBlocks) {
								BlockData chkBlock = BlockManager.instance.BlockList.Find (o => o.rowId == (rowId + s.rowID) && o.columnId == (columnId + s.columnId));
								if (chkBlock != null && !chkBlock.isFilled) {
									chkBlock.block.color = ColorToSet;
									chkBlock.isFilled = true;
									GameController.instance.createBlockElement (chkBlock.rowId, chkBlock.columnId, ColorToSet);
								}
							}
							AudioManager.instance.PlayOneShotClip (SFX_BlockPlace);
							StartCoroutine (UpdateScore (ObjectBlocks.Count * 10));
						}
					}
				}
			}
		}

		blockSelected = false;
		Destroy (SelectedObject.gameObject);
		SelectedObject = null;
		yield return CheckForRowColumn();
		if (GameController.instance.isHelpRunning == 0) {
			BlockTrayManager.instance.OnPlacingBlock ();
		} else {
			if(GameController.instance.isHelpRunning == 1)
			{
				GetComponent<ClassicHelp_Gameplay> ().PlaceObject ();
			}
		}
		CheckForRestObjectPlacing ();
		TotalMoves++;
		if (GamePlayMode == GameMode.bomb) {
			BombCounter ();
		}
	}

	/// <summary>
	/// Checks for row column Filled or Not.
	/// </summary>
	IEnumerator CheckForRowColumn ()
	{
		List<List<BlockData>> BlocksToDestroy = new List<List<BlockData>> ();
		
		//CheckColumns
		int Count = 0;

//		if (GamePlayMode == GameMode.hexa) {
//			//ColumnList1
//			for (int i = 0; i < column1List.Count; i++) {
//				List<BlockData> blocklist = new List<BlockData> ();
//				int startRowId = column1List [i].RowStartIndex;
//				int startcolumnId = column1List [i].columnStartIndex;
//				while (startRowId <= column1List [i].rowLastElement && startcolumnId <= column1List [i].columnLastElement) {
//					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == startRowId && o.columnId == startcolumnId);
//					if (data != null) {
//						if (!data.isFilled) {
//							blocklist.Clear ();
//							break;
//						} else {
//							blocklist.Add (data);
//						}
//					}
//					startRowId += column1List [i].AddtoRow;
//					startcolumnId += column1List [i].AddtoColumn;
//					//				yield return new WaitForSeconds(0.01f);
//				}
//				if (blocklist.Count > 0) {
//					Count++;
//					BlocksToDestroy.Add (blocklist);
//				}
//			}
//
//			//ColumnList2
//			for (int i = 0; i < column2List.Count; i++) {
//				List<BlockData> blocklist = new List<BlockData> ();
//				int startRowId = column2List [i].RowStartIndex;
//				int startcolumnId = column2List [i].columnStartIndex;
//				while (startRowId <= column2List [i].rowLastElement && startcolumnId <= column2List [i].columnLastElement) {
//					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == startRowId && o.columnId == startcolumnId);
//					if (data != null) {
//						if (!data.isFilled) {
//							blocklist.Clear ();
//							break;
//						} else {
//							blocklist.Add (data);
//						}
//					}
//					startRowId += column2List [i].AddtoRow;
//					startcolumnId += column2List [i].AddtoColumn;
//					//				yield return new WaitForSeconds(0.01f);
//				}
//				if (blocklist.Count > 0) {
//					Count++;
//					BlocksToDestroy.Add (blocklist);
//				}
//			}
//		} 
//		else 
		{
			for (int i = 0; i < BlockManager.instance.TotalColumns; i++) {
				List<BlockData> blocklist = BlockManager.instance.BlockList.FindAll (o => o.rowId <= BlockManager.instance.TotalRows && o.columnId == i);
				if (blocklist != null && blocklist.Count > 0) {
					if (blocklist.FindAll (o => o.isFilled == false).Count == 0) {
						Count++;
						BlocksToDestroy.Add (blocklist);
						if (GameController.instance.isHelpRunning == 1) {
							CompletedOnhelp ("column");
						}
						SetTimer (5);
					}
				}
			}
		}

		//CheckRows
		for (int i = 0; i < BlockManager.instance.TotalRows; i++) {
			List<BlockData> blocklist = BlockManager.instance.BlockList.FindAll (o => o.rowId == i && o.columnId <= BlockManager.instance.TotalColumns);
			if (blocklist  != null && blocklist.Count > 0) {
				if (blocklist.FindAll (o => o.isFilled == false).Count == 0) {
					Count++;
					BlocksToDestroy.Add(blocklist);
					CompletedOnhelp ("row");
					SetTimer (5);
				}
			}
		}
		
		if(BlocksToDestroy != null && BlocksToDestroy.Count > 0)
		{
			if (Count > 0) 
			{
				if (Count == 1) {
					AudioManager.instance.PlayOneShotClip (SFX_SingleLine);
				} else if (Count == 2) {
					AudioManager.instance.PlayOneShotClip (SFX_DoubleLine);
				} else if (Count == 3) {
					AudioManager.instance.PlayOneShotClip (SFX_MultiLine);
				}
				int addingScore = (Count *(((Count - 1) * 50) + 100));
				StartCoroutine (UpdateScore (addingScore));
			}

			for (int i = 0; i < BlocksToDestroy.Count; i++) 
			{
				foreach (BlockData d in BlocksToDestroy[i]) 
				{
					GameObject currentBlock = (GameObject)Instantiate (sampleBlock.gameObject);
					currentBlock.transform.SetParent (sampleBlock.parent);
					currentBlock.transform.localScale = Vector3.one;
					currentBlock.GetComponent<RectTransform> ().position = d.block.GetComponent<RectTransform> ().position;
					currentBlock.GetComponent<Image> ().color = d.block.color;
					currentBlock.gameObject.SetActive (true);
					d.isFilled = false;
					d.block.GetComponent<Image> ().color = blockColor;

					GameController.instance.createBlockElement (d.rowId, d.columnId, blockColor);

					if (GamePlayMode == GameMode.bomb && bombPlacingDetails != null && bombPlacingDetails.Count > 0) {
						int index = bombPlacingDetails.FindIndex (o => o.block.rowId == d.rowId && o.block.columnId == d.columnId);
						if (index != -1) {
							BlockData objblock = BlockManager.instance.BlockList.Find (o => o.rowId == bombPlacingDetails [index].block.rowId && o.columnId == bombPlacingDetails [index].block.columnId);
							if (objblock != null) {
								if (objblock.block.transform.childCount > 0) {
									objblock.block.sprite = blockImage;
									objblock.block.color = blockColor;
									Destroy (objblock.block.transform.GetChild (0).gameObject);
								}
							}
							bombPlacingDetails.RemoveAt (index);
						}
					}

					yield return new WaitForSeconds (0.01F);
				}
			}
		}
	}

	void BombCounter()
	{
		bool Gameover = false;
		if (bombPlacingDetails != null && bombPlacingDetails.Count > 0) {
			foreach (BombModedetails s in bombPlacingDetails) {
				s.counter--;
				if (s.block.block.transform.childCount > 0) {
					s.block.block.transform.GetChild (0).GetChild(0).GetComponent<Text> ().text = s.counter.ToString ();
				}

				if (s.counter == 0) {
					Gameover = true;
					foreach(BombModedetails b in bombPlacingDetails)
					{
						BlockData objblock = BlockManager.instance.BlockList.Find (o => o.rowId == b.block.rowId && o.columnId == b.block.columnId);
						if (objblock != null) {
							if (objblock.block.transform.childCount > 0) {
								objblock.block.sprite = blockImage;
								objblock.block.color = blockColor;
								Destroy (objblock.block.transform.GetChild (0).gameObject);
							}
						}
					}
					bombPlacingDetails.Clear ();
					break;
				}
			}	
		}

		if (!Gameover) {
			PlaceBomb ();
		}
		else 
		{
			GameController.instance.SpawnUIScreen ("GameOver", true).GetComponent<GameOver> ().setScore (GamePlayMode, Score, BestScore);
			AudioManager.instance.PlayOneShotClip (SFX_GameOver);
		}
	}

	void PlaceBomb()
	{
		bool DoPlaceBomb = false;
		int NoOfBombToPlace = 1;
		if (TotalMoves % 5 == 0 && bombPlacingDetails != null && bombPlacingDetails.Count < 10) {
			if (TotalMoves > 100) {
				NoOfBombToPlace = bombPlacingDetails.Count+3 > 10 ? 10-bombPlacingDetails.Count : 3;
			} else if (TotalMoves > 50) {
				NoOfBombToPlace = bombPlacingDetails.Count+2 > 10 ? 10-bombPlacingDetails.Count : 2;
			}
			DoPlaceBomb = true;
		}

		if (DoPlaceBomb && bombPlacingDetails != null) 
		{
			if (DoPlaceBomb) {
				for (int i = 0; i < NoOfBombToPlace; i++) {
					List<BlockData> blockList = BlockManager.instance.BlockList.FindAll (o => o.isFilled == true && bombPlacingDetails.FindIndex (b => b.block.rowId == o.rowId && b.block.columnId == o.columnId) == -1);
					BlockData block = blockList [Random.Range (0, blockList.Count)];
					block.block.sprite = dynamiteImage;
					block.block.color = new Color (1, 1, 1, 1);
					GameObject Text = (GameObject)Instantiate (transform.FindChild ("GamePlay-Content/txt-Bomb").gameObject);
					Text.transform.SetParent (block.block.transform);
					Text.transform.localScale = Vector3.one;
					Text.GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;
					int Counter = Random.Range (6, 10);
					Text.transform.GetChild (0).GetComponent<Text> ().text = Counter.ToString ();
					Text.gameObject.SetActive (true);
					bombPlacingDetails.Add (new BombModedetails (block, Counter));
					GameController.instance.createBombElement (block.rowId, block.columnId, Counter);
				}
			}
		}
	}

	/// <summary>
	/// Updates the score.
	/// </summary>
	/// <returns>The score.</returns>
	/// <param name="ScoreToUpdate">Score to update.</param>
	IEnumerator UpdateScore (int ScoreToUpdate)
	{
		int lastScore = Score;
		Score += ScoreToUpdate;

		txtScore.rectTransform.localScale = Vector3.one * 1.2F;
		for (int count = lastScore; count <= Score; count += (ScoreToUpdate / 10)) {
			txtScore.text = count.ToString ();
			yield return new WaitForSeconds (0.001F);
		}
		txtScore.text = Score.ToString ();
		txtScore.rectTransform.localScale = Vector3.one;

		if (Score > BestScore) {
			BestScore = Score;
			PlayerPrefs.SetInt ("bestscore_" + GamePlayMode.ToString (), BestScore);
			txtBestScore.text = "Best : " + BestScore.ToString ();
		}

		PlayerPrefs.SetInt ("lastscore_" + GamePlayMode.ToString (), Score);
	}

	/// <summary>
	/// Checks for rest object placing.
	/// </summary>
	void CheckForRestObjectPlacing ()
	{
		Transform nextBlocks = transform.FindChild ("GamePlay-Content").FindChild ("NextBlocks-Container");
		List<RectTransform> objectToplace = new List<RectTransform> ();

		foreach (Transform nextBlock in nextBlocks) {
			if (nextBlock.childCount > 0) {
				objectToplace.Add (nextBlock.GetChild (0).GetComponent<RectTransform> ());
			}
		}

		List<bool> CanPlaceObject = new List<bool> ();
		if (objectToplace.Count > 0) {
			foreach (RectTransform s in objectToplace) {
				SelectedObject = s;
				foreach (BlockData d in BlockManager.instance.BlockList) {
					if (!d.isFilled) {
						if (!CheckForEmptyBlocks (d.block.transform)) {
							CanPlaceObject.Add (false);
						} else {
							CanPlaceObject.Add (true);
							break;
						}
					}
				}

				if (CanPlaceObject.FindIndex (o => o == true) != -1) {
					break;
				}
			}

			if(CanPlaceObject.FindIndex(o=>o == true) == -1)
			{
				GameController.instance.SpawnUIScreen ("GameOver", true).GetComponent<GameOver> ().setScore (GamePlayMode, Score, BestScore);
				AudioManager.instance.PlayOneShotClip (SFX_GameOver);
			}
		}
	}


	/// <summary>
	/// Checks for empty blocks.
	/// Check object is placed on this block or not
	/// </summary>
	/// <returns><c>true</c>, if empty blocks was checked, <c>false</c> otherwise.</returns>
	/// <param name="BlockToCheck">Block to check.</param>
	bool CheckForEmptyBlocks (Transform BlockToCheck)
	{
		bool canPlaceBlock = false;
		if (BlockToCheck != null) {
			string[] id = BlockToCheck.name.Split ('_');
			if (id.Length >= 3) {
				int rowId = id [1].TryParseInt (-1);
				int columnId = id [2].TryParseInt (-1);

				if (rowId != -1 && columnId != -1) {
					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == rowId && o.columnId == columnId);
					if (data != null) {
						if (!data.isFilled) {
							List<BlockShapeDetails> ObjectBlocks = SelectedObject.GetComponent<Block> ().ObjectDetails.objectBlocksids;
							bool BlockFilled = false;
							foreach (BlockShapeDetails s  in ObjectBlocks) {
								BlockData chkBlock = BlockManager.instance.BlockList.Find (o => o.rowId == (rowId + s.rowID) && o.columnId == (columnId + s.columnId));
								if (chkBlock != null && !chkBlock.isFilled) {
									
								} else {
									BlockFilled = true;
									break;
								}
							}

							canPlaceBlock = !BlockFilled;
						}
					}
				}
			}
		}

		return canPlaceBlock;
	}

	/// <summary>
	/// Resets the color of the block.
	/// </summary>
	void ResetBlockColor ()
	{
		if (hintColorBlocks != null && hintColorBlocks.Count > 0) {
			foreach (Image i in hintColorBlocks) {
				i.color = blockColor;
			}
			hintColorBlocks.Clear ();
		}
	}

	/// <summary>
	/// Checks for empty blocks.
	/// Check object is placed on this block or not
	/// </summary>
	/// <returns><c>true</c>, if empty blocks was checked, <c>false</c> otherwise.</returns>
	/// <param name="BlockToCheck">Block to check.</param>
	void CheckForHintBlocks (Transform BlockToCheck)
	{
		ResetBlockColor ();
		if (BlockToCheck != null) {
			string[] id = BlockToCheck.name.Split ('_');

			if (id.Length >= 3) {
				int rowId = id [1].TryParseInt (-1);
				int columnId = id [2].TryParseInt (-1);

				if (rowId != -1 && columnId != -1) {
					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == rowId && o.columnId == columnId);

					if (data != null) {
						if (!data.isFilled) {
							Block objManager = SelectedObject.GetComponent<Block> ();
							List<BlockShapeDetails> ObjectBlocks = objManager.ObjectDetails.objectBlocksids;

							bool BlockFilled = false;
							foreach (BlockShapeDetails s  in ObjectBlocks) {
								BlockData chkBlock = BlockManager.instance.BlockList.Find (o => o.rowId == (rowId + s.rowID) && o.columnId == (columnId + s.columnId));
								if (chkBlock != null && !chkBlock.isFilled) {
									hintColorBlocks.Add (chkBlock.block.GetComponent<Image> ());
								} else {
									hintColorBlocks.Clear ();
									BlockFilled = true;
									break;
								}
							}

							if (!BlockFilled && hintColorBlocks != null && hintColorBlocks.Count > 0) {
								foreach (Image i in hintColorBlocks) {
									i.color = new Color (objManager.ObjectDetails.blockColor.r, objManager.ObjectDetails.blockColor.g, objManager.ObjectDetails.blockColor.b, 0.5f);
								}
							}
						}
					}
				}
			}
		}
	}



	/// <summary>
	/// if Help is Running Than This Method Will Trigger next step for Help
	/// </summary>
	/// <param name="Completed">Completed.</param>
	void CompletedOnhelp(string Completed)
	{
		switch(GameController.instance.isHelpRunning)
		{
		case 1:
			if (Completed == "row") {
				GetComponent<ClassicHelp_Gameplay> ().OnRowComplete ();
			} else {
				GetComponent<ClassicHelp_Gameplay> ().OnColumnComplete();
			}
			break;
		}
	}

	void OnDisable()
	{
		ThemeManager.OnThemeChangedEvent -= OnThemeChangedEvent;
//		if (GamePlayMode == GameMode.timer) {
//			StopCoroutine("timerForTimeMode");
//		}
		DisableSettingsMenu ();
	}

	void EnableSettingsMenu()
	{
		if (SettingsContent.instance.settings_gameplay != null) {
			SettingsContent.instance.settings_gameplay.gameObject.SetActive(true);
			SettingsContent.instance.transform.SetAsLastSibling();
		}
	}
	
	void DisableSettingsMenu() {
		if (SettingsContent.instance.settings_gameplay != null) {
			SettingsContent.instance.settings_gameplay.gameObject.SetActive(false);
		}
	}

	void OnEnable()
	{
		ThemeManager.OnThemeChangedEvent += OnThemeChangedEvent;
	}

	void OnThemeChangedEvent (bool isDarkTheme)
	{
		blockColor = (isDarkTheme) ? ThemeColors [0] : ThemeColors [1];
	}
}

public class BombModedetails
{
	public BlockData block;
	public int counter;

	public BombModedetails(BlockData _block,int _counter)
	{
		this.block = _block;
		this.counter = _counter;
	}
}


[System.Serializable]
public class RowColumnData
{
	public int RowStartIndex;
	public int AddtoRow;
	[Space]
	public int columnStartIndex;
	public int AddtoColumn;
	[Space]
	public int rowLastElement;
	public int columnLastElement;

}