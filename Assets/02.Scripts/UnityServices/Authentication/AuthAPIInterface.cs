using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthAPIInterface
{
    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        UnityServices.InitializeAsync();
    }

    async Task SignInAnonymousAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException e)
        {
            Debug.LogException(e);
        }
    }

    async Task SignInWithFacebook(string token)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithFacebookAsync(token);
        }
        catch (AuthenticationException e)
        {
            Debug.LogException(e);
        }
        catch (RequestFailedException e)
        {
            Debug.LogException(e);
        }
    }
    async Task SignInWithGoogle(string token)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGoogleAsync(token);
        }
        catch (AuthenticationException e)
        {
            Debug.LogException(e);
        }
        catch (RequestFailedException e)
        {
            Debug.LogException(e);
        }
    }

    async Task SignInWithSteamAsync(string ticket, string identity)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithSteamAsync(ticket, identity);
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
}
