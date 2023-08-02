//#define FACEBOOK_ENABLED

using UnityEngine;
using System;
using System.Collections;

public class AN_SoomlaGrow : SA_Singleton<AN_SoomlaGrow> {


	private static bool _IsInitialized = false;

	public static event Action ActionInitialized 	= delegate {};
	public static event Action ActionConnected 		= delegate {};
	public static event Action ActionDisconnected 	= delegate {};





	// --------------------------------------
	// INITIALIZATION
	// --------------------------------------

	public void CreateListner() {
		
	}

	public static void Init() {

		if(_IsInitialized || !AndroidNativeSettings.Instance.EnableSoomla) {
			return;
		}

		Instance.CreateListner();
		AN_SoomlaProxy.Initalize(AndroidNativeSettings.Instance.SoomlaGameKey, AndroidNativeSettings.Instance.SoomlaEnvKey);

		#if FACEBOOK_ENABLED

		SPFacebook.Instance.OnLoginStarted += FB_OnLoginStarted;
		SPFacebook.Instance.OnLogOut += FB_OnLogOut;
		SPFacebook.Instance.OnAuthCompleteAction += FB_OnAuthCompleteAction;
		SPFacebook.Instance.OnPostStarted += FB_PostStarted;
		SPFacebook.Instance.OnPostingCompleteAction += FB_PostCompleted;
		SPFacebook.Instance.OnFriendsDataRequestCompleteAction += FB_HandleOnFriendsDataRequestCompleteAction;
		SPFacebook.Instance.OnFriendsRequestStarted += FB_OnFriendsRequestStarted;

		#endif

		_IsInitialized = true;
	}






	// --------------------------------------
	// Billing
	// --------------------------------------


	public static void PurchaseStarted(string prodcutId) {

		if (!AN_SoomlaGrow.CheckState ()) { return; }

		AN_SoomlaProxy.OnMarketPurchaseStarted(prodcutId);
	}
	
	public static void PurchaseFinished(string prodcutId, long priceInMicros, string currency) {

		if (!AN_SoomlaGrow.CheckState ()) { return; }

		AN_SoomlaProxy.OnMarketPurchaseFinished(prodcutId, priceInMicros, currency);
	}
	
	public static void PurchaseCanceled(string prodcutId) {
		if (!AN_SoomlaGrow.CheckState ()) { return; }

		AN_SoomlaProxy.OnMarketPurchaseCancelled(prodcutId);
	}
	
	public static void SetPurhsesSupportedState(bool isSupported) {
		if (!AN_SoomlaGrow.CheckState ()) { return; }

		AN_SoomlaProxy.SetBillingState(isSupported);
	}
	
	
	public static void PurchaseError() {
		if (!AN_SoomlaGrow.CheckState ()) { return; }

		AN_SoomlaProxy.OnMarketPurchaseFailed();
	}

	private static void FriendsRequest(AN_SoomlaEventType eventType, AN_SoomlaSocialProvider provider) {
		if (!AN_SoomlaGrow.CheckState ()) { return; }
		
		AN_SoomlaProxy.OnFriendsRequest( (int) eventType, (int) provider);
	}



	public static void SocialLogin(AN_SoomlaEventType eventType, AN_SoomlaSocialProvider provider) {
		if (!AN_SoomlaGrow.CheckState ()) { return; }

		AN_SoomlaProxy.OnSocialLogin( (int) eventType, (int) provider);
	}


	public static void SocialLoginFinished(AN_SoomlaSocialProvider provider, string ProfileId) {
		if (!AN_SoomlaGrow.CheckState ()) { return; }
		
		AN_SoomlaProxy.OnSocialLoginFinished((int) provider, ProfileId);
	}

	public static void SocialLogOut(AN_SoomlaEventType eventType, AN_SoomlaSocialProvider provider) {
		if (!AN_SoomlaGrow.CheckState ()) { return; }

		AN_SoomlaProxy.OnSocialLogout((int) eventType, (int) provider);
	}

	public static void SocialShare(AN_SoomlaEventType eventType, AN_SoomlaSocialProvider provider) {
		if (!AN_SoomlaGrow.CheckState ()) { return; }
		
		AN_SoomlaProxy.OnSocialShare((int) eventType, (int) provider);
	}

	// --------------------------------------
	// Facebook
	// --------------------------------------

	#if FACEBOOK_ENABLED

	private static void FB_OnFriendsRequestStarted() {
		FriendsRequest(AN_SoomlaEventType.SOOMLA_EVENT_STARTED, AN_SoomlaSocialProvider.FACEBOOK);
	}


	private static void FB_HandleOnFriendsDataRequestCompleteAction (FB_APIResult res){
		if(res.IsSucceeded) {
			FriendsRequest(AN_SoomlaEventType.SOOMLA_EVENT_FINISHED, AN_SoomlaSocialProvider.FACEBOOK);
		} else {
			FriendsRequest(AN_SoomlaEventType.SOOMLA_EVENT_FAILED, AN_SoomlaSocialProvider.FACEBOOK);
		}
	}
	

	private static void FB_OnAuthCompleteAction (FB_APIResult res) {
		if(res.IsSucceeded) {
			SocialLoginFinished(AN_SoomlaSocialProvider.FACEBOOK, SPFacebook.Instance.UserId);
		} else {
			SocialLogin(AN_SoomlaEventType.SOOMLA_EVENT_FAILED, AN_SoomlaSocialProvider.FACEBOOK);
		}
	}
	
	private static void FB_OnLoginStarted () {
		SocialLogin(AN_SoomlaEventType.SOOMLA_EVENT_STARTED, AN_SoomlaSocialProvider.FACEBOOK);
	}
	
	private static void FB_OnLogOut () {
		SocialLogOut(AN_SoomlaEventType.SOOMLA_EVENT_STARTED, AN_SoomlaSocialProvider.FACEBOOK);
		SocialLogOut(AN_SoomlaEventType.SOOMLA_EVENT_FINISHED, AN_SoomlaSocialProvider.FACEBOOK);
	}

	private static void FB_PostStarted () {
		SocialShare(AN_SoomlaEventType.SOOMLA_EVENT_STARTED, AN_SoomlaSocialProvider.FACEBOOK);
	}

	private static void FB_PostCompleted (FBPostResult res) {
		Debug.Log("FB_PostCompleted");
		if(res.IsSucceeded) {
			Debug.Log("SOOMLA_EVENT_FINISHED");
			SocialShare(AN_SoomlaEventType.SOOMLA_EVENT_FINISHED, AN_SoomlaSocialProvider.FACEBOOK);
		} else {
			Debug.Log("SOOMLA_EVENT_CNACELED");
			SocialShare(AN_SoomlaEventType.SOOMLA_EVENT_CNACELED, AN_SoomlaSocialProvider.FACEBOOK);
		}

	}

	#endif


	// --------------------------------------
	// Private Methods
	// --------------------------------------


	private static bool CheckState() {

		if(AndroidNativeSettings.Instance.EnableSoomla) {
			Init();
		}

		return AndroidNativeSettings.Instance.EnableSoomla;
	}



	// --------------------------------------
	// Events
	// --------------------------------------


	private void OnInitialized() {
		Debug.Log("AN_SOOMAL OnInitialized");
		ActionInitialized();
	}

	private void OnConnected() {
		ActionConnected();
	}

	private void OnDisconnected() {
		ActionDisconnected();
	}
	

}
