using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using System;

public class FBook : MonoBehaviour
{
    public Button fbButton;
    List<string> perms = new List<string>() { "public_profile", "email", "user_friends" };
    void Awake()
    {
        //#if UNITY_IOS || UNITY_ANDROID
        //        if (!FB.IsInitialized)
        //        {
        //            // Initialize the Facebook SDK
        //            FB.Init(InitCallback, OnHideUnity);
        //        }
        //        else
        //        {
        //            // Already initialized, signal an app activation App Event
        //            FB.ActivateApp();
        //        }
        //#endif

        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }
    void Start()
    {
#if UNITY_IOS || UNITY_ANDROID
       // fbButton.interactable = !FB.IsLoggedIn;
#endif
    }
    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
            var perms = new List<string>() { "public_profile", "email", "user_friends" };
            //FB.LogInWithReadPermissions(perms, AuthCallback);

        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }
           // fbButton.interactable = false;
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }
    private void FeedCallback(IShareResult result)
    {

    }
    public void OnShareClicked()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (FB.IsLoggedIn)
        {
            ShareScore();
        }
        else
        {
            FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, AuthCallback);
        }
#endif
    }
    private void ShareScore()
    {
        int score;
        score = 0;
        if (score == 0) score = 1;
        FB.FeedShare(
            link: new Uri("https://google.com/"),
            linkName: "World!",
            linkCaption: "Who is reached more world",
            linkDescription: " I got " + score + " world in Springer!",
            picture: new Uri("http://i.imgsafe.org/c3a842a627.png"),
            callback: FeedCallback
        );
    }
    public void OnSettingsFbClicked()
    {
#if UNITY_IOS || UNITY_ANDROID
        FB.LogInWithReadPermissions(perms, AuthCallback);
#endif
    }
}
