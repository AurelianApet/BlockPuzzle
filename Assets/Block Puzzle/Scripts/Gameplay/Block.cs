
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Block : MonoBehaviour
{
	[SerializeField]
	public BlockShape ObjectDetails;
	public int blockProbability;
	Vector3 OrigionalScale;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake ()
	{
		OrigionalScale = transform.FindChild ("blocksContainer").localScale;
	}

	/// <summary>
	/// Resets the scaling of the block to original scale.
	/// </summary>
	public void ResetScaling ()
	{
		transform.FindChild ("blocksContainer").localScale = OrigionalScale;
	}
}

[System.Serializable]
public class BlockShapeDetails
{
	public int rowID;
	public int columnId;
}

/// <summary>
/// This class contains all the property related to block.
/// </summary>
[System.Serializable]
public class BlockShape
{
	public int blockID;
	public int totalBlocks;
	public int totalRows;
	public int totalColumns;
	[SerializeField]
	public List<BlockShapeDetails> objectBlocksids;
	public Color blockColor;
	public RectTransform ColliderObject;

	/// <summary>
	/// Initializes a new instance of the <see cref="BlockShape"/> class.
	/// </summary>
	/// <param name="objectId">Object identifier.</param>
	/// <param name="totalBlocks">Total blocks.</param>
	/// <param name="totalRows">Total rows.</param>
	/// <param name="totalColumns">Total columns.</param>
	/// <param name="objectBlocksids">Object blocksids.</param>
	/// <param name="colliderObject">Collider object.</param>
	/// <param name="blockColor">Block color.</param>
	public BlockShape (int objectId, int totalBlocks, int totalRows, int totalColumns, List<BlockShapeDetails> objectBlocksids, RectTransform colliderObject, Color blockColor)
	{
		this.blockID = objectId;
		this.totalBlocks = totalBlocks;
		this.totalRows = totalRows;
		this.totalColumns = totalColumns;
		this.objectBlocksids = objectBlocksids;
		this.ColliderObject = colliderObject;
		this.blockColor = blockColor;
	}
}
