using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class BlockManager : MonoBehaviour 
{
	public int TotalRows = 10;
	public int TotalColumns = 10;
	public List<BlockData> BlockList = new List<BlockData>();
	public static BlockManager instance;
	public Color blockColor;
	public Color[] ThemeColors;

	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
	}

	void Start()
	{
//		if (GamePlay.GamePlayMode == GameMode.hexa) {
//			TotalRows = 9;
//			TotalColumns = 9;
//		} else 
		{
			TotalRows = 10;
			TotalColumns = 10;
		}
		blockColor = (ThemeManager.instance.isDarkTheme) ? ThemeColors [0] : ThemeColors [1];
		InitializeBlocks ();
	}

	/// <summary>
	/// Initializes the blockList.
	/// </summary>
	public void InitializeBlocks()
	{
		Transform obj;
		int blockId = 0;

		for(int i = 0;i<TotalRows;i++)
		{
			for(int j = 0;j<TotalColumns;j++)
			{
				obj = transform.FindChild ("Block_" + i + "_" + j);
	
				if (obj != null) 
				{
					obj.GetComponent<Image>().color = blockColor;

					if (GameController.instance.PlayFromLastStatus) 
					{
						
						XElement rootElemnt = GameController.instance.GameDoc.Root;
						XElement BlockElement = rootElemnt.Elements ("block").Where (o => o.Attribute ("row").Value == i.ToString () && o.Attribute ("col").Value == j.ToString ()).FirstOrDefault ();
						//XElement BombElement = rootElemnt.Elements ("bomb").Where (o => o.Attribute ("row").Value == i.ToString () && o.Attribute ("col").Value == j.ToString ()).FirstOrDefault ();
						bool isFilled = false;
						if (BlockElement != null) {
							Color _blockColor = new Color (
								                   float.Parse (BlockElement.Element ("color").Attribute ("r").Value),
								                   float.Parse (BlockElement.Element ("color").Attribute ("g").Value),
								                   float.Parse (BlockElement.Element ("color").Attribute ("b").Value)
							                   );
							if (_blockColor != ThemeColors [0] && _blockColor != ThemeColors [1]) {
								isFilled = true;
								obj.GetComponent<Image>().color = _blockColor;      
							}

//							if (BombElement != null) {
//								obj.GetComponent<Image> ().sprite = GamePlay.instance.dynamiteImage;
//								obj.GetComponent<Image> ().color = new Color (1, 1, 1, 1);
//								GameObject Text = (GameObject)Instantiate (GamePlay.instance.transform.FindChild ("GamePlay-Content/txt-Bomb").gameObject);
//								Text.transform.SetParent (obj.transform);
//								Text.transform.localScale = Vector3.one;
//								Text.GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;
//								Text.transform.GetChild (0).GetComponent<Text> ().text = BombElement.Attribute ("number").Value;
//								Text.gameObject.SetActive (true);
//								GamePlay.instance.bombPlacingDetails.Add (new BombModedetails (new BlockData (blockId, i, j, true, obj.GetComponent<Image> ()), int.Parse (BombElement.Attribute ("number").Value)));
//							}
						}
						BlockList.Add (new BlockData (blockId, i, j, isFilled, obj.GetComponent<Image> ()));
						GamePlay.instance.UpdateScoreFromLastSession ();
					} else {
						//						Debug.LogError ("Adding " + i + " , " + j);
						BlockList.Add (new BlockData (blockId, i, j, false, obj.GetComponent<Image> ()));
						GameController.instance.createBlockElement (i, j, GamePlay.instance.blockColor);
					}
				}
				blockId++;
			}
		}
		if (GameController.instance.PlayFromLastStatus) {
			GamePlay.instance.Score = int.Parse (GameController.instance.GameDoc.Root.Element ("totalScore").Attribute ("score").Value);
			PlayerPrefs.SetString ("GameData",string.Empty);

//			if (GamePlay.GamePlayMode == GameMode.timer) 
//			{
//				if (GameController.instance.GameDoc.Root.Element ("timerValue") != null) {
//					GamePlay.instance.TimerValue = int.Parse (GameController.instance.GameDoc.Root.Element ("timerValue").Attribute ("time").Value);
//				 	float PercentageValue = (GamePlay.instance.TimerValue * 100) / GamePlay.instance.TotalTimerValue;
//					GamePlay.instance.Timer.sizeDelta = new Vector2 ((PercentageValue * 555) / 100,GamePlay.instance.Timer.sizeDelta.y);
//				}
//			}
		}
		GameController.instance.PlayFromLastStatus = false;
	}

	/// <summary>
	/// ReInitializes the blockList.
	/// </summary>
	public void ReInitializeBlocks()
	{
		Transform obj;
		int blockId = 0;
		BlockList.Clear ();
		for(int i = 0;i<TotalRows;i++)
		{
			for(int j = 0;j<TotalColumns;j++)
			{
				obj = transform.FindChild ("Block_" + i + "_" + j);
				if (obj != null) {
					BlockList.Add (new BlockData (blockId, i, j, false, obj.GetComponent<Image> ()));
					obj.GetComponent<Image> ().color = blockColor;
					GameController.instance.createBlockElement (i, j, blockColor);
					blockId++;
				}
			}
		}
		GameController.instance.GameDoc.Root.Descendants ().Where (e => e.Name == "bomb").Remove ();
	}


	void OnEnable()
	{
		ThemeManager.OnThemeChangedEvent += OnThemeChangedEvent;
	}

	void OnDisable()
	{
		ThemeManager.OnThemeChangedEvent -= OnThemeChangedEvent;
	}

	void OnThemeChangedEvent (bool isDarkTheme)
	{
		blockColor = (isDarkTheme) ? ThemeColors [0] : ThemeColors [1];
		foreach (Transform t in transform) {
			Image img = t.GetComponent<Image> ();
			if (img.color == ThemeColors [0] || img.color == ThemeColors [1]) {
				t.GetComponent<Image> ().color = blockColor;
			}
		}
	}
}

public class BlockData
{
	public int blockId;
	public int rowId;
	public int columnId;
	public bool isFilled;
	public Image block;

	public BlockData (int blockId, int rowId, int columnId, bool isFilled,Image block)
	{
		this.blockId = blockId;
		this.rowId = rowId;
		this.columnId = columnId;
		this.isFilled = isFilled;
		this.block = block;
	}	


}
