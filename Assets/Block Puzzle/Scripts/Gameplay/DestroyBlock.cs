using UnityEngine;
using System.Collections;

/// <summary>
/// Destroy block.
/// </summary>
public class DestroyBlock : MonoBehaviour
{
	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable ()
	{
		EGTween.ScaleTo (gameObject, EGTween.Hash ("x", 0, "y", 0, "time", 0.5f, "oncomplete", "DestroyObject", "oncompletetarget", gameObject));
	}

	/// <summary>
	/// Destroies the object.
	/// </summary>
	void DestroyObject ()
	{
		Destroy (gameObject);
	}
}
