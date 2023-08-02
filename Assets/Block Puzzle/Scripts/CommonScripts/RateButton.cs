using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RateButton : MonoBehaviour 
{
	//  The button to rate. Assigned from inspector.
	public Button btnRate;
	//	The URL to navigate on playstore. only for android.
	public string PlayStoreURL;
	// The URL to navigate to appstore. only for iOS.
	public string AmazonStoreURL;
	//The UTL to navigate to amazon appstore. set isAmazon to true if you want to navigate there.
	public string AppStoreURL = "itms-apps://itunes.apple.com/app/id1147338552";

	public bool isAmazon = false;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		btnRate.onClick.AddListener(() => 
		{
			if (InputManager.instance.canInput ()) 
			{
				AudioManager.instance.PlayButtonClickSound ();

				#if UNITY_ANDROID
				if(!isAmazon) {
					Application.OpenURL(PlayStoreURL);
				}
				else {
					Application.OpenURL(AmazonStoreURL);
				}
				#elif UNITY_IOS
				Application.OpenURL(AppStoreURL);
				#elif UNITY_EDITOR
				Application.OpenURL("http://www.epilexgames.com");
				#endif
			}
		});
	}
}
