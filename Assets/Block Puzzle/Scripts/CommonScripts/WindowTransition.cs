using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// Add this script to any popup/window if you want to animatate or give transition effect when it is spawed/despawned on the screen.
/// The Transition effect is static and same for all the windows, you can always modify it if you want.
/// </summary>
public class WindowTransition : MonoBehaviour
{
	//set to true if animate on load
	public bool doAnimateOnLoad = true;

	//set to true if animate on destroy.
	public bool doAnimateOnDestroy = true;

	//set to true if need fade effect (black lay) on load.
	public bool doFadeInBackLayOnLoad = true;

	//set to true if need fade effect (black lay) on destroy.
	public bool doFadeOutBacklayOnDestroy = true;

	// Assign the black lay image object.
	public Image BackLay;

	//Assign windows that will animate, suggested that you see any existing window for understanding of the hierarchy.
	public GameObject WindowContent;

	// Time require to transit.
	public float TransitionDuration = 0.35F;

	/// <summary>
	/// This will execute on the time of screen spawn.
	/// </summary>
	public void OnWindowAdded ()
	{
		if (doAnimateOnLoad && (WindowContent != null)) {
			WindowContent.MoveFrom (EGTween.Hash ("x", -600, "easeType", EGTween.EaseType.easeOutBack, "time", TransitionDuration, "islocal", true, "ignoretimescale", true));
		}

		if (doFadeInBackLayOnLoad) {
			BackLay.gameObject.ValueTo (EGTween.Hash ("From", 0, "To", 0.7F, "Time", TransitionDuration, "onupdate", "OnOpacityUpdate", "onupdatetarget", gameObject, "ignoretimescale", true));
		}
	}

	/// <summary>
	/// This will execute on the time of screen destroy.
	/// </summary>
	public void OnWindowRemove ()
	{
		if ((doAnimateOnDestroy && (WindowContent != null))) {
			WindowContent.MoveTo (EGTween.Hash ("x", 600F, "easeType", EGTween.EaseType.easeInBack, "time", 0.5F, "islocal", true, "ignoretimescale", true));

			if (doFadeOutBacklayOnDestroy) {
				BackLay.gameObject.ValueTo (EGTween.Hash ("From", TransitionDuration, "To", 0F, "Time", TransitionDuration, "onupdate", "OnOpacityUpdate", "onupdatetarget", gameObject, "ignoretimescale", true));
			}

			Invoke ("OnRemoveTransitionComplete", 0.5F);
		} else {
			if (doFadeOutBacklayOnDestroy) {
				BackLay.gameObject.ValueTo (EGTween.Hash ("From", TransitionDuration, "To", 0F, "Time", TransitionDuration, "onupdate", "OnOpacityUpdate", "onupdatetarget", gameObject));
				Invoke ("OnRemoveTransitionComplete", 0.5F);
			} else {
				OnRemoveTransitionComplete ();
			}
		}
	}

	/// <summary>
	/// Animates the window on load.
	/// </summary>
	public void AnimateWindowOnLoad ()
	{
		if (doAnimateOnLoad && (WindowContent != null)) {
			WindowContent.MoveFrom (EGTween.Hash ("x", 600, "easeType", EGTween.EaseType.easeOutBack, "time", TransitionDuration, "islocal", true));
		}

		FadeInBackLayOnLoad ();
	}

	/// <summary>
	/// Animates the window on destroy.
	/// </summary>
	public void AnimateWindowOnDestroy ()
	{
		if (doAnimateOnDestroy && (WindowContent != null)) {
			WindowContent.MoveTo (EGTween.Hash ("x", -600F, "easeType", EGTween.EaseType.easeInBack, "time", TransitionDuration, "islocal", true));
		}

		FadeOutBacklayOnDestroy ();
	}

	/// <summary>
	/// Fades the in back lay on load.
	/// </summary>
	public void FadeInBackLayOnLoad ()
	{
		if (doFadeInBackLayOnLoad) {
			BackLay.gameObject.ValueTo (EGTween.Hash ("From", 0F, "To", 0.5F, "Time", TransitionDuration, "onupdate", "OnOpacityUpdate", "onupdatetarget", gameObject));
		}
	}

	/// <summary>
	/// Fades the out backlay on destroy.
	/// </summary>
	public void FadeOutBacklayOnDestroy ()
	{
		if (doFadeOutBacklayOnDestroy) {
			BackLay.gameObject.ValueTo (EGTween.Hash ("From", 0.5F, "To", 0F, "Time", TransitionDuration, "onupdate", "OnOpacityUpdate", "onupdatetarget", gameObject));
		}
	}

	/// <summary>
	/// Raises the opacity update event.
	/// </summary>
	/// <param name="Opacity">Opacity.</param>
	void OnOpacityUpdate (float Opacity)
	{
		BackLay.color = new Color (BackLay.color.r, BackLay.color.g, BackLay.color.b, Opacity);
	}

	/// <summary>
	/// Raises the remove transition complete event.
	/// </summary>
	void OnRemoveTransitionComplete ()
	{
		Destroy (gameObject);
	}
}
