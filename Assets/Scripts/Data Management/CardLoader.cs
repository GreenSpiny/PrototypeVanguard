using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CardLoader : MonoBehaviour
{
    public static CardLoader instance = null;
    private const string localVersionJSONPath = "JSON/dataVersion";
    private const string localCardsJSONPath = "JSON/allCardsSingleton";
    private const string localCardImagesPath = "CardImages";

    [SerializeField] Material cardMaterial;

    // Card Loading
    private DataVersionObject versionObject;
    private Dictionary<int, CardInfo> allCardsData = new Dictionary<int, CardInfo>();
    public Dictionary<int, AssetBundle> allImagesBundles = new Dictionary<int, AssetBundle>();
    public Dictionary<int, Material> allImagesData = new Dictionary<int, Material>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void Start()
    {
        if (instance == this)
        {
            StartCoroutine(Initialize());
        }
    }

    public IEnumerator Initialize()
    {
        Debug.Log("Initializing asset download...");

        allCardsData.Clear();
        allImagesData.Clear();

        // Download the Data Version Object. This determines whether mew asset bundles must be downloaded.
        // It must be checked against the locally saved copy.
        TextAsset versionJSON = Resources.Load<TextAsset>(localVersionJSONPath);
        Debug.Log(versionJSON.text);
        versionObject = JsonConvert.DeserializeObject<DataVersionObject>(versionJSON.text);

        // Grab the existing card data JSON, or download an updated version if needed.
        TextAsset allCardsJSON = Resources.Load<TextAsset>(localCardsJSONPath);
        var parsedJSON = JsonConvert.DeserializeObject<IDictionary<string, object>>(allCardsJSON.text);
        JObject token = parsedJSON["cards"] as JObject;
        Dictionary<string, JObject> parsedCards = token.ToObject<Dictionary<string, JObject>>();
        foreach (JObject card in parsedCards.Values)
        {
            Dictionary<string, object> cardData = card.ToObject<Dictionary<string, object>>();
            CardInfo newEntry = CardInfo.FromDictionary(cardData);
            allCardsData[newEntry.index] = newEntry;
        }

        // Download the card image assets.
        int bundleCount = versionObject.imageBundleVersions.Count();
        for (int i = 0; i < bundleCount; i++)
        {
            Debug.Log("Downloading texture bundle " + (i+1).ToString() + " of " + bundleCount.ToString());
            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "AssetBundles", "cardimages", i.ToString()));
            yield return bundleLoadRequest;
            var bundle = bundleLoadRequest.assetBundle;
            instance.allImagesBundles[i] = bundle;
        }
        Debug.Log("Asset download complete.");
    }

    public static CardInfo GetCardInfo(int cardIndex)
    {
        if (instance == null || !instance.allCardsData.ContainsKey(cardIndex))
        {
            return null;
        }
        return instance.allCardsData[cardIndex];
    }

    // Synchronous version
    public static Material GetCardImage(int cardIndex)
    {
        if (instance == null)
        {
            return null;
        }
        if (instance.allImagesData.ContainsKey(cardIndex))
        {
            return instance.allImagesData[cardIndex];
        }
        else
        {
            Material newMaterial = new Material(instance.cardMaterial);
            int folderIndex = Mathf.FloorToInt(cardIndex / 100f);
            if (folderIndex < instance.allImagesBundles.Count)
            {
                AssetBundle targetBundle = instance.allImagesBundles[folderIndex];
                if (targetBundle != null)
                {
                    Texture desiredTexture = targetBundle.LoadAsset<Texture>(cardIndex.ToString());
                    
                    newMaterial.mainTexture = desiredTexture;
                }
            }
            instance.allImagesData[cardIndex] = newMaterial;
            return newMaterial;
        }
    }

    [System.Serializable]
    private class DataVersionObject
    {
        public int cardsFileVersion;
        public int[] imageBundleVersions;

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
