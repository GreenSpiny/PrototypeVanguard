using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

// Most code here is from Unity's Lobby tutorial.

public class UnityGameServices : MonoBehaviour
{
    public static UnityGameServices instance;
    private static bool servicesInitialized = false;
    private static string lastSignInError = string.Empty;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            InitializeServices();
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    async void InitializeServices()
    {
        if (!servicesInitialized)
        {
            try
            {
                await UnityServices.InitializeAsync();
                await SignUpAnonymouslyAsync();
                servicesInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    async Task SignUpAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            lastSignInError = string.Empty;
        }
        catch (AuthenticationException e)
        {
            lastSignInError = e.Message;
        }
        catch (RequestFailedException e)
        {
            lastSignInError = e.Message;
        }
    }

    public async void SwitchProfileAsync()
    {
        Debug.Log("Switch profile!");
        try
        {
            AuthenticationService.Instance.SignOut();
            AuthenticationService.Instance.SwitchProfile("test_profile");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException e)
        {
            lastSignInError = e.Message;
        }
    }

}