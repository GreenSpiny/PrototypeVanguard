using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CardLoader : MonoBehaviour
{
    public static CardLoader instance = null;

    private const string dataVersionFilename = "dataVersion";
    private const string cardsDataFilename = "cardsData";
    private const string imageBundlePrefix = "cardImages/cardimages_";
    private const string r2endpoint = "https://vanguard-url-signer.akruchkow.workers.dev/";

    [SerializeField] private Material cardMaterial;
    [SerializeField] private Material defaultCardBackMaterial;

    // Card Loading
    [NonSerialized] public DataVersionObject dataVersionObject;
    [NonSerialized] public Dictionary<int, CardInfo> allCardsData = new Dictionary<int, CardInfo>();
    [NonSerialized] public Dictionary<int, AssetBundle> allBundles = new Dictionary<int, AssetBundle>();
    [NonSerialized] public Dictionary<int, AssetBundleRequest> allBundleRequests = new Dictionary<int, AssetBundleRequest>();
    [NonSerialized] public Dictionary<int, Material> allImagesData = new Dictionary<int, Material>();
    [NonSerialized] public List<CardInfo> allCardsDataSorted = new List<CardInfo>();
    [NonSerialized] public AvatarBank avatarBank;

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
    public uint oldVersionNumber = 0;
    private string errorText;

    public bool IsError { get { return !string.IsNullOrEmpty(errorText); } }

    private bool cardsLoaded;
    public static bool CardsLoaded { get { return CardLoader.instance != null && CardLoader.instance.cardsLoaded; } }

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

        avatarBank = GetComponent<AvatarBank>();

        // Set game quality settings
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
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
        // Reset progress

        cardsLoaded = false;
        versionDownloadProgress = 0;
        cardsDownloadProgress = 0;
        imageDownloadProgress = 0;
        allCardsData.Clear();
        allImagesData.Clear();

        // Download the Data Version object. This determines whether a mew cards JSON file and new image asset bundles should be downloaded.

        versionDownloadProgress = 0;
        DataVersionObject oldDataVersionObject;
        if (downloadMode == DownloadMode.remoteDownload)
        {
            oldDataVersionObject = SaveDataManager.LoadVersionJSON();

            var webRequest = UnityWebRequest.Get(r2endpoint + dataVersionFilename + ".json");
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                dataVersionObject = oldDataVersionObject;
                errorText = "Error downloading version file: " + webRequest.error;
                Debug.LogError(errorText);
            }
            else
            {
                string text = webRequest.downloadHandler.text;
                dataVersionObject = DataVersionObject.FromJSON(text);
                Debug.Log("Acquired version file from remote.");
            }
        }
        else
        {
            TextAsset versionJSON = Resources.Load<TextAsset>("JSON/" + dataVersionFilename);
            oldDataVersionObject = DataVersionObject.FromJSON(versionJSON.text);
            dataVersionObject = oldDataVersionObject;
        }
        oldVersionNumber = oldDataVersionObject.cardsFileVersion;
        versionDownloadProgress = 1;

        // Download the Cards Data object if a new one is available.

        cardsDownloadProgress = 0;
        string oldCardsText;
        string newCardsText;
        if (downloadMode == DownloadMode.remoteDownload)
        {
            bool shouldUpdateCards = dataVersionObject.cardsFileVersion > oldDataVersionObject.cardsFileVersion;
            oldCardsText = SaveDataManager.LoadCardsJSON();
            if (shouldUpdateCards)
            {
                var webRequest = UnityWebRequest.Get(r2endpoint + cardsDataFilename + ".json");
                webRequest.SetRequestHeader("Content-Type", "application/json");
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    newCardsText = oldCardsText;
                    errorText = "Error downloading cards file: " + webRequest.error;
                    Debug.LogError(errorText);
                }
                else
                {
                    string text = webRequest.downloadHandler.text;
                    newCardsText = text;
                    Debug.Log("Acquired new cards file from remote.");
                }
            }
            else
            {
                newCardsText = oldCardsText;
                Debug.Log("No need to update cards file.");
            }
        }
        else
        {
            TextAsset allCardsJSON = Resources.Load<TextAsset>("JSON/" + cardsDataFilename);
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

        // Grab the existing card data JSON, or download an updated version if needed.
        var parsedCardsFile = JsonConvert.DeserializeObject<Dictionary<string, object>>(newCardsText);
        var parsedCards = ((JObject)parsedCardsFile["cards"]).ToObject<Dictionary<string, object>>();

        foreach (JObject card in parsedCards.Values)
        {
            Dictionary<string, object> cardData = card.ToObject<Dictionary<string, object>>();
            CardInfo newEntry = CardInfo.FromDictionary(cardData);

            if (newEntry.regulation == CardInfo.invalidRegulation)
            {
                continue;
            }

            allCardsData[newEntry.index] = newEntry;
            allCardsDataSorted.Add(newEntry);

            gradeSet.Add(newEntry.grade);
            if (!string.IsNullOrEmpty(newEntry.gift))
                giftSet.Add(newEntry.gift);
            if (!string.IsNullOrEmpty(newEntry.group))
                groupSet.Add(newEntry.group);
            foreach (string nation in newEntry.nation)
                nationSet.Add(nation);
            foreach (string race in newEntry.race)
                raceSet.Add(race);
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
            // If no errors occurred, save the most up-to-date data files.
            if (!IsError)
            {
                SaveDataManager.SaveVersionJSON(dataVersionObject);
                SaveDataManager.SaveCardsJSON(newCardsText);
            }
        }
        else
        {
            cardsLoaded = true;
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
                    string cardBundleUrl = r2endpoint + imageBundlePrefix + i.ToString();
                    downloadHandlers[i] = new RemoteDownloadHandlerObject(i, cardBundleUrl, dataversion.imageBundleVersions[i]);
                    currentDownloads++;
                }
            }
        }

        cardsLoaded = true;
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

    public static CardInfo GetCardInfoCopy(int cardIndex)
    {
        if (instance == null || !instance.allCardsData.ContainsKey(cardIndex))
        {
            return new CardInfo();
        }
        return instance.allCardsData[cardIndex].Copy();
    }

    public static CardInfo GetCardInfo(int cardIndex)
    {
        if (instance == null || !instance.allCardsData.ContainsKey(cardIndex))
        {
            return new CardInfo();
        }
        return instance.allCardsData[cardIndex];
    }

    public static Material GetCardImage(int cardIndex)
    {
        if (instance == null)
        {
            return new Material(instance.defaultCardBackMaterial);
        }
        else if (instance.allImagesData.ContainsKey(cardIndex))
        {
            return new Material(instance.allImagesData[cardIndex]);
        }
        // If we are not downloading remote assets, load the assets from the Resources folder.
        else if (instance.downloadMode == DownloadMode.localResources)
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
                Debug.LogError("Failed to load card with index: " + cardIndex.ToString());
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
        public readonly int index;
        public readonly string url;
        public readonly uint version;
        public UnityWebRequest webRequest;
        public AssetBundleRequest bundleRequest;
        public bool completed = false;
        public string error;

        public RemoteDownloadHandlerObject(int index, string url, uint version)
        {
            this.index = index;
            this.url = url;
            this.version = version;
            instance.StartCoroutine(DownloadAndExtractAsync());
        }

        public IEnumerator DownloadAndExtractAsync()
        {
            webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, version, 0);
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                instance.errorText = "Error downloading image bundle number " + index.ToString() + ": " + webRequest.error;
                Debug.LogError(instance.errorText);
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
                bundle.UnloadAsync(false);
            }
            completed = true;
        }
    }

}
