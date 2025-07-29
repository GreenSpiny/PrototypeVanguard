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

    [SerializeField] bool loadRemoteAssets;
    [SerializeField] Material cardMaterial;

    // Card Loading
    private DataVersionObject versionObject;
    private Dictionary<int, CardInfo> allCardsData = new Dictionary<int, CardInfo>();
    public Dictionary<int, AssetBundle> allBundles = new Dictionary<int, AssetBundle>();
    public Dictionary<int, AssetBundleRequest> allBundleRequests = new Dictionary<int, AssetBundleRequest>();
    public Dictionary<int, Material> allImagesData = new Dictionary<int, Material>();

    // Card Parameter Tracking
    List<string> allCardGifts = new List<string>();
    List<int> allCardGrades = new List<int>();
    List<string> allCardGroups = new List<string>();
    List<string> allCardNations = new List<string>();
    List<string> allCardRaces = new List<string>();
    List<string> allCardUnitTypes = new List<string>();

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

            giftSet.Add(newEntry.gift);
            gradeSet.Add(newEntry.grade);
            groupSet.Add(newEntry.group);
            nationSet.Add(newEntry.nation);
            raceSet.Add(newEntry.race);
            unitTypeSet.Add(newEntry.unitType);
        }

        allCardGifts = new List<string>(giftSet);
        allCardGrades = new List<int>(gradeSet);
        allCardGroups = new List<string>(giftSet);
        allCardNations = new List<string>(nationSet);
        allCardRaces = new List<string>(raceSet);
        allCardUnitTypes = new List<string>(unitTypeSet);

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

    public static CardInfo GetCardInfo(int cardIndex)
    {
        if (instance == null || !instance.allCardsData.ContainsKey(cardIndex))
        {
            return null;
        }
        return instance.allCardsData[cardIndex];
    }

    // TODO: handle asynchronous version
    public static Material GetCardImage(int cardIndex)
    {
        if (instance == null)
        {
            return null;
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
        return null;
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
