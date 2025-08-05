using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CardLoader : MonoBehaviour
{
    public static CardLoader instance = null;
    private const string localVersionJSONPath = "JSON/dataVersion";
    private const string localCardsJSONPath = "JSON/allCardsSingleton";

    [SerializeField] private Material cardMaterial;
    [SerializeField] private Material defaultCardBackMaterial;

    // Card Loading
    private DataVersionObject versionObject;
    public Dictionary<int, CardInfo> allCardsData = new Dictionary<int, CardInfo>();
    public Dictionary<int, AssetBundle> allBundles = new Dictionary<int, AssetBundle>();
    public Dictionary<int, AssetBundleRequest> allBundleRequests = new Dictionary<int, AssetBundleRequest>();
    public Dictionary<int, Material> allImagesData = new Dictionary<int, Material>();

    // Card Parameter Tracking
    public List<string> allCardGifts;
    public List<int> allCardGrades;
    public List<string> allCardGroups;
    public List<string> allCardNations;
    public List<string> allCardRaces;
    public List<string> allCardUnitTypes;

    private enum DownloadMode { localResources = 0, localAsync = 1, remoteDownload = 2 }
    [SerializeField] DownloadMode downloadMode;
    public bool CardsLoaded { get; private set; }

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
        allCardsData.Clear();
        allImagesData.Clear();

        Debug.Log("JSON download initiated.");

        // Download the Data Version Object. This determines whether mew asset bundles should be downloaded.
        // It must be checked against the locally saved copy.
        TextAsset versionJSON = Resources.Load<TextAsset>(localVersionJSONPath);
        versionObject = DataVersionObject.FromJSON(versionJSON.text);
        Debug.Log(versionObject.cardsFileVersion);
        Debug.Log(versionObject.imageBundleVersions.Length);

        // Track all data types, for deckbuilder sorting purposes
        HashSet<string> giftSet = new HashSet<string>();
        HashSet<int> gradeSet = new HashSet<int>();
        HashSet<string> groupSet = new HashSet<string>();
        HashSet<string> nationSet = new HashSet<string>();
        HashSet<string> raceSet = new HashSet<string>();
        HashSet<string> unitTypeSet = new HashSet<string>();

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

            if (!string.IsNullOrEmpty(newEntry.gift))
                giftSet.Add(newEntry.gift);
            gradeSet.Add(newEntry.grade);
            if (!string.IsNullOrEmpty(newEntry.group))
                groupSet.Add(newEntry.group);
            if (!string.IsNullOrEmpty(newEntry.nation))
                nationSet.Add(newEntry.nation);
            if (!string.IsNullOrEmpty(newEntry.race))
                raceSet.Add(newEntry.race);
            if (!string.IsNullOrEmpty(newEntry.unitType))
                unitTypeSet.Add(newEntry.unitType);
        }

        allCardGifts = new List<string>(giftSet);
        allCardGifts.Sort();
        allCardGrades = new List<int>(gradeSet);
        allCardGrades.Sort();
        allCardGroups = new List<string>(groupSet);
        allCardGroups.Sort();
        allCardNations = new List<string>(nationSet);
        allCardNations.Sort();
        allCardRaces = new List<string>(raceSet);
        allCardRaces.Sort();
        allCardUnitTypes = new List<string>(unitTypeSet);
        allCardUnitTypes.Sort();

        Debug.Log("JSON download complete.");

        yield return null;

        if (downloadMode == DownloadMode.remoteDownload)
        {
            StartCoroutine(DownloadAllBundlesAsync(versionObject, 5));
            while (!CardsLoaded)
            {
                yield return null;
            }
        }
        else if (downloadMode == DownloadMode.localAsync)
        {
            Debug.Log("Texture download initiated.");

            // Download the card image assets.
            int bundleCount = versionObject.imageBundleVersions.Count();
            for (int i = 0; i < bundleCount; i++)
            {
                Debug.Log("Downloading texture bundle " + (i + 1).ToString() + " of " + bundleCount.ToString());
                var bundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "AssetBundles", "cardimages", i.ToString()));
                yield return bundleLoadRequest;
                var bundle = bundleLoadRequest.assetBundle;
                AssetBundleRequest req = bundle.LoadAllAssetsAsync();
                instance.allBundles[i] = bundle;
                instance.allBundleRequests[i] = req;
            }

            Debug.Log("Texture download complete.");

            while (allBundleRequests.Count > 0)
            {
                yield return null;
                foreach (KeyValuePair<int, AssetBundleRequest> kvp in allBundleRequests)
                {
                    if (kvp.Value.isDone)
                    {
                        foreach (object asset in kvp.Value.allAssets)
                        {
                            Texture t = asset as Texture;
                            Material mat = new Material(cardMaterial);
                            mat.mainTexture = t;
                            allImagesData[Convert.ToInt32(t.name)] = mat;
                        }
                        allBundleRequests.Remove(kvp.Key);
                        allBundles[kvp.Key].UnloadAsync(false);
                        break;
                    }
                }
            }

            CardsLoaded = true;
            Debug.Log("Texture extraction complete.");
        }
        else
        {
            CardsLoaded = true;
        }
    }

    public IEnumerator DownloadAllBundlesAsync(DataVersionObject dataversion, int maxConcurrentDownloads)
    {
        Debug.Log("Async download & extraction initiated.");

        List<DownloadHandlerObject> downloadHandlers = new List<DownloadHandlerObject>();
        for (int i = 0; i < dataversion.imageBundleVersions.Length; i++)
        {
            downloadHandlers.Add(null);
        }

        while (downloadHandlers.Count > 0)
        {
            yield return null;
            int currentDownloads = 0;
            for (int i = downloadHandlers.Count - 1; i >= 0; i--)
            {
                if (downloadHandlers[i] != null)
                {
                    if (downloadHandlers[i].completed)
                    {
                        downloadHandlers.RemoveAt(i);
                    }
                    else
                    {
                        currentDownloads++;
                    }
                }
                else if (currentDownloads < maxConcurrentDownloads)
                {
                    string url = "http://localhost:8000/CardImages/" + i.ToString();
                    downloadHandlers[i] = new DownloadHandlerObject(url, 0);
                    currentDownloads++;
                    Debug.Log("Initiating download: " + i.ToString());
                }
            }
        }

        CardsLoaded = true;
        Debug.Log("Async download & extraction completed.");
    }

    public static Material GetDefaultCardBack()
    {
        if (instance == null)
        {
            return null;
        }
        return instance.defaultCardBackMaterial;
    }

    public static CardInfo GetCardInfo(int cardIndex)
    {
        if (instance == null || !instance.allCardsData.ContainsKey(cardIndex))
        {
            return null;
        }
        return instance.allCardsData[cardIndex];
    }

    // TODO: Handle asynchronous version
    public static Material GetCardImage(int cardIndex)
    {
        if (instance == null)
        {
            return new Material(instance.defaultCardBackMaterial);
        }
        if (instance.allImagesData.ContainsKey(cardIndex))
        {
            return new Material(instance.allImagesData[cardIndex]);
        }
        else if (Application.isEditor && instance.downloadMode == DownloadMode.localResources)
        {
            // If we are not downloading remote assets, load the assets from the Resources folder.
            // This is mainly for fast editor testing.
            Material newMaterial = new Material(instance.cardMaterial);
            int folderIndex = Mathf.FloorToInt(cardIndex / 100f);
            Texture targetTexture = Resources.Load<Texture>("cardimages/" + folderIndex +  '/' + cardIndex.ToString());
            if (targetTexture != null)
            {
                newMaterial.mainTexture = targetTexture;
            }
            else
            {
                Debug.Log("Failed to load card with index: " + cardIndex.ToString());
            }
            instance.allImagesData[cardIndex] = newMaterial;
            return newMaterial;
        }
        return new Material(instance.defaultCardBackMaterial);
    }

    [System.Serializable]
    public class DataVersionObject
    {
        public int cardsFileVersion = 0;
        public int[] imageBundleVersions = new int[0];

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static DataVersionObject FromJSON(string json)
        {
            return JsonConvert.DeserializeObject<DataVersionObject>(json);
        }
    }

    public class DownloadHandlerObject
    {
        public readonly string url;
        public readonly uint version;
        public UnityWebRequest webRequest;
        public AssetBundleRequest bundleRequest;
        public bool completed = false;
        public string error;

        public DownloadHandlerObject(string url, uint version)
        {
            this.url = url;
            this.version = version;
            instance.StartCoroutine(DownloadAndExtractAsync());
        }

        public IEnumerator DownloadAndExtractAsync()
        {
            webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("error: " + webRequest.error);
                error = webRequest.error;
            }
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(webRequest);
                bundleRequest = bundle.LoadAllAssetsAsync();
                while (!bundleRequest.isDone)
                {
                    yield return null;
                }
                foreach (object asset in bundleRequest.allAssets)
                {
                    Texture t = asset as Texture;
                    Material mat = new Material(instance.cardMaterial);
                    mat.mainTexture = t;
                    instance.allImagesData[Convert.ToInt32(t.name)] = mat;
                }
            }
            completed = true;
        }
    }

}
