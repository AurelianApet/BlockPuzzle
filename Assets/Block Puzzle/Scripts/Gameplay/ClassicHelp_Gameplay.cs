using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ClassicHelp_Gameplay : MonoBehaviour
{
	public string helpMode = "";
	float blockTransitionTime = 0.5F;
	Transform blockContainer;
	Transform HelpContent;
	Transform handImage;

	Vector3 targetPosition;
	Vector3 blockPosition;

	void Start()
	{
		HelpContent = transform.FindChild ("GamePlay-Content").FindChild ("Content");
		blockContainer = BlockTrayManager.instance.blockContainer;
		helpMode = "Horizontal";
		PlaceObject ();
		Invoke ("ChangeBlockColor", 0.1F);
		HelpContent.FindChild ("txt-help1").gameObject.SetActive (true);
		handImage = transform.FindChild ("GamePlay-Content/img-hand").transform;
		Invoke ("HandAnimation", 1f);
	}

	void HandAnimation()
	{
		if (HelpContent.FindChild ("txt-help1").gameObject.activeSelf) {
			targetPosition = BlockManager.instance.BlockList.Find (o => o.rowId == 2 && o.columnId == 1).block.transform.position;
			blockPosition = blockContainer.GetChild (0).GetChild (0).position;
		}
		else 
		{
			targetPosition = BlockManager.instance.BlockList.Find (o => o.rowId == 1 && o.columnId == 2).block.transform.position;
			blockPosition = blockContainer.GetChild (2).GetChild (0).position;
		}


		blockPosition.z = 0;
		targetPosition.z = 0;
		handImage.transform.position = blockPosition;
		handImage.gameObject.SetActive (true);
		EGTween.MoveTo (handImage.gameObject, EGTween.Hash ("Delay", 1F, "x", targetPosition.x, "y", targetPosition.y, "time", 2.5f, "easeType", EGTween.EaseType.linear));
		Invoke ("RepeatHandAnimation", 4.5f);
	}

	void RepeatHandAnimation()
	{
		handImage.transform.position = blockPosition;
		EGTween.MoveTo (handImage.gameObject, EGTween.Hash ("Delay", 0.5F,"x", targetPosition.x, "y", targetPosition.y, "time", 2.5f, "easeType", EGTween.EaseType.linear));
		Invoke ("RepeatHandAnimation", 4.0f);
	}

	public void StophandAnimation()
	{
		if (handImage.gameObject.activeSelf) {
			EGTween.Stop (handImage.gameObject);
			handImage.gameObject.SetActive (false);
		}
	}

	/// <summary>
	/// Changes the color of the block To Highlight Block For Help.
	/// </summary>
	void ChangeBlockColor()
	{
		Color blockcolorOpauqe = new Color (0.77F, 0.82F, 0.87F, 0.3F);
		Color blockcolorNormal = new Color (0.77F, 0.82F, 0.87F, 1.0F);
		if (BlockManager.instance.BlockList != null && BlockManager.instance.BlockList.Count > 0) 
		{
			foreach (BlockData blockdata in BlockManager.instance.BlockList) {
				if (helpMode == "Horizontal") {
					if (blockdata.rowId == 2) {
						blockdata.block.color = blockcolorNormal;
						blockdata.block.GetComponent<BoxCollider2D> ().enabled = true;
					} else {
						blockdata.block.color = blockcolorOpauqe;
						blockdata.block.GetComponent<BoxCollider2D> ().enabled = false;
					}

				} else {
					if (blockdata.columnId == 2) {
						blockdata.block.color = blockcolorNormal;
						blockdata.block.GetComponent<BoxCollider2D> ().enabled = true;
					} else {
						blockdata.block.color = blockcolorOpauqe;
						blockdata.block.GetComponent<BoxCollider2D> ().enabled = false;
					}
				}
			}
		}

		if (helpMode != "Horizontal") {
			transform.FindChild ("GamePlay-Content/img-rect").transform.eulerAngles = new Vector3 (0, 0,90);
		}
	}

	public void OnRowComplete()
	{
		HelpContent.FindChild ("txt-help1").gameObject.SetActive (false);
		HelpContent.FindChild ("txt-help3").gameObject.SetActive (true);
		HelpContent.FindChild ("txt-help2").gameObject.SetActive (true);
		helpMode = "Verticle";
		Invoke ("ResetBlockContainer", 1f);
	}

	void ResetBlockContainer()
	{
		ChangeBlockColor ();
		foreach (Transform t in blockContainer) {
			if(t.childCount > 0)
			{
				Destroy (t.GetChild (0).gameObject);
			}
		}
		Invoke ("PlaceObject",0.2F);
		HelpContent.FindChild ("txt-help3").gameObject.SetActive (false);
		HelpContent.FindChild ("txt-help2").gameObject.SetActive (false);
		HelpContent.FindChild ("txt-help4").gameObject.SetActive (true);
		Invoke ("HandAnimation", 1f);
	}

	public void OnColumnComplete()
	{
		HelpContent.FindChild ("txt-help4").gameObject.SetActive (false);
		HelpContent.FindChild ("txt-help5").gameObject.SetActive (true);
		transform.FindChild ("GamePlay-Content/img-rect").gameObject.SetActive (false);
		Invoke ("helpComplete", 1.0F);
	}

	void helpComplete()
	{
		GameController.instance.OnCloseButtonPressed ();
		GameController.instance.isHelpRunning = 0;
		GameController.instance.SpawnUIScreen ("Classic_HelpIntro_3", true);
	}

	public void PlaceObject()
	{
		int blockRemained = 0;

		List<Transform> blockList = BlockTrayManager.instance.blockList;
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
						GameObject obj = (GameObject)Instantiate (blockList [2].gameObject);
						obj.transform.SetParent (blockContainer.GetChild (i - 1).transform);
						obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
						obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
						obj.gameObject.SetActive (true);
						EGTween.MoveFrom (obj, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
					}
				}
			}

			blockContainer.GetChild (1).GetComponent<Image> ().raycastTarget = true;
			blockContainer.GetChild (helpMode == "Horizontal" ? 2 : 0).GetComponent<Image> ().raycastTarget = true;
		} else if (blockRemained == 0) {
			blockContainer.GetChild (1).GetComponent<Image> ().raycastTarget = false;
			blockContainer.GetChild (helpMode == "Horizontal" ? 2 : 0).GetComponent<Image> ().raycastTarget = false;
			for (int i = 0; i < 3; i++) {
				GameObject obj = (GameObject)Instantiate (blockList [helpMode == "Horizontal" ? i:i+2].gameObject);
				obj.transform.SetParent (blockContainer.GetChild (i).transform);
				obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
				obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
				obj.gameObject.SetActive (true);
			}
			EGTween.MoveFrom (blockContainer.gameObject, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
		}
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
}
