using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;

/// <summary>
/// Input manager.
/// </summary>
public class InputManager : MonoBehaviour
{
	public static event Action<Vector2> OnTouchDownEvent;
	public static event Action<Vector2> OnTouchUpEvent;

	public static event Action<Vector2> OnMouseDownEvent;
	public static event Action<Vector2> OnMouseUpEvent;

	public static event Action OnBackButtonPressedEvent;

	public static InputManager instance;
	static bool isTouchAvailable = true;

	public EventSystem eventSystem;

	public AudioClip ClickSound;

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

	/// <summary>
	/// Cans the input.
	/// </summary>
	/// <returns><c>true</c>, if input was caned, <c>false</c> otherwise.</returns>
	/// <param name="delay">Delay.</param>
	/// <param name="disableOnAvailable">If set to <c>true</c> disable on available.</param>
	public bool canInput (float delay = 0.5F, bool disableOnAvailable = true)
	{
		bool status = isTouchAvailable;
		if (status && disableOnAvailable) {
			isTouchAvailable = false;
			eventSystem.enabled = false;

			StopCoroutine ("EnbaleTouchAfterDelay");
			StartCoroutine ("EnbaleTouchAfterDelay", delay);

		}
		return status;
	}

	/// <summary>
	/// Disables the touch for delay.
	/// </summary>
	/// <param name="delay">Delay.</param>
	public void DisableTouchForDelay (float delay = 0.5F)
	{
		isTouchAvailable = false;
		eventSystem.enabled = false;

		StopCoroutine ("EnbaleTouchAfterDelay");
		StartCoroutine ("EnbaleTouchAfterDelay", delay);
	}

	/// <summary>
	/// Enables the touch.
	/// </summary>
	public void EnableTouch ()
	{
		isTouchAvailable = true;
		eventSystem.enabled = true;
	}

	/// <summary>
	/// Enbales the touch after delay.
	/// </summary>
	/// <returns>The touch after delay.</returns>
	/// <param name="delay">Delay.</param>
	public IEnumerator EnbaleTouchAfterDelay (float delay)
	{
		yield return new WaitForSeconds (delay);
		EnableTouch ();
	}

	/// <summary>
	/// Adds the button touch effect.
	/// </summary>
	public void AddButtonTouchEffect ()
	{
		if (AudioManager.instance.isSoundEnabled) {
			GetComponent<AudioSource> ().PlayOneShot (ClickSound);
		}
	}

	/// <summary>
	/// Adds the button touch effect.
	/// </summary>
	/// <param name="btn">Button.</param>
	public void AddButtonTouchEffect (GameObject btn)
	{
		if (AudioManager.instance.isSoundEnabled) {
			GetComponent<AudioSource> ().PlayOneShot (ClickSound);
		}
	}
		
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
		#if (UNITY_ANDROID || UNITY_IOS || UNITY_WP8) && !UNITY_EDITOR
		foreach(Touch t in Input.touches)
		{
			switch(t.phase)
			{
			case TouchPhase.Began :
				if(OnTouchDownEvent != null)
				{
					OnTouchDownEvent(t.position);
				}
				break;
			case TouchPhase.Stationary :
				break;

			case TouchPhase.Canceled :
			case TouchPhase.Ended :
				if(OnTouchUpEvent != null)
				{
					OnTouchUpEvent(t.position);
				}
				break;
			}
		}
		#endif

		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (OnBackButtonPressedEvent != null) {
				OnBackButtonPressedEvent ();
			}
		}
	}

	/// <summary>
	/// Raises the GUI event.
	/// </summary>
	void OnGUI ()
	{
		#if UNITY_EDITOR || UNITY_METRO || UNITY_STANDALONE || UNITY_WEBPLAYER || WEBGL
		if (Event.current.type == EventType.MouseDown) {
			if (OnMouseDownEvent != null) {
				OnMouseDownEvent (Input.mousePosition);
			}
		}
		if (Event.current.type == EventType.MouseUp) {
			if (OnMouseUpEvent != null) {
				OnMouseUpEvent (Input.mousePosition);
			}
		}
		#endif
	}
}
