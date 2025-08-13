using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using static CardLoader;
using static System.Net.WebRequestMethods;

public class GetFileFromCDN : MonoBehaviour
{
    void Start()
    {
        string dataVersionEndpoint = "https://vanguard-url-signer.akruchkow.workers.dev/dataVersion.json";
        StartCoroutine(GetVersionTest(dataVersionEndpoint));
    }

    IEnumerator GetVersionTest(string apiEndpoint)
    {
        var webRequest = UnityWebRequest.Get(apiEndpoint);
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("error: " + webRequest.error);
        }
        else
        {
            string text = webRequest.downloadHandler.text;
            Debug.Log(text);
        }
    }

    // Not used, but good for reference
    private string CreateBasicAuthString(string username, string password)
    {
        string auth = username + ":" + password;
        string base64 = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(auth));
        return "Basic " + base64;
    }
}
