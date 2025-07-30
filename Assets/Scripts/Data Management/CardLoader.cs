using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardLoader : MonoBehaviour
{
    public static CardLoader instance = null;
    private const string localVersionJSONPath = "JSON/dataVersion";
    private const string localCardsJSONPath = "JSON/allCardsSingleton";

    [SerializeField] bool loadRemoteAssets;
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

    public bool CardsLoaded { get; private set; }
    public bool JSONLoaded { get; private set; }

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

        // Download the Data Version Object. This determines whether mew asset bundles must be downloaded.
        // It must be checked against the locally saved copy.
        TextAsset versionJSON = Resources.Load<TextAsset>(localVersionJSONPath);
        versionObject = JsonConvert.DeserializeObject<DataVersionObject>(versionJSON.text);

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

        JSONLoaded = true;

        Debug.Log("JSON download complete.");

        yield return null;

        if (loadRemoteAssets)
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
        else if (!instance.loadRemoteAssets)
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
