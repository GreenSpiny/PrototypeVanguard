using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
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
    [NonSerialized] public DataVersionObject dataVersionObject;
    [NonSerialized] public Dictionary<int, CardInfo> allCardsData = new Dictionary<int, CardInfo>();
    [NonSerialized] public Dictionary<int, AssetBundle> allBundles = new Dictionary<int, AssetBundle>();
    [NonSerialized] public Dictionary<int, AssetBundleRequest> allBundleRequests = new Dictionary<int, AssetBundleRequest>();
    [NonSerialized] public Dictionary<int, Material> allImagesData = new Dictionary<int, Material>();
    [NonSerialized] public List<CardInfo> allCardsDataSorted = new List<CardInfo>();

    // Card Parameter Tracking
    [NonSerialized] public List<string> allCardGifts;
    [NonSerialized] public List<int> allCardGrades;
    [NonSerialized] public List<string> allCardGroups;
    [NonSerialized] public List<string> allCardNations;
    [NonSerialized] public List<string> allCardRaces;
    [NonSerialized] public List<string> allCardUnitTypes;

    public enum DownloadMode { localResources = 0, remoteDownload = 1 }
    [SerializeField] public DownloadMode downloadMode;

    public int versionDownloadProgress = 0;
    public int cardsDownloadProgress = 0;
    public float imageDownloadProgress = 0;

    public bool CardsLoaded { get; private set; } = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            enabled = false;
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

        // Download the Data Version object. This determines whether mew cards JSON and image asset bundles should be downloaded.

        versionDownloadProgress = 0;
        DataVersionObject oldDataVersionObject;
        if (downloadMode == DownloadMode.remoteDownload)
        {
            oldDataVersionObject = SaveDataManager.LoadVersionJSON();
            string url = "http://localhost:8000/dataVersion.json";
            var webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("error: " + webRequest.error);
                dataVersionObject = oldDataVersionObject;
            }
            else
            {
                string text = webRequest.downloadHandler.text;
                dataVersionObject = DataVersionObject.FromJSON(text);
            }
        }
        else
        {
            TextAsset versionJSON = Resources.Load<TextAsset>(localVersionJSONPath);
            oldDataVersionObject = DataVersionObject.FromJSON(versionJSON.text);
            dataVersionObject = oldDataVersionObject;
        }
        versionDownloadProgress = 1;
        Debug.Log("Data Version: " + oldDataVersionObject.cardsFileVersion.ToString() + " -> " + dataVersionObject.cardsFileVersion.ToString());

        // Download the Cards Data object, updating the existing file if necessary.

        cardsDownloadProgress = 0;
        string oldCardsText;
        string newCardsText;
        if (downloadMode == DownloadMode.remoteDownload)
        {
            bool shouldUpdateCards = dataVersionObject.cardsFileVersion > oldDataVersionObject.cardsFileVersion;
            oldCardsText = SaveDataManager.LoadCardsJSON();
            if (shouldUpdateCards)
            {
                string url = "http://localhost:8000/allCardsSingleton.json";
                var webRequest = UnityWebRequest.Get(url);
                yield return webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("error: " + webRequest.error);
                    newCardsText = oldCardsText;
                }
                else
                {
                    string text = webRequest.downloadHandler.text;
                    newCardsText = text;
                    SaveDataManager.SaveCardsJSON(newCardsText);
                }
            }
            else
            {
                newCardsText = oldCardsText;
            }
        }
        else
        {
            TextAsset allCardsJSON = Resources.Load<TextAsset>(localCardsJSONPath);
            oldCardsText = allCardsJSON.text;
            newCardsText = oldCardsText;
        }
        cardsDownloadProgress = 1;

        // Track all data types, for deckbuilder sorting purposes
        HashSet<string> giftSet = new HashSet<string>();
        HashSet<int> gradeSet = new HashSet<int>();
        HashSet<string> groupSet = new HashSet<string>();
        HashSet<string> nationSet = new HashSet<string>();
        HashSet<string> raceSet = new HashSet<string>();
        HashSet<string> unitTypeSet = new HashSet<string>();

        // Grab the existing card data JSON, or download an updated version if needed.\
        var parsedCards = JsonConvert.DeserializeObject<Dictionary<string, object>>(newCardsText);

        foreach (JObject card in parsedCards.Values)
        {
            Dictionary<string, object> cardData = card.ToObject<Dictionary<string, object>>();
            CardInfo newEntry = CardInfo.FromDictionary(cardData);
            allCardsData[newEntry.index] = newEntry;
            allCardsDataSorted.Add(newEntry);

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

        allCardsDataSorted.Sort();

        Debug.Log("JSON download complete.");

        yield return null;

        // Download all card images asynchronously.
        if (downloadMode == DownloadMode.remoteDownload)
        {
            StartCoroutine(DownloadAllBundlesAsync(dataVersionObject, 5));
            while (!CardsLoaded)
            {
                yield return null;
            }
        }
        else
        {
            CardsLoaded = true;
        }
    }

    public IEnumerator DownloadAllBundlesAsync(DataVersionObject dataversion, int maxConcurrentDownloads)
    {
        Debug.Log("Remote image download & extraction initiated.");

        imageDownloadProgress = 0;
        List<RemoteDownloadHandlerObject> downloadHandlers = new List<RemoteDownloadHandlerObject>();
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
                        imageDownloadProgress = 1f - ((float)downloadHandlers.Count / dataversion.imageBundleVersions.Length);
                    }
                    else
                    {
                        currentDownloads++;
                    }
                }
                else if (currentDownloads < maxConcurrentDownloads)
                {
                    string url = "http://localhost:8000/CardImages/" + i.ToString();
                    downloadHandlers[i] = new RemoteDownloadHandlerObject(url, dataversion.imageBundleVersions[i]);
                    currentDownloads++;
                }
            }
        }

        CardsLoaded = true;
        Debug.Log("Remote image download & extraction completed.");
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
        // If we are not downloading remote assets, load the assets from the Resources folder.
        else if (Application.isEditor && instance.downloadMode == DownloadMode.localResources)
        {
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
        public uint cardsFileVersion = 0;
        public uint[] imageBundleVersions = new uint[0];

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static DataVersionObject FromJSON(string json)
        {
            return JsonConvert.DeserializeObject<DataVersionObject>(json);
        }
    }

    public class RemoteDownloadHandlerObject
    {
        public readonly string url;
        public readonly uint version;
        public UnityWebRequest webRequest;
        public AssetBundleRequest bundleRequest;
        public bool completed = false;
        public string error;

        public RemoteDownloadHandlerObject(string url, uint version)
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
